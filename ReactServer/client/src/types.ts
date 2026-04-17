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
  createdAt: string
}

export type Health = {
  status: string
  uptimeSeconds: number
  timestamp: string
  credentialsStored: number
}
