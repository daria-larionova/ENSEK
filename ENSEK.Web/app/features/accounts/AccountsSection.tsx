import { getAccounts } from '@/app/lib/actions/accounts'
import { AccountsList } from '@/app/components/accounts/AccountsList'
import { Account } from '@/app/lib/types/account'

export async function AccountsSection() {
  let accounts: Account[] = []
  let error: string | null = null

  try {
    accounts = await getAccounts()
  } catch (err) {
    error = err instanceof Error ? err.message : 'Failed to load accounts'
  }

  return (
    <div className="bg-white/80 backdrop-blur-xl rounded-3xl p-12 shadow-2xl border border-gray-200/50">
      <h2 className="text-3xl font-semibold mb-8 text-gray-800">
        Accounts <span className="text-gray-400 text-xl">({accounts.length})</span>
      </h2>

      {error ? (
        <div className="bg-red-50 border border-red-200 rounded-2xl p-6">
          <p className="text-red-700">Error loading accounts: {error}</p>
        </div>
      ) : (
        <AccountsList accounts={accounts} />
      )}
    </div>
  )
}
