import type { Prisma } from "@prisma/client";
import { pick } from "lodash";
import z, { ZodNullable, ZodObject, ZodOptional } from "zod";

import { appDataSchemas } from "@shipmvp/app-store/apps.schemas.generated";
import { fieldsSchema as formBuilderFieldsSchema } from "@shipmvp/features/form-builder/FormBuilderFieldsSchema";
import { slugify } from "@shipmvp/lib/slugify";

// dayjs iso parsing is very buggy - cant use :( - turns ISO string into Date object
export const iso8601 = z.string().transform((val, ctx) => {
  const time = Date.parse(val);
  if (!time) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      message: "Invalid ISO Date",
    });
  }
  const d = new Date();
  d.setTime(time);
  return d;
});

export const intervalLimitsType = z
  .object({
    PER_DAY: z.number().optional(),
    PER_WEEK: z.number().optional(),
    PER_MONTH: z.number().optional(),
    PER_YEAR: z.number().optional(),
  })
  .nullable();

export const stringToDate = z.string().transform((a) => new Date(a));

export const stringOrNumber = z.union([
  z.string().transform((v, ctx) => {
    const parsed = parseInt(v);
    if (isNaN(parsed)) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: "Not a number",
      });
    }
    return parsed;
  }),
  z.number().int(),
]);

export const userMetadata = z
  .object({
    proPaidForByTeamId: z.number().optional(),
    stripeCustomerId: z.string().optional(),
    isPremium: z.boolean().optional(),
    sessionTimeout: z.number().optional(), // Minutes
    defaultConferencingApp: z
      .object({
        appSlug: z.string().default("daily-video").optional(),
        appLink: z.string().optional(),
      })
      .optional(),
  })
  .nullable();

export const teamMetadataSchema = z
  .object({
    requestedSlug: z.string(),
    paymentId: z.string(),
    subscriptionId: z.string().nullable(),
    subscriptionItemId: z.string().nullable(),
    isOrganization: z.boolean().nullable(),
    isOrganizationVerified: z.boolean().nullable(),
    orgAutoAcceptEmail: z.string().nullable(),
  })
  .partial()
  .nullable();

/**
 * Like Object.entries, but with actually useful typings
 * @param obj The object to turn into a tuple array (`[key, value][]`)
 * @returns The constructed tuple array from the given object
 * @see https://github.com/3x071c/lsg-remix/blob/e2a9592ba3ec5103556f2cf307c32f08aeaee32d/app/lib/util/entries.ts
 */
export const entries = <O extends Record<string, unknown>>(
  obj: O
): {
  readonly [K in keyof O]: [K, O[K]];
}[keyof O][] => {
  return Object.entries(obj) as {
    [K in keyof O]: [K, O[K]];
  }[keyof O][];
};

/**
 * Returns a type with all readonly notations removed (traverses recursively on an object)
 */
type DeepWriteable<T> = T extends Readonly<{
  -readonly [K in keyof T]: T[K];
}>
  ? {
      -readonly [K in keyof T]: DeepWriteable<T[K]>;
    }
  : T; /* Make it work with readonly types (this is not strictly necessary) */

type FromEntries<T> = T extends [infer Keys, unknown][]
  ? { [K in Keys & PropertyKey]: Extract<T[number], [K, unknown]>[1] }
  : never;

/**
 * Like Object.fromEntries, but with actually useful typings
 * @param arr The tuple array (`[key, value][]`) to turn into an object
 * @returns Object constructed from the given entries
 * @see https://github.com/3x071c/lsg-remix/blob/e2a9592ba3ec5103556f2cf307c32f08aeaee32d/app/lib/util/fromEntries.ts
 */
export const fromEntries = <
  E extends [PropertyKey, unknown][] | ReadonlyArray<readonly [PropertyKey, unknown]>
>(
  entries: E
): FromEntries<DeepWriteable<E>> => {
  return Object.fromEntries(entries) as FromEntries<DeepWriteable<E>>;
};

/** Facilitates converting values from Select inputs to plain ones before submitting */
export const optionToValueSchema = <T extends z.ZodTypeAny>(valueSchema: T) =>
  z
    .object({
      label: z.string(),
      value: valueSchema,
    })
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    .transform((foo) => (foo as any).value as z.infer<T>);

/**
 * Allows parsing without losing original data inference.
 * @url https://github.com/colinhacks/zod/discussions/1655#discussioncomment-4367368
 */
export const getParserWithGeneric =
  <T extends z.ZodTypeAny>(valueSchema: T) =>
  <Data>(data: Data) => {
    type Output = z.infer<typeof valueSchema>;
    return valueSchema.parse(data) as {
      [key in keyof Data]: key extends keyof Output ? Output[key] : Data[key];
    };
  };
