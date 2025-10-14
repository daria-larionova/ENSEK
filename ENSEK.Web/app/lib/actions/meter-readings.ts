'use server'

import { apiClient } from '@/app/lib/api/client'
import { MeterReadingUploadResult } from '@/app/lib/types/upload'

interface FileData {
  name: string
  type: string
  size: number
  data: string // base64 encoded data
}

export async function uploadMeterReadings(fileData: FileData): Promise<MeterReadingUploadResult> {
  try {
    // Convert base64 back to binary data
    const binaryString = atob(fileData.data)
    const bytes = new Uint8Array(binaryString.length)
    for (let i = 0; i < binaryString.length; i++) {
      bytes[i] = binaryString.charCodeAt(i)
    }
    
    // Create a Blob from the binary data
    const blob = new Blob([bytes], { type: fileData.type })
    
    // Create FormData with the blob
    const formData = new FormData()
    formData.append('file', blob, fileData.name)

    const response = await apiClient.post('/MeterReading/meter-reading-uploads', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    })
    return response.data
  } catch (error) {
    console.error('Failed to upload meter readings:', error)
    throw new Error('Failed to upload meter readings')
  }
}

export async function clearMeterReadings(): Promise<void> {
  try {
    await apiClient.delete('/MeterReading/clear-readings')
  } catch (error) {
    console.error('Failed to clear meter readings:', error)
    throw new Error('Failed to clear meter readings')
  }
}

export async function getMeterReadingsByAccount(accountId: number) {
  try {
    const response = await apiClient.get(`/Accounts/${accountId}`)
    return response.data
  } catch (error) {
    console.error('Failed to fetch meter readings:', error)
    throw new Error('Failed to fetch meter readings')
  }
}
