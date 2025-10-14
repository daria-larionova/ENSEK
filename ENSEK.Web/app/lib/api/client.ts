import axios from 'axios'

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000'

export const apiClient = axios.create({
  baseURL: `${API_URL}/api/v1`,
  headers: {
    'Content-Type': 'application/json',
  },
})

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error('API Error:', error)
    if (error.response) {
      console.error('Response data:', error.response.data)
      console.error('Response status:', error.response.status)
      const message = error.response.data?.error || error.response.data?.message || `HTTP ${error.response.status}: ${error.response.statusText}`
      throw new Error(message)
    } else if (error.request) {
      console.error('No response received:', error.request)
      throw new Error('No response from server. Check if API is running.')
    } else {
      console.error('Request setup error:', error.message)
      throw new Error(error.message || 'An unexpected error occurred')
    }
  }
)
