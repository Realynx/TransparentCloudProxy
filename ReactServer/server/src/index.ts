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

type OneKeyImportResult = {
  created: SavedCredential[]
  skipped: string[]
  createdAt: string
}

const nowIso = (): string => new Date().toISOString()

const createOneKeySchema = z.object({
  oneKey: z
    .string()
    .trim()
    .min(65, 'OneKey must contain at least a 64-char credential and encoded addresses'),
  displayName: z.string().trim().min(2).max(80).default('Remote Proxy'),
})

const oneKeyPath = path.resolve(process.cwd(), 'server', 'data', 'onekeys.json')

let store: OneKeyStore = { savedCredentials: [] }

const normalizeReachableAddress = (input: string): string => {
  let trimmed = input.trim()
  if (!/^https?:\/\//i.test(trimmed)) {
    trimmed = `https://${trimmed}`
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
  const addresses = matches.map((entry) => normalizeReachableAddress(entry[1]))

  if (addresses.length === 0) {
    throw new Error('No reachable addresses were found in OneKey')
  }

  return { credential: credential.toUpperCase(), addresses }
}

const ensureStoreDirectory = async (): Promise<void> => {
  const directory = path.dirname(oneKeyPath)
  await fs.promises.mkdir(directory, { recursive: true })
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

const persistStore = async (): Promise<void> => {
  await ensureStoreDirectory()
  await fs.promises.writeFile(oneKeyPath, JSON.stringify(store, null, 2), 'utf8')
}

const buildOneKeyEntries = (
  displayName: string,
  oneKey: string,
): OneKeyImportResult => {
  const normalizedOneKey = oneKey.trim()
  const { credential, addresses } = decodeOneKey(oneKey)
  const dedupedAddresses = [...new Set(addresses)]

  const created: SavedCredential[] = []
  const skipped: string[] = []

  dedupedAddresses.forEach((reachableAddress, index) => {
    const exists = store.savedCredentials.some(
      (entry) =>
        entry.credential === credential &&
        entry.reachableAddress.toLowerCase() === reachableAddress.toLowerCase(),
    )

    if (exists) {
      skipped.push(reachableAddress)
      return
    }

    const timestamp = nowIso()
    const label = dedupedAddresses.length === 1 ? displayName : `${displayName} #${index + 1}`

    created.push({
      id: randomUUID(),
      displayName: label,
      credential,
      reachableAddress,
      sourceOneKeySuffix: normalizedOneKey
        .slice(Math.max(0, normalizedOneKey.length - 12))
        .toUpperCase(),
      createdAt: timestamp,
      updatedAt: timestamp,
    })
  })

  return {
    created,
    skipped,
    createdAt: nowIso(),
  }
}

app.get('/api/health', (_req, res) => {
  res.json({
    status: 'ok',
    uptimeSeconds: Math.round(process.uptime()),
    timestamp: nowIso(),
    credentialsStored: store.savedCredentials.length,
  })
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
    const importResult = buildOneKeyEntries(parsed.data.displayName, parsed.data.oneKey)
    if (importResult.created.length === 0) {
      res.status(409).json({
        error: 'All reachable addresses from this OneKey are already stored',
        skipped: importResult.skipped,
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

  app.listen(port, () => {
    console.log(`Proxy admin API listening on http://localhost:${port}`)
  })
}

void start()
