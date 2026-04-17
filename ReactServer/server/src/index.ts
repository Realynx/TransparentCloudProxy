import fs from 'node:fs'
import path from 'node:path'
import { randomUUID } from 'node:crypto'

import cors from 'cors'
import dotenv from 'dotenv'
import express from 'express'
import morgan from 'morgan'
import { z } from 'zod'

dotenv.config()

const app = express()
const port = Number(process.env.PORT ?? 4000)
const clientOrigin = process.env.CLIENT_ORIGIN ?? 'http://localhost:5173'

app.use(cors({ origin: clientOrigin }))
app.use(express.json())
app.use(morgan('dev'))

type SavedCredential = {
  id: string
  displayName: string
  credential: string
  reachableAddress: string
  sourceOneKeySuffix: string
  createdAt: string
  updatedAt: string
}

type OneKeyStore = {
  savedCredentials: SavedCredential[]
}

type ProxyServer = {
  id: string
  name: string
  host: string
  createdAt: string
  updatedAt: string
}

type NatRule = {
  id: string
  name: string
  ports: number[]
  addresses: string[]
  targetMode: 'all' | 'selected'
  serverIds: string[]
  enabled: boolean
  createdAt: string
  updatedAt: string
}

type ProxyRulesStore = {
  proxyServers: ProxyServer[]
  natRules: NatRule[]
}

type OneKeyImportResult = {
  created: SavedCredential[]
  skipped: string[]
  attemptedAddresses: string[]
  unreachableAddresses: string[]
  connectedAddress: string | null
  createdAt: string
}

type AddressProbeResult = {
  address: string
  ok: boolean
  status?: number
  error?: string
}

const nowIso = (): string => new Date().toISOString()

const createOneKeySchema = z.object({
  oneKey: z
    .string()
    .trim()
    .min(65, 'OneKey must contain at least a 64-char credential and encoded addresses'),
  displayName: z.string().trim().min(2).max(80).default('Remote Proxy'),
})

const createProxyServerSchema = z.object({
  name: z.string().trim().min(2).max(80),
  host: z.string().trim().min(3).max(120),
})

const upsertNatRuleSchema = z
  .object({
    name: z.string().trim().min(2).max(80),
    ports: z.array(z.coerce.number().int().min(1).max(65535)).min(1).max(30),
    addresses: z.array(z.string().trim().min(1).max(120)).min(1).max(30),
    targetMode: z.enum(['all', 'selected']),
    serverIds: z.array(z.string().uuid()).default([]),
    enabled: z.boolean().default(true),
  })
  .superRefine((value, ctx) => {
    if (value.targetMode === 'selected' && value.serverIds.length === 0) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        path: ['serverIds'],
        message: 'Select at least one server when target mode is selected',
      })
    }
  })

const probeTimeoutMs = Number(process.env.ONEKEY_PROBE_TIMEOUT_MS ?? 3000)
const oneKeyFallbackPorts = [8080, 7002, 7001]

const oneKeyPath = path.resolve(process.cwd(), 'server', 'data', 'onekeys.json')
const proxyRulesPath = path.resolve(process.cwd(), 'server', 'data', 'proxy-rules.json')

let store: OneKeyStore = { savedCredentials: [] }
let proxyRulesStore: ProxyRulesStore = {
  proxyServers: [],
  natRules: [],
}

