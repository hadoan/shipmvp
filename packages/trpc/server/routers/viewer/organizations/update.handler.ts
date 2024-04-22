import type { Prisma } from "@prisma/client";

import { isOrganisationAdmin } from "@shipmvp/lib/server/queries/organisations";
import { prisma } from "@shipmvp/prisma";

import { TRPCError } from "@trpc/server";

import type { TrpcSessionUser } from "../../../trpc";
import type { TUpdateInputSchema } from "./update.schema";

type UpdateOptions = {
  ctx: {
    user: NonNullable<TrpcSessionUser>;
  };
  input: TUpdateInputSchema;
};

export const updateHandler = async ({ ctx, input }: UpdateOptions) => {
  // A user can only have one org so we pass in their currentOrgId here
  const currentOrgId = ctx.user?.organization?.id;

  if (!currentOrgId) throw new TRPCError({ code: "UNAUTHORIZED" });

  if (!(await isOrganisationAdmin(ctx.user?.id, currentOrgId))) throw new TRPCError({ code: "UNAUTHORIZED" });

  if (input.slug) {
    const userConflict = await prisma.team.findMany({
      where: {
        slug: input.slug,
        parent: {
          id: currentOrgId,
        },
      },
    });
    if (userConflict.some((t) => t.id !== currentOrgId))
      throw new TRPCError({ code: "CONFLICT", message: "Slug already in use." });
  }

  const prevOrganisation = await prisma.team.findFirst({
    where: {
      id: currentOrgId,
    },
    select: {
      metadata: true,
      name: true,
      slug: true,
    },
  });

  if (!prevOrganisation) throw new TRPCError({ code: "NOT_FOUND", message: "Organisation not found." });

  const data: Prisma.TeamUpdateArgs["data"] = {
    name: input.name,
    logo: input.logo,
    bio: input.bio,
    hideBranding: input.hideBranding,
    hideBookATeamMember: input.hideBookATeamMember,
    brandColor: input.brandColor,
    darkBrandColor: input.darkBrandColor,
    theme: input.theme,
    timeZone: input.timeZone,
    weekStart: input.weekStart,
    timeFormat: input.timeFormat,
  };

  const updatedOrganisation = await prisma.team.update({
    where: { id: currentOrgId },
    data,
  });

  return { update: true, userId: ctx.user.id };
};
