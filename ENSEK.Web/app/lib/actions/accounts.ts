'use server'

import { apiClient } from '@/app/lib/api/client'
import { Account, AccountWithReadings } from '@/app/lib/types/account'

export async function getAccounts(): Promise<Account[]> {
  try {
    const response = await apiClient.get('/Accounts')
    return response.data
  } catch (error) {
    console.error('Failed to fetch accounts:', error)
    throw new Error('Failed to fetch accounts')
  }
}

export async function getAccountById(accountId: number): Promise<AccountWithReadings> {
  try {
    const response = await apiClient.get(`/Accounts/${accountId}`)
    return response.data
  } catch (error) {
    console.error('Failed to fetch account:', error)
    throw new Error('Failed to fetch account')
  }
}
