'use client'

import { useState } from 'react'
import { Account } from '@/app/lib/types/account'
import { AccountModal } from '@/app/features/accounts/AccountModal'

interface AccountCardProps {
  account: Account
}

export function AccountCard({ account }: AccountCardProps) {
  const [isModalOpen, setIsModalOpen] = useState(false)

  return (
    <>
      <button
        onClick={() => setIsModalOpen(true)}
        className="bg-gradient-to-br from-white to-gray-50 p-6 rounded-2xl border border-gray-200/70 hover:border-blue-500/50 hover:shadow-xl transition-all duration-300 text-left group"
      >
        <div className="text-sm font-medium text-gray-500 mb-2">ID: {account.accountId}</div>
        <div className="text-lg font-semibold text-gray-900 group-hover:text-blue-600 transition-colors">
          {account.firstName} {account.lastName}
        </div>
      </button>

      {isModalOpen && (
        <AccountModal 
          accountId={account.accountId} 
          onClose={() => setIsModalOpen(false)} 
        />
      )}
    </>
  )
}
