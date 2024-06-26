import type { Prisma } from "@shipmvp/prisma/client";

export interface FilteredApp {
  name: string;
  slug: string;
  logo: string;
  title?: string;
  type: string;
  description: string;
  dirName: string;
  keys: Prisma.JsonObject | null;
  enabled: boolean;
  isTemplate?: boolean;
}
