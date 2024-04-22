import type { PrismaClient } from "@prisma/client";

import { getLocalAppMetadata } from "@shipmvp/app-store/utils";
import { sendDisabledAppEmail } from "@shipmvp/emails";
import { getTranslation } from "@shipmvp/lib/server";
import { AppCategories } from "@shipmvp/prisma/enums";

import { TRPCError } from "@trpc/server";

import type { TrpcSessionUser } from "../../../trpc";
import type { TToggleInputSchema } from "./toggle.schema";

type ToggleOptions = {
  ctx: {
    user: NonNullable<TrpcSessionUser>;
    prisma: PrismaClient;
  };
  input: TToggleInputSchema;
};

export const toggleHandler = async ({ input, ctx }: ToggleOptions) => {
  const { prisma } = ctx;
  const { enabled, slug } = input;

  // Get app name from metadata
  const localApps = getLocalAppMetadata();
  const appMetadata = localApps.find((localApp) => localApp.slug === slug);

  if (!appMetadata) {
    throw new TRPCError({ code: "INTERNAL_SERVER_ERROR", message: "App metadata could not be found" });
  }

  const app = await prisma.app.upsert({
    where: {
      slug,
    },
    update: {
      enabled,
      dirName: appMetadata?.dirName || appMetadata?.slug || "",
    },
    create: {
      slug,
      dirName: appMetadata?.dirName || appMetadata?.slug || "",
      categories:
        (appMetadata?.categories as AppCategories[]) ||
        ([appMetadata?.category] as AppCategories[]) ||
        undefined,
      keys: undefined,
      enabled,
    },
  });

  // If disabling an app then we need to alert users basesd on the app type
  if (!enabled) {
    const translations = new Map();
    if (
      app.categories.some((category) =>
        (
          [AppCategories.calendar, AppCategories.video, AppCategories.conferencing] as AppCategories[]
        ).includes(category)
      )
    ) {
      // Find all users with the app credentials
      const appCredentials = await prisma.credential.findMany({
        where: {
          appId: app.slug,
        },
        select: {
          user: {
            select: {
              email: true,
              locale: true,
            },
          },
        },
      });

      // TODO: This should be done async probably using a queue.
      Promise.all(
        appCredentials.map(async (credential) => {
          // No need to continue if credential does not have a user
          if (!credential.user || !credential.user.email) return;

          const locale = credential.user.locale ?? "en";
          let t = translations.get(locale);

          if (!t) {
            t = await getTranslation(locale, "common");
            translations.set(locale, t);
          }

          await sendDisabledAppEmail({
            email: credential.user.email,
            appName: appMetadata?.name || app.slug,
            appType: app.categories,
            t,
          });
        })
      );
    } else {
    }
  }

  return app.enabled;
};
