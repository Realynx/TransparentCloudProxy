import { useState } from 'react'

import { OneKeyPage } from './pages/OneKeyPage.tsx'
import { ProxyRulesPage } from './pages/ProxyRulesPage.tsx'

type AdminPageId = 'proxy-rules' | 'onekey'

function App() {
  const [activePage, setActivePage] = useState<AdminPageId>('proxy-rules')

  return (
    <div className="relative min-h-screen overflow-hidden">
      <div className="pointer-events-none absolute inset-0 opacity-40 [background-image:linear-gradient(rgba(30,109,227,0.14)_1px,transparent_1px),linear-gradient(90deg,rgba(30,109,227,0.14)_1px,transparent_1px)] [background-size:34px_34px]" />

      <main className="relative mx-auto flex w-full max-w-7xl flex-col gap-6 px-4 pb-10 pt-6 sm:px-6 lg:px-8">
        <nav className="panel animate-rise flex flex-wrap items-center justify-between gap-3 p-4">
          <div>
            <p className="font-mono text-xs uppercase tracking-[0.22em] text-blue/80">Admin Pages</p>
            <p className="mt-1 text-sm text-slate-300">Switch between proxy credential and firewall policy management.</p>
          </div>

          <div className="inline-flex rounded-xl border border-blue/25 bg-midnight/70 p-1">
            <button
              type="button"
              onClick={() => setActivePage('proxy-rules')}
              className={`rounded-lg px-3 py-2 text-xs font-semibold uppercase tracking-wide transition ${
                activePage === 'proxy-rules'
                  ? 'bg-blue text-white'
                  : 'text-slate-300 hover:bg-blue/10'
              }`}
            >
              Proxy Rules
            </button>
            <button
              type="button"
              onClick={() => setActivePage('onekey')}
              className={`rounded-lg px-3 py-2 text-xs font-semibold uppercase tracking-wide transition ${
                activePage === 'onekey'
                  ? 'bg-blue text-white'
                  : 'text-slate-300 hover:bg-blue/10'
              }`}
            >
              OneKey Credentials
            </button>
          </div>
        </nav>

        {activePage === 'proxy-rules' ? <ProxyRulesPage /> : <OneKeyPage />}
      </main>
    </div>
  )
}

export default App
