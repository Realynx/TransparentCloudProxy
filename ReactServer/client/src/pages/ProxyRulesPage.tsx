import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'

import {
  createNatRule,
  createProxyServer,
  deleteNatRule,
  deleteProxyServer,
  getProxyRulesConfig,
  updateNatRule,
} from '../api.ts'
import type { NatRule, NatRuleDraft, NatRuleTargetMode, ProxyServer, ProxyServerDraft } from '../types.ts'

type NatRuleFormState = {
  name: string
  portsInput: string
  addressesInput: string
  targetMode: NatRuleTargetMode
  serverIds: string[]
  enabled: boolean
}

const defaultServerDraft: ProxyServerDraft = {
  name: '',
  host: '',
}

const defaultRuleForm: NatRuleFormState = {
  name: '',
  portsInput: '443',
  addressesInput: '0.0.0.0',
  targetMode: 'all',
  serverIds: [],
  enabled: true,
}

const toRuleDraft = (form: NatRuleFormState): NatRuleDraft => {
  const ports = form.portsInput
    .split(/[\s,]+/)
    .map((entry) => entry.trim())
    .filter((entry) => entry.length > 0)
    .map((entry) => Number(entry))

  if (ports.length === 0) {
    throw new Error('Provide at least one listen port')
  }

  for (const port of ports) {
    if (!Number.isInteger(port) || port < 1 || port > 65535) {
      throw new Error(`Invalid port value: ${port}`)
    }
  }

  const addresses = form.addressesInput
    .split(/[\n,]+/)
    .map((entry) => entry.trim())
    .filter((entry) => entry.length > 0)

  if (addresses.length === 0) {
    throw new Error('Provide at least one bind address. Use 0.0.0.0 for all interfaces.')
  }

  if (form.targetMode === 'selected' && form.serverIds.length === 0) {
    throw new Error('Select at least one proxy server when target mode is Selected servers')
  }

  return {
    name: form.name.trim(),
    ports,
    addresses,
    targetMode: form.targetMode,
    serverIds: form.targetMode === 'all' ? [] : form.serverIds,
    enabled: form.enabled,
  }
}

const ruleToForm = (rule: NatRule): NatRuleFormState => ({
  name: rule.name,
  portsInput: rule.ports.join(', '),
  addressesInput: rule.addresses.join('\n'),
  targetMode: rule.targetMode,
  serverIds: [...rule.serverIds],
  enabled: rule.enabled,
})

