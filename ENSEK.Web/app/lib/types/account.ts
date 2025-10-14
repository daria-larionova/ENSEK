import { z } from 'zod'

export const MeterReadingSchema = z.object({
  id: z.number(),
  accountId: z.number(),
  meterReadingDateTime: z.string(),
  meterReadValue: z.string(),
})

export const AccountSchema = z.object({
  accountId: z.number(),
  firstName: z.string(),
  lastName: z.string(),
})

export const AccountWithReadingsSchema = AccountSchema.extend({
  meterReadings: z.array(MeterReadingSchema),
})

export type MeterReading = z.infer<typeof MeterReadingSchema>
export type Account = z.infer<typeof AccountSchema>
export type AccountWithReadings = z.infer<typeof AccountWithReadingsSchema>


