import { Account } from '@/app/lib/types/account'
import { AccountCard } from '@/app/components/accounts/AccountCard'

interface AccountsListProps {
  accounts: Account[]
}

export function AccountsList({ accounts }: AccountsListProps) {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4 max-h-[600px] overflow-y-auto pr-2">
      {accounts.map((account: Account) => (
        <AccountCard key={account.accountId} account={account} />
      ))}
    </div>
  )
}
