import type { TrpcSessionUser } from "@shipmvp/trpc/server/trpc";

type AvatarOptions = {
  ctx: {
    user: NonNullable<TrpcSessionUser>;
  };
};

export const avatarHandler = async ({ ctx }: AvatarOptions) => {
  return {
    avatar: ctx.user.rawAvatar,
  };
};
