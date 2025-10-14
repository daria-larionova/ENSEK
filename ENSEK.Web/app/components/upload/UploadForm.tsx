'use client'

import { useState, useRef } from 'react'
import { uploadMeterReadings, clearMeterReadings } from '@/app/lib/actions/meter-readings'
import { MeterReadingUploadResult } from '@/app/lib/types/upload'

interface FileData {
  name: string
  type: string
  size: number
  data: string
}

export function UploadForm() {
  const [selectedFile, setSelectedFile] = useState<File | null>(null)
  const [isUploading, setIsUploading] = useState(false)
  const [isClearing, setIsClearing] = useState(false)
  const [uploadResult, setUploadResult] = useState<MeterReadingUploadResult | null>(null)
  const [uploadError, setUploadError] = useState<string | null>(null)
  const [clearError, setClearError] = useState<string | null>(null)
  const [clearSuccess, setClearSuccess] = useState(false)
  const fileInputRef = useRef<HTMLInputElement>(null)

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      setSelectedFile(e.target.files[0])
      setUploadResult(null)
      setUploadError(null)
    }
  }

  const handleUpload = async () => {
    if (!selectedFile) return

    setIsUploading(true)
    setUploadError(null)
    setUploadResult(null)

    try {
      // Convert File to ArrayBuffer and then to base64
      const arrayBuffer = await selectedFile.arrayBuffer()
      const uint8Array = new Uint8Array(arrayBuffer)
      const base64 = btoa(String.fromCharCode.apply(null, Array.from(uint8Array)))
      
      const result = await uploadMeterReadings({
        name: selectedFile.name,
        type: selectedFile.type,
        size: selectedFile.size,
        data: base64
      })
      setUploadResult(result)
      // Reset file input to allow re-upload
      if (fileInputRef.current) {
        fileInputRef.current.value = ''
      }
      setSelectedFile(null)
    } catch (error) {
      setUploadError(error instanceof Error ? error.message : 'Upload failed')
    } finally {
      setIsUploading(false)
    }
  }

  const handleClearDatabase = async () => {
    if (confirm('Are you sure you want to clear all meter readings? This cannot be undone.')) {
      setIsClearing(true)
      setClearError(null)
      setClearSuccess(false)

      try {
        await clearMeterReadings()
        setClearSuccess(true)
        setSelectedFile(null)
        setUploadResult(null)
        if (fileInputRef.current) {
          fileInputRef.current.value = ''
        }
      } catch (error) {
        setClearError(error instanceof Error ? error.message : 'Clear failed')
      } finally {
        setIsClearing(false)
      }
    }
  }

  return (
    <div className="space-y-6">
      <div className="border-2 border-dashed border-gray-300 rounded-2xl p-12 text-center hover:border-blue-500 transition-colors">
        <input
          ref={fileInputRef}
          type="file"
          accept=".csv"
          onChange={handleFileChange}
          className="hidden"
          id="file-upload"
          disabled={isUploading || isClearing}
        />
        <label
          htmlFor="file-upload"
          className="cursor-pointer inline-flex flex-col items-center"
        >
          <div className="text-6xl mb-4">📁</div>
          <span className="text-lg font-medium text-gray-700 mb-2">
            {selectedFile ? selectedFile.name : 'Click to select CSV file'}
          </span>
          <span className="text-sm text-gray-500">or drag and drop</span>
        </label>
      </div>

      <div className="flex gap-4">
        <button
          onClick={handleUpload}
          disabled={!selectedFile || isUploading || isClearing}
          className="flex-1 bg-gradient-to-r from-blue-600 to-blue-700 hover:from-blue-700 hover:to-blue-800 text-white px-8 py-4 rounded-2xl font-semibold text-lg disabled:opacity-50 disabled:cursor-not-allowed transition-all duration-300 shadow-lg hover:shadow-xl"
        >
          {isUploading ? 'Uploading...' : 'Upload'}
        </button>

        <button
          onClick={handleClearDatabase}
          disabled={isClearing || isUploading}
          className="bg-gradient-to-r from-red-500 to-red-600 hover:from-red-600 hover:to-red-700 text-white px-8 py-4 rounded-2xl font-semibold text-lg disabled:opacity-50 disabled:cursor-not-allowed transition-all duration-300 shadow-lg hover:shadow-xl"
          title="Clear all meter readings from database"
        >
          {isClearing ? 'Clearing...' : 'Clear DB'}
        </button>
      </div>

      {uploadResult && (
        <div className="bg-gradient-to-r from-green-50 to-blue-50 border border-green-200 rounded-2xl p-8">
          <h3 className="text-xl font-semibold mb-4 text-gray-900">Upload Complete</h3>
          <div className="grid grid-cols-2 gap-6 mb-6">
            <div className="bg-white/80 rounded-xl p-6 text-center">
              <div className="text-4xl font-bold text-green-600 mb-2">
                {uploadResult.successfulReadings}
              </div>
              <div className="text-sm text-gray-600">Successful</div>
            </div>
            <div className="bg-white/80 rounded-xl p-6 text-center">
              <div className="text-4xl font-bold text-red-600 mb-2">
                {uploadResult.failedReadings}
              </div>
              <div className="text-sm text-gray-600">Failed</div>
            </div>
          </div>

          {uploadResult.errors.length > 0 && (
            <div className="bg-white/90 rounded-xl p-6">
              <h4 className="font-semibold mb-3 text-gray-900">Errors:</h4>
              <div className="space-y-2 max-h-60 overflow-y-auto">
                  {uploadResult.errors.map((error: string, index: number) => (
                  <div
                    key={index}
                    className="text-sm text-red-700 bg-red-50 p-3 rounded-lg border border-red-100"
                  >
                    {error}
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      )}

      {uploadError && (
        <div className="bg-red-50 border border-red-200 rounded-2xl p-6">
          <p className="text-red-700">Upload failed: {uploadError}</p>
        </div>
      )}

      {clearSuccess && (
        <div className="bg-green-50 border border-green-200 rounded-2xl p-6">
          <p className="text-green-700">All meter readings cleared successfully</p>
        </div>
      )}

      {clearError && (
        <div className="bg-red-50 border border-red-200 rounded-2xl p-6">
          <p className="text-red-700">Clear failed: {clearError}</p>
        </div>
      )}
    </div>
  )
}
