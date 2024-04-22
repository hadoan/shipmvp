import { z } from "zod";

import { optionToValueSchema } from "@shipmvp/prisma/zod-utils";

export const userBodySchema = z
  .object({
    locale: optionToValueSchema(z.string()),
    role: optionToValueSchema(z.enum(["USER", "ADMIN"])),
    weekStart: optionToValueSchema(z.string()),
    timeFormat: optionToValueSchema(z.number()),
  })
  .passthrough();
