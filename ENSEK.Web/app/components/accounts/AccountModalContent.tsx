'use client'

import { useState, useEffect } from 'react'
import { getAccountById } from '@/app/lib/actions/accounts'
import { Account, AccountWithReadings } from '@/app/lib/types/account'

interface AccountModalContentProps {
  accountId: number
  onClose: () => void
}

export function AccountModalContent({ accountId, onClose }: AccountModalContentProps) {
  const [account, setAccount] = useState<AccountWithReadings | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const loadAccountData = async () => {
      try {
        setIsLoading(true)
        setError(null)
        
        const accountData = await getAccountById(accountId)
        setAccount(accountData)
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load account data')
      } finally {
        setIsLoading(false)
      }
    }

    loadAccountData()
  }, [accountId])

  return (
    <div className="fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-3xl shadow-2xl max-w-4xl w-full max-h-[90vh] overflow-hidden">
        <div className="p-8 border-b border-gray-200">
          <div className="flex justify-between items-start">
            <div>
              <h2 className="text-2xl font-semibold text-gray-900">
                {isLoading ? 'Loading...' : `${account?.firstName} ${account?.lastName}`}
              </h2>
              <p className="text-gray-500 mt-1">Account ID: {accountId}</p>
            </div>
            <button
              onClick={onClose}
              className="text-gray-400 hover:text-gray-600 transition-colors text-3xl leading-none"
            >
              ×
            </button>
          </div>
        </div>

        <div className="p-8 overflow-y-auto max-h-[calc(90vh-140px)]">
          {isLoading && (
            <div className="flex items-center justify-center py-12">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
            </div>
          )}

          {error && (
            <div className="bg-red-50 border border-red-200 rounded-2xl p-6">
              <p className="text-red-700">Error: {error}</p>
            </div>
          )}

          {account && (
            <>
              <h3 className="text-xl font-semibold mb-4 text-gray-900">
                Meter Readings ({account.meterReadings?.length || 0})
              </h3>
              {!account.meterReadings || !Array.isArray(account.meterReadings) || account.meterReadings.length === 0 ? (
                <p className="text-gray-500 text-center py-8">No meter readings for this account</p>
              ) : (
                <div className="space-y-3">
                  {account.meterReadings
                    .sort(
                      (a: any, b: any) =>
                        new Date(b.meterReadingDateTime).getTime() -
                        new Date(a.meterReadingDateTime).getTime()
                    )
                    .map((reading: any) => (
                      <div
                        key={reading.id}
                        className="bg-gradient-to-r from-gray-50 to-white p-5 rounded-xl border border-gray-200 hover:border-blue-500/30 hover:shadow-md transition-all"
                      >
                        <div className="flex justify-between items-center">
                          <div>
                            <div className="text-sm text-gray-500 mb-1">
                              {new Date(reading.meterReadingDateTime).toLocaleString('en-GB', {
                                day: '2-digit',
                                month: '2-digit',
                                year: 'numeric',
                                hour: '2-digit',
                                minute: '2-digit',
                              })}
                            </div>
                            <div className="text-2xl font-mono font-bold text-gray-900">
                              {reading.meterReadValue}
                            </div>
                          </div>
                        </div>
                      </div>
                    ))}
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  )
}
