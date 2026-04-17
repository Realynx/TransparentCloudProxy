export type SavedCredential = {
  id: string
  displayName: string
  credential: string
  reachableAddress: string
  sourceOneKeySuffix: string
  createdAt: string
  updatedAt: string
}

export type SaveOneKeyDraft = {
  displayName: string
  oneKey: string
}

export type OneKeyImportResult = {
  created: SavedCredential[]
  skipped: string[]
  attemptedAddresses: string[]
  unreachableAddresses: string[]
  connectedAddress: string | null
  createdAt: string
}

export type Health = {
  status: string
  uptimeSeconds: number
  timestamp: string
  credentialsStored: number
  natRulesStored?: number
  proxyServersStored?: number
}

export type ProxyServer = {
  id: string
  name: string
  host: string
  createdAt: string
  updatedAt: string
}

export type NatRuleTargetMode = 'all' | 'selected'

export type NatRule = {
  id: string
  name: string
  ports: number[]
  addresses: string[]
  targetMode: NatRuleTargetMode
  serverIds: string[]
  enabled: boolean
  createdAt: string
  updatedAt: string
}

export type ProxyRulesConfig = {
  servers: ProxyServer[]
  rules: NatRule[]
}

export type ProxyServerDraft = {
  name: string
  host: string
}

export type NatRuleDraft = {
  name: string
  ports: number[]
  addresses: string[]
  targetMode: NatRuleTargetMode
  serverIds: string[]
  enabled: boolean
}
