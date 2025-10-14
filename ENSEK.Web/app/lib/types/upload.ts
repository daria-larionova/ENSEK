import { z } from 'zod'

export const UploadResultSchema = z.object({
  successfulReadings: z.number(),
  failedReadings: z.number(),
  errors: z.array(z.string()),
})

export type UploadResult = z.infer<typeof UploadResultSchema>
export type MeterReadingUploadResult = UploadResult