export function ProxyRulesPage() {
  const [servers, setServers] = useState<ProxyServer[]>([])
  const [rules, setRules] = useState<NatRule[]>([])
  const [serverDraft, setServerDraft] = useState<ProxyServerDraft>(defaultServerDraft)
  const [ruleForm, setRuleForm] = useState<NatRuleFormState>(defaultRuleForm)
  const [editingRuleId, setEditingRuleId] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const [savingRule, setSavingRule] = useState(false)
  const [savingServer, setSavingServer] = useState(false)
  const [busyRuleId, setBusyRuleId] = useState<string | null>(null)
  const [busyServerId, setBusyServerId] = useState<string | null>(null)
  const [notice, setNotice] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  const loadConfig = async (): Promise<void> => {
    try {
      const config = await getProxyRulesConfig()
      setServers(config.servers)
      setRules(config.rules)
      setError(null)
    } catch (loadError) {
      setError(loadError instanceof Error ? loadError.message : 'Failed to load proxy rule config')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void loadConfig()
    const timer = window.setInterval(() => {
      void loadConfig()
    }, 15000)

    return () => {
      window.clearInterval(timer)
    }
  }, [])

  const ruleCountEnabled = useMemo(() => rules.filter((entry) => entry.enabled).length, [rules])

  const uniquePorts = useMemo(() => {
    const allPorts = new Set<number>()
    for (const rule of rules) {
      for (const port of rule.ports) {
        allPorts.add(port)
      }
    }

    return allPorts.size
  }, [rules])

  const resetRuleEditor = () => {
    setEditingRuleId(null)
    setRuleForm(defaultRuleForm)
  }

  const startEditRule = (rule: NatRule) => {
    setEditingRuleId(rule.id)
    setRuleForm(ruleToForm(rule))
  }

  const onSubmitServer = async (event: FormEvent<HTMLFormElement>): Promise<void> => {
    event.preventDefault()
    setSavingServer(true)
    setNotice(null)
    setError(null)

    try {
      await createProxyServer(serverDraft)
      setServerDraft(defaultServerDraft)
      setNotice('Proxy server added to fleet')
      await loadConfig()
    } catch (submitError) {
      setError(submitError instanceof Error ? submitError.message : 'Failed to add proxy server')
    } finally {
      setSavingServer(false)
    }
  }

  const onDeleteServer = async (id: string): Promise<void> => {
    setBusyServerId(id)
    setNotice(null)
    setError(null)

    try {
      await deleteProxyServer(id)
      setNotice('Proxy server removed. Any affected selected rules were auto-disabled.')
      await loadConfig()
    } catch (deleteError) {
      setError(deleteError instanceof Error ? deleteError.message : 'Failed to remove proxy server')
    } finally {
      setBusyServerId(null)
    }
  }

  const onSubmitRule = async (event: FormEvent<HTMLFormElement>): Promise<void> => {
    event.preventDefault()
    setSavingRule(true)
    setNotice(null)
    setError(null)

    try {
      const payload = toRuleDraft(ruleForm)

      if (editingRuleId) {
        await updateNatRule(editingRuleId, payload)
        setNotice('NAT rule updated')
      } else {
        await createNatRule(payload)
        setNotice('NAT rule created')
      }

      resetRuleEditor()
      await loadConfig()
    } catch (submitError) {
      setError(submitError instanceof Error ? submitError.message : 'Failed to save NAT rule')
    } finally {
      setSavingRule(false)
    }
  }

  const onDeleteRule = async (id: string): Promise<void> => {
    setBusyRuleId(id)
    setNotice(null)
    setError(null)

    try {
      await deleteNatRule(id)
      if (editingRuleId === id) {
        resetRuleEditor()
      }
      setNotice('NAT rule deleted')
      await loadConfig()
    } catch (deleteError) {
      setError(deleteError instanceof Error ? deleteError.message : 'Failed to delete NAT rule')
    } finally {
      setBusyRuleId(null)
    }
  }

  const onToggleRule = async (rule: NatRule): Promise<void> => {
    setBusyRuleId(rule.id)
    setNotice(null)
    setError(null)

    try {
      await updateNatRule(rule.id, {
        name: rule.name,
        ports: rule.ports,
        addresses: rule.addresses,
        targetMode: rule.targetMode,
        serverIds: rule.serverIds,
        enabled: !rule.enabled,
      })
      setNotice(`Rule ${rule.enabled ? 'disabled' : 'enabled'}`)
      await loadConfig()
    } catch (toggleError) {
      setError(toggleError instanceof Error ? toggleError.message : 'Failed to toggle rule state')
    } finally {
      setBusyRuleId(null)
    }
  }

  const targetSummary = (rule: NatRule): string => {
    if (rule.targetMode === 'all') {
      return `All servers alias (currently ${servers.length}/${servers.length} servers)`
    }

    const selected = servers.filter((entry) => rule.serverIds.includes(entry.id))
    if (selected.length === 0) {
      return 'Selected servers (none available)'
    }

    return `Selected ${selected.length}/${servers.length} server(s): ${selected.map((entry) => entry.name).join(', ')}`
  }

  return (
    <>
      <header className="panel animate-rise overflow-hidden p-6" style={{ animationDelay: '30ms' }}>
        <p className="font-mono text-xs uppercase tracking-[0.35em] text-blue/80">Firewall Management</p>
        <div className="mt-3 flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
          <h1 className="font-heading text-3xl text-white sm:text-4xl">NAT Proxy Rules Control Plane</h1>
          <div className="rounded-lg border border-orange/40 bg-orange/10 px-3 py-2 font-mono text-xs text-orange">
            Stateful Policy + Server Targeting
          </div>
        </div>
        <p className="mt-3 max-w-3xl text-sm text-slate-300">
          Define incoming ports, bind addresses, and execution targets for your proxy fleet. Use the All servers alias
          to automatically include newly added servers without editing existing rules.
        </p>
      </header>

      {notice && (
        <div className="animate-rise rounded-xl border border-blue/50 bg-blue/10 p-3 text-sm text-blue" role="status">
          {notice}
        </div>
      )}

      {error && (
        <div className="animate-rise whitespace-pre-wrap rounded-xl border border-orange/50 bg-orange/10 p-3 text-sm text-orange" role="alert">
          {error}
        </div>
      )}

      <section className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <article className="panel animate-rise p-4" style={{ animationDelay: '70ms' }}>
          <p className="text-xs uppercase tracking-[0.2em] text-blue/80">Total Rules</p>
          <p className="mt-3 font-heading text-3xl text-white">{rules.length}</p>
          <p className="mt-1 text-sm text-slate-300">configured NAT/passthrough entries</p>
        </article>
        <article className="panel animate-rise p-4" style={{ animationDelay: '120ms' }}>
          <p className="text-xs uppercase tracking-[0.2em] text-blue/80">Enabled Rules</p>
          <p className="mt-3 font-heading text-3xl text-white">{ruleCountEnabled}</p>
          <p className="mt-1 text-sm text-slate-300">actively deployed policies</p>
        </article>
        <article className="panel animate-rise p-4" style={{ animationDelay: '170ms' }}>
          <p className="text-xs uppercase tracking-[0.2em] text-blue/80">Unique Listen Ports</p>
          <p className="mt-3 font-heading text-3xl text-white">{uniquePorts}</p>
          <p className="mt-1 text-sm text-slate-300">across all defined rules</p>
        </article>
        <article className="panel animate-rise p-4" style={{ animationDelay: '220ms' }}>
          <p className="text-xs uppercase tracking-[0.2em] text-blue/80">Proxy Fleet</p>
          <p className="mt-3 inline-flex items-center gap-2 font-heading text-2xl text-white">
            <span className="inline-flex h-2.5 w-2.5 animate-signal rounded-full bg-blue" />
            {loading ? 'Syncing' : `${servers.length} Online`}
          </p>
          <p className="mt-1 text-sm text-slate-300">named proxy execution nodes</p>
        </article>
      </section>

      <section className="grid gap-6 xl:grid-cols-[1.5fr_1fr]">
        <article className="panel animate-rise p-5" style={{ animationDelay: '250ms' }}>
          <div className="mb-4 flex flex-wrap items-center justify-between gap-2">
            <h2 className="font-heading text-2xl text-white">
              {editingRuleId ? 'Edit NAT Rule' : 'Create NAT Rule'}
            </h2>
            {editingRuleId && (
              <button
                type="button"
                onClick={resetRuleEditor}
                className="rounded-lg border border-orange/40 px-3 py-1.5 text-xs font-semibold uppercase tracking-wide text-orange transition hover:bg-orange/10"
              >
                Cancel Edit
              </button>
            )}
          </div>

          <form className="grid gap-3" onSubmit={(event) => void onSubmitRule(event)}>
            <input
              className="field"
              placeholder="Rule name (example: Public HTTPS ingress)"
              value={ruleForm.name}
              onChange={(event) =>
                setRuleForm((state) => ({
                  ...state,
                  name: event.target.value,
                }))
              }
            />

            <input
              className="field"
              placeholder="Ports (comma separated, example: 80,443,8443)"
              value={ruleForm.portsInput}
              onChange={(event) =>
                setRuleForm((state) => ({
                  ...state,
                  portsInput: event.target.value,
                }))
              }
            />

            <textarea
              className="field"
              rows={4}
              placeholder="Bind addresses (comma or newline separated). Use 0.0.0.0 for all interfaces."
              value={ruleForm.addressesInput}
              onChange={(event) =>
                setRuleForm((state) => ({
                  ...state,
                  addressesInput: event.target.value,
                }))
              }
            />

            <div className="rounded-xl border border-blue/20 bg-midnight/70 p-3">
              <p className="font-mono text-xs uppercase tracking-[0.2em] text-blue/80">Server Targeting</p>
              <div className="mt-3 flex flex-wrap gap-2">
                <button
                  type="button"
                  onClick={() =>
                    setRuleForm((state) => ({
                      ...state,
                      targetMode: 'all',
                    }))
                  }
                  className={`rounded-lg border px-3 py-1.5 text-xs font-semibold uppercase tracking-wide transition ${
                    ruleForm.targetMode === 'all'
                      ? 'border-blue bg-blue/20 text-blue'
                      : 'border-blue/30 text-slate-300 hover:bg-blue/10'
                  }`}
                >
                  All Servers Alias
                </button>
                <button
                  type="button"
                  onClick={() =>
                    setRuleForm((state) => ({
                      ...state,
                      targetMode: 'selected',
                    }))
                  }
                  className={`rounded-lg border px-3 py-1.5 text-xs font-semibold uppercase tracking-wide transition ${
                    ruleForm.targetMode === 'selected'
                      ? 'border-blue bg-blue/20 text-blue'
                      : 'border-blue/30 text-slate-300 hover:bg-blue/10'
                  }`}
                >
                  Selected Servers
                </button>
              </div>

              {ruleForm.targetMode === 'all' && (
                <p className="mt-3 text-sm text-slate-300">
                  This alias dynamically targets all current and future servers in the fleet.
                </p>
              )}

              {ruleForm.targetMode === 'selected' && (
                <div className="mt-3 grid gap-2">
                  {servers.length === 0 && (
                    <p className="text-sm text-orange">No servers available. Add at least one server first.</p>
                  )}

                  {servers.map((server) => {
                    const checked = ruleForm.serverIds.includes(server.id)

                    return (
                      <label
                        key={server.id}
                        className="flex cursor-pointer items-center justify-between rounded-lg border border-blue/20 bg-black/40 px-3 py-2"
                      >
                        <span>
                          <span className="block text-sm text-white">{server.name}</span>
                          <span className="block font-mono text-xs text-slate-400">{server.host}</span>
                        </span>
                        <input
                          type="checkbox"
                          className="h-4 w-4 rounded border-blue/30 bg-midnight"
                          checked={checked}
                          onChange={() => {
                            setRuleForm((state) => {
                              const nextIds = checked
                                ? state.serverIds.filter((id) => id !== server.id)
                                : [...state.serverIds, server.id]

                              return {
                                ...state,
                                serverIds: nextIds,
                              }
                            })
                          }}
                        />
                      </label>
                    )
                  })}
                </div>
              )}
            </div>

            <label className="inline-flex items-center gap-2 text-sm text-slate-300">
              <input
                type="checkbox"
                className="h-4 w-4 rounded border-blue/30 bg-midnight"
                checked={ruleForm.enabled}
                onChange={(event) =>
                  setRuleForm((state) => ({
                    ...state,
                    enabled: event.target.checked,
                  }))
                }
              />
              Rule enabled
            </label>

            <button
              type="submit"
              disabled={savingRule}
              className="rounded-xl bg-blue px-4 py-2 font-semibold text-white transition hover:brightness-110 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {savingRule
                ? 'Saving rule...'
                : editingRuleId
                  ? 'Update NAT Rule'
                  : 'Create NAT Rule'}
            </button>
          </form>
        </article>

        <article className="panel animate-rise p-5" style={{ animationDelay: '280ms' }}>
          <h2 className="font-heading text-2xl text-white">Proxy Server Fleet</h2>
          <p className="mt-2 text-sm text-slate-300">
            Manage proxy nodes available as execution targets for NAT rules.
          </p>

          <form className="mt-4 grid gap-3" onSubmit={(event) => void onSubmitServer(event)}>
            <input
              className="field"
              placeholder="Server name"
              value={serverDraft.name}
              onChange={(event) =>
                setServerDraft((state) => ({
                  ...state,
                  name: event.target.value,
                }))
              }
            />
            <input
              className="field"
              placeholder="Host or address (example: 10.10.0.20)"
              value={serverDraft.host}
              onChange={(event) =>
                setServerDraft((state) => ({
                  ...state,
                  host: event.target.value,
                }))
              }
            />
            <button
              type="submit"
              disabled={savingServer}
              className="rounded-xl bg-orange px-4 py-2 font-semibold text-black transition hover:brightness-110 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {savingServer ? 'Adding server...' : 'Add Server'}
            </button>
          </form>

          <div className="mt-4 grid gap-2">
            {servers.map((server) => (
              <div key={server.id} className="rounded-xl border border-blue/20 bg-midnight/70 p-3">
                <div className="flex items-center justify-between gap-2">
                  <div>
                    <p className="text-sm font-semibold text-white">{server.name}</p>
                    <p className="font-mono text-xs text-slate-400">{server.host}</p>
                  </div>
                  <button
                    type="button"
                    onClick={() => void onDeleteServer(server.id)}
                    disabled={busyServerId === server.id}
                    className="rounded-lg border border-orange/30 px-2 py-1 text-xs font-semibold uppercase tracking-wide text-orange transition hover:bg-orange/10 disabled:cursor-not-allowed disabled:opacity-50"
                  >
                    {busyServerId === server.id ? 'Removing...' : 'Remove'}
                  </button>
                </div>
              </div>
            ))}

            {servers.length === 0 && (
              <p className="text-sm text-slate-300">No proxy servers registered yet.</p>
            )}
          </div>
        </article>
      </section>

      <section className="panel animate-rise p-5" style={{ animationDelay: '310ms' }}>
        <div className="mb-4 flex flex-wrap items-center justify-between gap-2">
          <h2 className="font-heading text-2xl text-white">Rule Set</h2>
          <button
            type="button"
            onClick={() => void loadConfig()}
            className="rounded-lg border border-blue/40 px-3 py-1.5 text-xs font-semibold uppercase tracking-wide text-blue transition hover:bg-blue/10"
          >
            Refresh
          </button>
        </div>

        <div className="grid gap-3">
          {rules.map((rule) => (
            <div key={rule.id} className="rounded-xl border border-blue/20 bg-midnight/70 p-4">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <div className="flex flex-wrap items-center gap-2">
                    <p className="font-heading text-lg text-white">{rule.name}</p>
                    <span
                      className={`rounded-full px-2.5 py-1 text-[10px] font-bold uppercase tracking-[0.14em] ${
                        rule.enabled
                          ? 'bg-blue/20 text-blue'
                          : 'bg-orange/20 text-orange'
                      }`}
                    >
                      {rule.enabled ? 'Enabled' : 'Disabled'}
                    </span>
                  </div>
                  <p className="mt-1 text-xs text-slate-400">Updated {new Date(rule.updatedAt).toLocaleString()}</p>
                </div>
                <div className="flex flex-wrap items-center gap-2">
                  <button
                    type="button"
                    onClick={() => startEditRule(rule)}
                    className="rounded-lg border border-blue/30 px-3 py-1.5 text-xs font-semibold uppercase tracking-wide text-blue transition hover:bg-blue/10"
                  >
                    Edit
                  </button>
                  <button
                    type="button"
                    onClick={() => void onToggleRule(rule)}
                    disabled={busyRuleId === rule.id}
                    className="rounded-lg border border-blue/30 px-3 py-1.5 text-xs font-semibold uppercase tracking-wide text-slate-200 transition hover:bg-blue/10 disabled:cursor-not-allowed disabled:opacity-50"
                  >
                    {busyRuleId === rule.id ? 'Working...' : rule.enabled ? 'Disable' : 'Enable'}
                  </button>
                  <button
                    type="button"
                    onClick={() => void onDeleteRule(rule.id)}
                    disabled={busyRuleId === rule.id}
                    className="rounded-lg border border-orange/30 px-3 py-1.5 text-xs font-semibold uppercase tracking-wide text-orange transition hover:bg-orange/10 disabled:cursor-not-allowed disabled:opacity-50"
                  >
                    Delete
                  </button>
                </div>
              </div>

              <div className="mt-3 grid gap-3 sm:grid-cols-2">
                <div>
                  <p className="font-mono text-[11px] uppercase tracking-[0.14em] text-blue/80">Listen Ports</p>
                  <div className="mt-2 flex flex-wrap gap-2">
                    {rule.ports.map((port) => (
                      <span
                        key={port}
                        className="rounded-md border border-blue/30 bg-blue/10 px-2 py-1 font-mono text-xs text-blue"
                      >
                        {port}
                      </span>
                    ))}
                  </div>
                </div>

                <div>
                  <p className="font-mono text-[11px] uppercase tracking-[0.14em] text-blue/80">Bind Addresses</p>
                  <div className="mt-2 flex flex-wrap gap-2">
                    {rule.addresses.map((address) => (
                      <span
                        key={address}
                        className={`rounded-md border px-2 py-1 font-mono text-xs ${
                          address === '0.0.0.0'
                            ? 'border-orange/40 bg-orange/10 text-orange'
                            : 'border-blue/30 bg-blue/10 text-slate-200'
                        }`}
                      >
                        {address === '0.0.0.0' ? '0.0.0.0 (all interfaces)' : address}
                      </span>
                    ))}
                  </div>
                </div>
              </div>

              <p className="mt-3 text-sm text-slate-300">{targetSummary(rule)}</p>
            </div>
          ))}

          {rules.length === 0 && (
            <p className="text-sm text-slate-300">
              No NAT rules yet. Create your first rule to start routing traffic through the proxy fleet.
            </p>
          )}
        </div>
      </section>
    </>
  )
}