const normalizeReachableAddress = (input: string): string => {
  let trimmed = input.trim()
  if (!/^https?:\/\//i.test(trimmed)) {
    trimmed = `https://${trimmed}`
  }

  // OneKey payloads can include unbracketed IPv6 literals with optional zone IDs
  // (for example: https://fe80::be24:11ff:fe57:4d8d%2:45419). URL parsing requires
  // bracketed hosts and percent-escaped zone delimiters.
  const schemeMatch = trimmed.match(/^(https?):\/\/(.*)$/i)
  if (schemeMatch) {
    const scheme = schemeMatch[1].toLowerCase()
    const remainder = schemeMatch[2]
    const pathStart = remainder.search(/[/?#]/)
    const authority = pathStart === -1 ? remainder : remainder.slice(0, pathStart)
    const suffix = pathStart === -1 ? '' : remainder.slice(pathStart)

    if (authority.includes(':') && !authority.startsWith('[')) {
      let hostPart = authority
      let portPart = ''

      const portMatch = authority.match(/^(.*):(\d+)$/)
      if (portMatch) {
        hostPart = portMatch[1]
        portPart = portMatch[2]
      }

      if (hostPart.includes(':')) {
        const escapedZoneHost = hostPart.replace(/%/g, '%25')
        const bracketedHost = `[${escapedZoneHost}]${portPart ? `:${portPart}` : ''}`
        trimmed = `${scheme}://${bracketedHost}${suffix}`
      }
    }
  }

  let normalized: URL

  try {
    normalized = new URL(trimmed)
  } catch {
    throw new Error(`Invalid reachable address in OneKey: ${input}`)
  }

  if (!normalized.pathname || normalized.pathname === '') {
    normalized.pathname = '/'
  } else if (!normalized.pathname.endsWith('/')) {
    normalized.pathname = `${normalized.pathname}/`
  }

  return normalized.toString()
}

const expandOneKeyAddressCandidates = (input: string): string[] => {
  const normalizedAddress = normalizeReachableAddress(input)
  const parsed = new URL(normalizedAddress)

  const candidates: string[] = [parsed.toString()]
  if (parsed.port) {
    return candidates
  }

  const preferredSchemes = [parsed.protocol.slice(0, -1), parsed.protocol === 'https:' ? 'http' : 'https']

  for (const scheme of [...new Set(preferredSchemes)]) {
    for (const fallbackPort of oneKeyFallbackPorts) {
      const candidate = new URL(parsed.toString())
      candidate.protocol = `${scheme}:`
      candidate.port = `${fallbackPort}`
      candidates.push(candidate.toString())
    }
  }

  return [...new Set(candidates)]
}

const decodeOneKey = (oneKey: string): { credential: string; addresses: string[] } => {
  const trimmed = oneKey.trim()
  const credential = trimmed.slice(0, 64)
  const addressHex = trimmed.slice(64)

  if (!/^[A-Fa-f0-9]{64}$/.test(credential)) {
    throw new Error('OneKey credential prefix must be a 64-character hex string')
  }

  if (!/^[A-Fa-f0-9]+$/.test(addressHex) || addressHex.length % 2 !== 0) {
    throw new Error('OneKey address payload is not valid hex')
  }

  const addressesString = Buffer.from(addressHex, 'hex').toString('utf8')
  const matches = [...addressesString.matchAll(/(https?:\/\/.+?)(?=https?:\/\/|$)/g)]
  const addresses: string[] = []

  for (const entry of matches) {
    try {
      addresses.push(...expandOneKeyAddressCandidates(entry[1]))
    } catch {
      // Keep scanning OneKey candidates in order. OneKey reliability depends on
      // trying alternatives when a single address cannot be parsed/reached.
      continue
    }
  }

  if (addresses.length === 0) {
    throw new Error('No valid reachable addresses were found in OneKey')
  }

  return { credential: credential.toUpperCase(), addresses: [...new Set(addresses)] }
}

const normalizeRuleAddress = (input: string): string => {
  const normalized = input.trim().toLowerCase()
  if (normalized === '*' || normalized === 'all' || normalized === 'any') {
    return '0.0.0.0'
  }

  return normalized
}

const normalizeNatRulePayload = (payload: z.infer<typeof upsertNatRuleSchema>) => {
  const uniquePorts = [...new Set(payload.ports)].sort((a, b) => a - b)
  const uniqueAddresses = [...new Set(payload.addresses.map(normalizeRuleAddress))]
  const uniqueServerIds = [...new Set(payload.serverIds)]

  return {
    name: payload.name,
    ports: uniquePorts,
    addresses: uniqueAddresses,
    targetMode: payload.targetMode,
    serverIds: payload.targetMode === 'all' ? [] : uniqueServerIds,
    enabled: payload.enabled,
  }
}

const ensureStoreDirectory = async (): Promise<void> => {
  const directory = path.dirname(oneKeyPath)
  await fs.promises.mkdir(directory, { recursive: true })
}

const probeRemoteAddress = async (
  address: string,
  credential: string,
): Promise<AddressProbeResult> => {
  const controller = new AbortController()
  const timeout = setTimeout(() => {
    controller.abort()
  }, probeTimeoutMs)

  try {
    const endpoint = new URL('User/Get', address).toString()
    const response = await fetch(endpoint, {
      method: 'GET',
      headers: {
        Authorization: `Key ${credential}`,
        Accept: 'application/json',
      },
      signal: controller.signal,
    })

    return {
      address,
      ok: response.ok,
      status: response.status,
      error: response.ok ? undefined : `HTTP ${response.status}`,
    }
  } catch (error) {
    return {
      address,
      ok: false,
      error: error instanceof Error ? error.message : 'Unknown connectivity error',
    }
  } finally {
    clearTimeout(timeout)
  }
}

const findReachableAddress = async (
  addresses: string[],
  credential: string,
): Promise<{ selectedAddress: string | null; attempts: AddressProbeResult[] }> => {
  const attempts: AddressProbeResult[] = []

  for (const address of addresses) {
    const result = await probeRemoteAddress(address, credential)
    attempts.push(result)

    if (result.ok) {
      return { selectedAddress: address, attempts }
    }
  }

  return { selectedAddress: null, attempts }
}

const loadStore = async (): Promise<void> => {
  await ensureStoreDirectory()

  if (!fs.existsSync(oneKeyPath)) {
    await fs.promises.writeFile(oneKeyPath, JSON.stringify(store, null, 2), 'utf8')
    return
  }

  try {
    const fileContent = await fs.promises.readFile(oneKeyPath, 'utf8')
    const parsed = JSON.parse(fileContent) as Partial<OneKeyStore>
    store = {
      savedCredentials: Array.isArray(parsed.savedCredentials) ? parsed.savedCredentials : [],
    }
  } catch (error) {
    console.error('Failed loading OneKey store:', error)
    store = { savedCredentials: [] }
  }
}

const loadProxyRulesStore = async (): Promise<void> => {
  await ensureStoreDirectory()

  if (!fs.existsSync(proxyRulesPath)) {
    await fs.promises.writeFile(proxyRulesPath, JSON.stringify(proxyRulesStore, null, 2), 'utf8')
    return
  }

  try {
    const fileContent = await fs.promises.readFile(proxyRulesPath, 'utf8')
    const parsed = JSON.parse(fileContent) as Partial<ProxyRulesStore>

    proxyRulesStore = {
      proxyServers: Array.isArray(parsed.proxyServers) ? parsed.proxyServers : [],
      natRules: Array.isArray(parsed.natRules) ? parsed.natRules : [],
    }
  } catch (error) {
    console.error('Failed loading proxy-rules store:', error)
    proxyRulesStore = {
      proxyServers: [],
      natRules: [],
    }
  }
}

const persistStore = async (): Promise<void> => {
  await ensureStoreDirectory()
  await fs.promises.writeFile(oneKeyPath, JSON.stringify(store, null, 2), 'utf8')
}

const persistProxyRulesStore = async (): Promise<void> => {
  await ensureStoreDirectory()
  await fs.promises.writeFile(proxyRulesPath, JSON.stringify(proxyRulesStore, null, 2), 'utf8')
}

const buildOneKeyEntries = async (
  displayName: string,
  oneKey: string,
): Promise<OneKeyImportResult> => {
  const normalizedOneKey = oneKey.trim()
  const { credential, addresses } = decodeOneKey(oneKey)
  const dedupedAddresses = [...new Set(addresses)]

  const { selectedAddress, attempts } = await findReachableAddress(
    dedupedAddresses,
    credential,
  )

  const attemptedAddresses = attempts.map((entry) => entry.address)
  const unreachableAddresses = attempts
    .filter((entry) => !entry.ok)
    .map((entry) => entry.address)

  if (!selectedAddress) {
    return {
      created: [],
      skipped: [],
      attemptedAddresses,
      unreachableAddresses,
      connectedAddress: null,
      createdAt: nowIso(),
    }
  }

  const exists = store.savedCredentials.some(
    (entry) =>
      entry.credential === credential &&
      entry.reachableAddress.toLowerCase() === selectedAddress.toLowerCase(),
  )

  if (exists) {
    return {
      created: [],
      skipped: [selectedAddress],
      attemptedAddresses,
      unreachableAddresses,
      connectedAddress: selectedAddress,
      createdAt: nowIso(),
    }
  }

  const timestamp = nowIso()
  const created: SavedCredential[] = [
    {
      id: randomUUID(),
      displayName,
      credential,
      reachableAddress: selectedAddress,
      sourceOneKeySuffix: normalizedOneKey
        .slice(Math.max(0, normalizedOneKey.length - 12))
        .toUpperCase(),
      createdAt: timestamp,
      updatedAt: timestamp,
    },
  ]

  return {
    created,
    skipped: [],
    attemptedAddresses,
    unreachableAddresses,
    connectedAddress: selectedAddress,
    createdAt: nowIso(),
  }
}

app.get('/api/health', (_req, res) => {
  res.json({
    status: 'ok',
    uptimeSeconds: Math.round(process.uptime()),
    timestamp: nowIso(),
    credentialsStored: store.savedCredentials.length,
    natRulesStored: proxyRulesStore.natRules.length,
    proxyServersStored: proxyRulesStore.proxyServers.length,
  })
})

app.get('/api/proxy-rules/config', (_req, res) => {
  const servers = [...proxyRulesStore.proxyServers].sort((a, b) =>
    a.name.localeCompare(b.name),
  )

  const rules = [...proxyRulesStore.natRules].sort((a, b) =>
    b.updatedAt.localeCompare(a.updatedAt),
  )

  res.json({
    servers,
    rules,
  })
})

app.post('/api/proxy-servers', async (req, res) => {
  const parsed = createProxyServerSchema.safeParse(req.body)
  if (!parsed.success) {
    res.status(400).json({ error: parsed.error.flatten() })
    return
  }

  const duplicateHost = proxyRulesStore.proxyServers.some(
    (entry) => entry.host.toLowerCase() === parsed.data.host.toLowerCase(),
  )

  if (duplicateHost) {
    res.status(409).json({ error: 'A proxy server with this host already exists' })
    return
  }

  const timestamp = nowIso()
  const created: ProxyServer = {
    id: randomUUID(),
    name: parsed.data.name,
    host: parsed.data.host,
    createdAt: timestamp,
    updatedAt: timestamp,
  }

  proxyRulesStore.proxyServers.push(created)
  await persistProxyRulesStore()
  res.status(201).json(created)
})

app.delete('/api/proxy-servers/:id', async (req, res) => {
  const index = proxyRulesStore.proxyServers.findIndex((entry) => entry.id === req.params.id)
  if (index < 0) {
    res.status(404).json({ error: 'Proxy server not found' })
    return
  }

  const [removed] = proxyRulesStore.proxyServers.splice(index, 1)

  for (const rule of proxyRulesStore.natRules) {
    if (rule.targetMode !== 'selected') {
      continue
    }

    const before = rule.serverIds.length
    rule.serverIds = rule.serverIds.filter((id) => id !== removed.id)
    if (before !== rule.serverIds.length) {
      rule.updatedAt = nowIso()
      if (rule.serverIds.length === 0) {
        rule.enabled = false
      }
    }
  }

  await persistProxyRulesStore()
  res.json(removed)
})

app.post('/api/proxy-rules', async (req, res) => {
  const parsed = upsertNatRuleSchema.safeParse(req.body)
  if (!parsed.success) {
    res.status(400).json({ error: parsed.error.flatten() })
    return
  }

  const normalizedPayload = normalizeNatRulePayload(parsed.data)
  const availableServerIds = new Set(proxyRulesStore.proxyServers.map((entry) => entry.id))
  const missingServerIds = normalizedPayload.serverIds.filter((id) => !availableServerIds.has(id))

  if (missingServerIds.length > 0) {
    res.status(400).json({ error: 'One or more selected proxy servers no longer exist', missingServerIds })
    return
  }

  const timestamp = nowIso()
  const created: NatRule = {
    id: randomUUID(),
    ...normalizedPayload,
    createdAt: timestamp,
    updatedAt: timestamp,
  }

  proxyRulesStore.natRules.unshift(created)
  await persistProxyRulesStore()
  res.status(201).json(created)
})

app.put('/api/proxy-rules/:id', async (req, res) => {
  const parsed = upsertNatRuleSchema.safeParse(req.body)
  if (!parsed.success) {
    res.status(400).json({ error: parsed.error.flatten() })
    return
  }

  const index = proxyRulesStore.natRules.findIndex((entry) => entry.id === req.params.id)
  if (index < 0) {
    res.status(404).json({ error: 'NAT rule not found' })
    return
  }

  const normalizedPayload = normalizeNatRulePayload(parsed.data)
  const availableServerIds = new Set(proxyRulesStore.proxyServers.map((entry) => entry.id))
  const missingServerIds = normalizedPayload.serverIds.filter((id) => !availableServerIds.has(id))

  if (missingServerIds.length > 0) {
    res.status(400).json({ error: 'One or more selected proxy servers no longer exist', missingServerIds })
    return
  }

  const updated: NatRule = {
    ...proxyRulesStore.natRules[index],
    ...normalizedPayload,
    updatedAt: nowIso(),
  }

  proxyRulesStore.natRules[index] = updated
  await persistProxyRulesStore()
  res.json(updated)
})

app.delete('/api/proxy-rules/:id', async (req, res) => {
  const index = proxyRulesStore.natRules.findIndex((entry) => entry.id === req.params.id)
  if (index < 0) {
    res.status(404).json({ error: 'NAT rule not found' })
    return
  }

  const [removed] = proxyRulesStore.natRules.splice(index, 1)
  await persistProxyRulesStore()
  res.json(removed)
})

app.get('/api/credentials', (_req, res) => {
  const sorted = [...store.savedCredentials].sort((a, b) =>
    b.updatedAt.localeCompare(a.updatedAt),
  )

  res.json(sorted)
})

app.post('/api/credentials', async (req, res) => {
  const parsed = createOneKeySchema.safeParse(req.body)
  if (!parsed.success) {
    res.status(400).json({ error: parsed.error.flatten() })
    return
  }

  try {
    const importResult = await buildOneKeyEntries(parsed.data.displayName, parsed.data.oneKey)

    if (!importResult.connectedAddress) {
      res.status(502).json({
        error:
          'Unable to connect to any address embedded in this OneKey. Tried each address in order.',
        attemptedAddresses: importResult.attemptedAddresses,
        unreachableAddresses: importResult.unreachableAddresses,
      })
      return
    }

    if (importResult.created.length === 0) {
      res.status(409).json({
        error: 'The first reachable address for this OneKey is already stored',
        skipped: importResult.skipped,
        connectedAddress: importResult.connectedAddress,
        attemptedAddresses: importResult.attemptedAddresses,
      })
      return
    }

    store.savedCredentials.unshift(...importResult.created)
    await persistStore()

    res.status(201).json(importResult)
  } catch (error) {
    res.status(400).json({
      error: error instanceof Error ? error.message : 'Failed to decode OneKey',
    })
  }
})

app.delete('/api/credentials/:id', async (req, res) => {
  const index = store.savedCredentials.findIndex((entry) => entry.id === req.params.id)
  if (index < 0) {
    res.status(404).json({ error: 'Saved credential not found' })
    return
  }

  const [removed] = store.savedCredentials.splice(index, 1)
  await persistStore()
  res.json(removed)
})

const clientDist = path.resolve(process.cwd(), 'client/dist')
if (fs.existsSync(path.join(clientDist, 'index.html'))) {
  app.use(express.static(clientDist))

  app.get('/{*path}', (req, res, next) => {
    if (req.path.startsWith('/api/')) {
      next()
      return
    }

    res.sendFile(path.join(clientDist, 'index.html'))
  })
}

const start = async (): Promise<void> => {
  await loadStore()
  await loadProxyRulesStore()

  app.listen(port, () => {
    console.log(`Proxy admin API listening on http://localhost:${port}`)
  })
}

void start()
