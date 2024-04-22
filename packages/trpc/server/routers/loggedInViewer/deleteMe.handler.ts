import { authenticator } from "otplib";

import { ErrorCode } from "@shipmvp/features/auth/lib/ErrorCode";
import { verifyPassword } from "@shipmvp/features/auth/lib/verifyPassword";
import { symmetricDecrypt } from "@shipmvp/lib/crypto";
import { deleteWebUser as syncServicesDeleteWebUser } from "@shipmvp/lib/sync/SyncServiceManager";
import { prisma } from "@shipmvp/prisma";
import { IdentityProvider } from "@shipmvp/prisma/enums";
import type { TrpcSessionUser } from "@shipmvp/trpc/server/trpc";

import type { TDeleteMeInputSchema } from "./deleteMe.schema";

type DeleteMeOptions = {
  ctx: {
    user: NonNullable<TrpcSessionUser>;
  };
  input: TDeleteMeInputSchema;
};

export const deleteMeHandler = async ({ ctx, input }: DeleteMeOptions) => {
  // Check if input.password is correct
  const user = await prisma.user.findUnique({
    where: {
      email: ctx.user.email.toLowerCase(),
    },
  });
  if (!user) {
    throw new Error(ErrorCode.UserNotFound);
  }

  if (user.identityProvider !== IdentityProvider.TEK) {
    throw new Error(ErrorCode.ThirdPartyIdentityProviderEnabled);
  }

  if (!user.password) {
    throw new Error(ErrorCode.UserMissingPassword);
  }

  const isCorrectPassword = await verifyPassword(input.password, user.password);
  if (!isCorrectPassword) {
    throw new Error(ErrorCode.IncorrectPassword);
  }

  if (user.twoFactorEnabled) {
    if (!input.totpCode) {
      throw new Error(ErrorCode.SecondFactorRequired);
    }

    if (!user.twoFactorSecret) {
      console.error(`Two factor is enabled for user ${user.id} but they have no secret`);
      throw new Error(ErrorCode.InternalServerError);
    }

    if (!process.env.MY_APP_ENCRYPTION_KEY) {
      console.error(`"Missing encryption key; cannot proceed with two factor login."`);
      throw new Error(ErrorCode.InternalServerError);
    }

    const secret = symmetricDecrypt(user.twoFactorSecret, process.env.MY_APP_ENCRYPTION_KEY);
    if (secret.length !== 32) {
      console.error(
        `Two factor secret decryption failed. Expected key with length 32 but got ${secret.length}`
      );
      throw new Error(ErrorCode.InternalServerError);
    }

    // If user has 2fa enabled, check if input.totpCode is correct
    const isValidToken = authenticator.check(input.totpCode, secret);
    if (!isValidToken) {
      throw new Error(ErrorCode.IncorrectTwoFactorCode);
    }
  }
  // Remove my account
  const deletedUser = await prisma.user.delete({
    where: {
      id: ctx.user.id,
    },
  });

  // Sync Services
  syncServicesDeleteWebUser(deletedUser);
  return;
};
