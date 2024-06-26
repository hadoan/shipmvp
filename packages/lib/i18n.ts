/* eslint-disable @typescript-eslint/no-var-requires */
import parser from "accept-language-parser";
import type { IncomingMessage } from "http";

import type { Maybe } from "@shipmvp/trpc/server";

const { i18n } = require("@shipmvp/config/next-i18next.config");

export function getLocaleFromHeaders(req: IncomingMessage): string {
  let preferredLocale: string | null | undefined;
  if (req.headers["accept-language"]) {
    preferredLocale = parser.pick(i18n.locales, req.headers["accept-language"]) as Maybe<string>;
  }
  return preferredLocale ?? i18n.defaultLocale;
}
