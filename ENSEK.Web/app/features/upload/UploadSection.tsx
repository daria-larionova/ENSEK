import { UploadForm } from '@/app/components/upload/UploadForm'

export function UploadSection() {
  return (
    <div className="bg-white/80 backdrop-blur-xl rounded-3xl p-12 shadow-2xl border border-gray-200/50">
      <h2 className="text-3xl font-semibold mb-8 text-gray-800">Upload Meter Readings</h2>
      <UploadForm />
    </div>
  )
}
