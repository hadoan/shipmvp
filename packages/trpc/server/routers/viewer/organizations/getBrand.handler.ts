import { subdomainSuffix, getOrgFullDomain } from "@shipmvp/features/ee/organizations/lib/orgDomains";
import { prisma } from "@shipmvp/prisma";
import { teamMetadataSchema } from "@shipmvp/prisma/zod-utils";
import type { TrpcSessionUser } from "@shipmvp/trpc/server/trpc";

type VerifyCodeOptions = {
  ctx: {
    user: NonNullable<TrpcSessionUser>;
  };
};

export const getBrandHandler = async ({ ctx }: VerifyCodeOptions) => {
  const { user } = ctx;

  if (!user.organizationId) return null;

  const team = await prisma.team.findFirst({
    where: {
      id: user.organizationId,
    },
    select: {
      logo: true,
      name: true,
      slug: true,
      metadata: true,
    },
  });

  const metadata = teamMetadataSchema.parse(team?.metadata);
  const slug = (team?.slug || metadata?.requestedSlug) as string;
  const fullDomain = getOrgFullDomain(slug);
  const domainSuffix = subdomainSuffix();

  return {
    ...team,
    metadata,
    slug,
    fullDomain,
    domainSuffix,
  };
};
