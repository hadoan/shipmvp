import { z } from "zod";

import { AppCategories } from "@shipmvp/prisma/enums";

export const ZListLocalInputSchema = z.object({
  category: z.nativeEnum({ ...AppCategories, conferencing: "conferencing" }),
});

export type TListLocalInputSchema = z.infer<typeof ZListLocalInputSchema>;
