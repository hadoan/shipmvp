import { randomBytes } from "crypto";

import { isTeamAdmin } from "@shipmvp/lib/server/queries/teams";
import { prisma } from "@shipmvp/prisma";
import { TRPCError } from "@shipmvp/trpc/server";
import type { TrpcSessionUser } from "@shipmvp/trpc/server/trpc";

import type { TCreateInviteInputSchema } from "./createInvite.schema";

type CreateInviteOptions = {
  ctx: {
    user: NonNullable<TrpcSessionUser>;
  };
  input: TCreateInviteInputSchema;
};

export const createInviteHandler = async ({ ctx, input }: CreateInviteOptions) => {
  const { teamId } = input;

  if (!(await isTeamAdmin(ctx.user.id, teamId))) throw new TRPCError({ code: "UNAUTHORIZED" });

  const token = randomBytes(32).toString("hex");
  await prisma.verificationToken.create({
    data: {
      identifier: "",
      token,
      expires: new Date(),
      teamId,
    },
  });
  return token;
};
