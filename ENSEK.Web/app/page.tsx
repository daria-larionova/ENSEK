import { Header } from './components/Header'
import { UploadSection } from './features/upload/UploadSection'
import { AccountsSection } from './features/accounts/AccountsSection'

export default function Home() {
  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 via-blue-50 to-indigo-50">
      <Header />
      <main className="container mx-auto px-4 py-12">
        <div className="space-y-8">
          <UploadSection />
          <AccountsSection />
        </div>
      </main>
    </div>
  )
}
