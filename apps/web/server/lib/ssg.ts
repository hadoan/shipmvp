import type { GetStaticPropsContext } from "next";
import { serverSideTranslations } from "next-i18next/serverSideTranslations";
import superjson from "superjson";

import prisma from "@shipmvp/prisma";
import { appRouter } from "@shipmvp/trpc/server/routers/_app";

import { createProxySSGHelpers } from "@trpc/react-query/ssg";

// eslint-disable-next-line @typescript-eslint/no-var-requires
const { i18n } = require("@shipmvp/config/next-i18next.config");

export async function ssgInit<TParams extends { locale?: string }>(opts: GetStaticPropsContext<TParams>) {
  const requestedLocale = opts.params?.locale || opts.locale || i18n.defaultLocale;
  const isSupportedLocale = i18n.locales.includes(requestedLocale);
  if (!isSupportedLocale) {
    console.warn(`Requested unsupported locale "${requestedLocale}"`);
  }
  const locale = isSupportedLocale ? requestedLocale : i18n.defaultLocale;

  const _i18n = await serverSideTranslations(locale, ["common"]);

  const ssg = createProxySSGHelpers({
    router: appRouter,
    transformer: superjson,
    ctx: {
      prisma,
      session: null,
      locale,
      i18n: _i18n,
    },
  });

  return ssg;
}
