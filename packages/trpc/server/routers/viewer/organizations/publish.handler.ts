import { WEBAPP_URL } from "@shipmvp/lib/constants";
import { isOrganisationAdmin } from "@shipmvp/lib/server/queries/organisations";
import { prisma } from "@shipmvp/prisma";
import { teamMetadataSchema } from "@shipmvp/prisma/zod-utils";

import { TRPCError } from "@trpc/server";

import type { TrpcSessionUser } from "../../../trpc";

type PublishOptions = {
  ctx: {
    user: NonNullable<TrpcSessionUser>;
  };
};

export const publishHandler = async ({ ctx }: PublishOptions) => {
  const orgId = ctx.user.organizationId;
  if (!orgId)
    throw new TRPCError({ code: "UNAUTHORIZED", message: "You do not have an organization to upgrade" });

  if (!(await isOrganisationAdmin(ctx.user.id, orgId))) throw new TRPCError({ code: "UNAUTHORIZED" });

  const prevTeam = await prisma.team.findFirst({
    where: {
      id: orgId,
    },
    include: { members: true },
  });

  if (!prevTeam) throw new TRPCError({ code: "NOT_FOUND", message: "Organization not found." });

  const metadata = teamMetadataSchema.safeParse(prevTeam.metadata);
  if (!metadata.success) throw new TRPCError({ code: "BAD_REQUEST", message: "Invalid team metadata" });

  if (!metadata.data?.requestedSlug) {
    throw new TRPCError({
      code: "BAD_REQUEST",
      message: "Can't publish organization without `requestedSlug`",
    });
  }

  const { requestedSlug, ...newMetadata } = metadata.data;
  await prisma.team.update({
    where: { id: orgId },
    data: {
      slug: requestedSlug,
      metadata: { ...newMetadata },
    },
  });

  return {
    url: `${WEBAPP_URL}/settings/organization/profile`,
    message: "Team published successfully",
  };
};
