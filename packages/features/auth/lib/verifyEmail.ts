import { randomBytes } from "crypto";

import { sendEmailVerificationLink } from "@shipmvp/emails/email-manager";
import { checkRateLimitAndThrowError } from "@shipmvp/lib/checkRateLimitAndThrowError";
import { WEBAPP_URL } from "@shipmvp/lib/constants";
import logger from "@shipmvp/lib/logger";
import { getTranslation } from "@shipmvp/lib/server/i18n";
import { prisma } from "@shipmvp/prisma";

const log = logger.getChildLogger({ prefix: [`[[Auth] `] });

interface VerifyEmailType {
  username?: string;
  email: string;
  language?: string;
}

export const sendEmailVerification = async ({ email, language, username }: VerifyEmailType) => {
  const token = randomBytes(32).toString("hex");
  const translation = await getTranslation(language ?? "en", "common");

  await checkRateLimitAndThrowError({
    rateLimitingType: "core",
    identifier: email,
  });

  await prisma.verificationToken.create({
    data: {
      identifier: email,
      token,
      expires: new Date(Date.now() + 24 * 3600 * 1000), // +1 day
    },
  });

  const params = new URLSearchParams({
    token,
  });

  await sendEmailVerificationLink({
    language: translation,
    verificationEmailLink: `${WEBAPP_URL}/api/auth/verify-email?${params.toString()}`,
    user: {
      email,
      name: username,
    },
  });

  return { ok: true, skipped: false };
};
