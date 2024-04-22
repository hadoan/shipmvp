import { ErrorCode } from "@shipmvp/features/auth/lib/ErrorCode";
import { deleteWebUser as syncServicesDeleteWebUser } from "@shipmvp/lib/sync/SyncServiceManager";
import { prisma } from "@shipmvp/prisma";
import { IdentityProvider } from "@shipmvp/prisma/enums";
import type { TrpcSessionUser } from "@shipmvp/trpc/server/trpc";

type DeleteMeWithoutPasswordOptions = {
  ctx: {
    user: NonNullable<TrpcSessionUser>;
  };
};

export const deleteMeWithoutPasswordHandler = async ({ ctx }: DeleteMeWithoutPasswordOptions) => {
  const user = await prisma.user.findUnique({
    where: {
      email: ctx.user.email.toLowerCase(),
    },
  });
  if (!user) {
    throw new Error(ErrorCode.UserNotFound);
  }

  if (user.identityProvider === IdentityProvider.TEK) {
    throw new Error(ErrorCode.SocialIdentityProviderRequired);
  }

  if (user.twoFactorEnabled) {
    throw new Error(ErrorCode.SocialIdentityProviderRequired);
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
