import { AccountModalContent } from '@/app/components/accounts/AccountModalContent'

interface AccountModalProps {
  accountId: number
  onClose: () => void
}

export function AccountModal({ accountId, onClose }: AccountModalProps) {
  return <AccountModalContent accountId={accountId} onClose={onClose} />
}
