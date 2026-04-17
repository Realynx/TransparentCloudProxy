import type {
  Health,
  NatRule,
  NatRuleDraft,
  OneKeyImportResult,
  ProxyRulesConfig,
  ProxyServer,
  ProxyServerDraft,
  SavedCredential,
  SaveOneKeyDraft,
} from './types.ts'

const apiBase = '/api'

type RequestOptions = RequestInit & {
  ignoreBody?: boolean
}

async function requestJson<T>(path: string, options?: RequestOptions): Promise<T> {
  const response = await fetch(`${apiBase}${path}`, {
    headers: {
      'Content-Type': 'application/json',
      ...(options?.headers ?? {}),
    },
    ...options,
  })

  if (!response.ok) {
    const errorText = await response.text()
    let parsedError: string | null = null

    try {
      const parsed = JSON.parse(errorText) as { error?: string }
      if (typeof parsed.error === 'string' && parsed.error.length > 0) {
        parsedError = parsed.error
      }
    } catch {
      // Fall back to raw body text when payload is not JSON.
    }

    throw new Error(parsedError ?? errorText ?? `Request failed: ${response.status}`)
  }

  if (options?.ignoreBody) {
    return {} as T
  }

  return (await response.json()) as T
}

export const getHealth = (): Promise<Health> => requestJson<Health>('/health')

export const getSavedCredentials = (): Promise<SavedCredential[]> =>
  requestJson<SavedCredential[]>('/credentials')

export const saveOneKey = (payload: SaveOneKeyDraft): Promise<OneKeyImportResult> =>
  requestJson<OneKeyImportResult>('/credentials', {
    method: 'POST',
    body: JSON.stringify(payload),
  })

export const deleteSavedCredential = (id: string): Promise<SavedCredential> =>
  requestJson<SavedCredential>(`/credentials/${id}`, {
    method: 'DELETE',
  })

export const getProxyRulesConfig = (): Promise<ProxyRulesConfig> =>
  requestJson<ProxyRulesConfig>('/proxy-rules/config')

export const createProxyServer = (payload: ProxyServerDraft): Promise<ProxyServer> =>
  requestJson<ProxyServer>('/proxy-servers', {
    method: 'POST',
    body: JSON.stringify(payload),
  })

export const deleteProxyServer = (id: string): Promise<ProxyServer> =>
  requestJson<ProxyServer>(`/proxy-servers/${id}`, {
    method: 'DELETE',
  })

export const createNatRule = (payload: NatRuleDraft): Promise<NatRule> =>
  requestJson<NatRule>('/proxy-rules', {
    method: 'POST',
    body: JSON.stringify(payload),
  })

export const updateNatRule = (id: string, payload: NatRuleDraft): Promise<NatRule> =>
  requestJson<NatRule>(`/proxy-rules/${id}`, {
    method: 'PUT',
    body: JSON.stringify(payload),
  })

export const deleteNatRule = (id: string): Promise<NatRule> =>
  requestJson<NatRule>(`/proxy-rules/${id}`, {
    method: 'DELETE',
  })
