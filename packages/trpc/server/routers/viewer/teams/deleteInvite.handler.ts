import { isTeamAdmin } from "@shipmvp/lib/server/queries/teams";
import { prisma } from "@shipmvp/prisma";
import { TRPCError } from "@shipmvp/trpc/server";
import type { TrpcSessionUser } from "@shipmvp/trpc/server/trpc";

import type { TDeleteInviteInputSchema } from "./deleteInvite.schema";

type DeleteInviteOptions = {
  ctx: {
    user: NonNullable<TrpcSessionUser>;
  };
  input: TDeleteInviteInputSchema;
};

export const deleteInviteHandler = async ({ ctx, input }: DeleteInviteOptions) => {
  const { token } = input;

  const verificationToken = await prisma.verificationToken.findFirst({
    where: {
      token: token,
    },
    select: {
      teamId: true,
      id: true,
    },
  });

  if (!verificationToken) throw new TRPCError({ code: "NOT_FOUND" });
  if (!verificationToken.teamId || !(await isTeamAdmin(ctx.user.id, verificationToken.teamId)))
    throw new TRPCError({ code: "UNAUTHORIZED" });

  await prisma.verificationToken.delete({ where: { id: verificationToken.id } });
};
