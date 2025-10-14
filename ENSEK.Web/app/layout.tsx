import type { Metadata } from 'next'
import './globals.css'

export const metadata: Metadata = {
  title: 'ENSEK Meter Reading Upload',
  description: 'Upload and validate meter readings with Clean Architecture',
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="en">
      <body>
        {children}
      </body>
    </html>
  )
}
