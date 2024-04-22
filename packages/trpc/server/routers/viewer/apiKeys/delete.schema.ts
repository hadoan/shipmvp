import { z } from "zod";

export const ZDeleteInputSchema = z.object({
  id: z.string(),
});

export type TDeleteInputSchema = z.infer<typeof ZDeleteInputSchema>;
