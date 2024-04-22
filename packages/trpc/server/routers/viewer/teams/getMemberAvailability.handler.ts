import { isTeamMember } from "@shipmvp/lib/server/queries/teams";
import { availabilityUserSelect } from "@shipmvp/prisma";
import { prisma } from "@shipmvp/prisma";
import type { TrpcSessionUser } from "@shipmvp/trpc/server/trpc";

import { TRPCError } from "@trpc/server";

import type { TGetMemberAvailabilityInputSchema } from "./getMemberAvailability.schema";

type GetMemberAvailabilityOptions = {
  ctx: {
    user: NonNullable<TrpcSessionUser>;
  };
  input: TGetMemberAvailabilityInputSchema;
};

export const getMemberAvailabilityHandler = async ({ ctx, input }: GetMemberAvailabilityOptions) => {
  const team = await isTeamMember(ctx.user?.id, input.teamId);
  if (!team) throw new TRPCError({ code: "UNAUTHORIZED" });

  // verify member is in team
  const members = await prisma.membership.findMany({
    where: { teamId: input.teamId },
    include: {
      user: {
        select: {
          credentials: true, // needed for getUserAvailability
          ...availabilityUserSelect,
          organization: {
            select: {
              slug: true,
            },
          },
        },
      },
    },
  });
  const member = members?.find((m) => m.userId === input.memberId);
  if (!member) throw new TRPCError({ code: "NOT_FOUND", message: "Member not found" });
  if (!member.user.username)
    throw new TRPCError({ code: "BAD_REQUEST", message: "Member doesn't have a username" });
};