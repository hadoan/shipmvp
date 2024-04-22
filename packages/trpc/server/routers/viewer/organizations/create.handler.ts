import { createHash } from "crypto";
import { totp } from "otplib";

import { sendOrganizationEmailVerification } from "@shipmvp/emails";
import { hashPassword } from "@shipmvp/features/auth/lib/hashPassword";
import { IS_PRODUCTION, RESERVED_SUBDOMAINS } from "@shipmvp/lib/constants";
import { getTranslation } from "@shipmvp/lib/server/i18n";
import { prisma } from "@shipmvp/prisma";
import { MembershipRole } from "@shipmvp/prisma/enums";

import { TRPCError } from "@trpc/server";

import type { TrpcSessionUser } from "../../../trpc";
import type { TCreateInputSchema } from "./create.schema";

type CreateOptions = {
  ctx: {
    user: NonNullable<TrpcSessionUser>;
  };
  input: TCreateInputSchema;
};

const vercelCreateDomain = async (domain: string) => {
  const response = await fetch(
    `https://api.vercel.com/v8/projects/${process.env.PROJECT_ID_VERCEL}/domains?teamId=${process.env.TEAM_ID_VERCEL}`,
    {
      body: JSON.stringify({ name: `${domain}.` }),
      headers: {
        Authorization: `Bearer ${process.env.AUTH_BEARER_TOKEN_VERCEL}`,
        "Content-Type": "application/json",
      },
      method: "POST",
    }
  );

  const data = await response.json();

  // Domain is already owned by another team but you can request delegation to shipmvp it
  if (data.error?.code === "forbidden")
    throw new TRPCError({ code: "CONFLICT", message: "domain_taken_team" });

  // Domain is already being used by a different project
  if (data.error?.code === "domain_taken")
    throw new TRPCError({ code: "CONFLICT", message: "domain_taken_project" });

  return true;
};

export const createHandler = async ({ input }: CreateOptions) => {
  const { slug, name, adminEmail, adminUsername, check } = input;

  const userCollisions = await prisma.user.findUnique({
    where: {
      email: adminEmail,
    },
  });

  const slugCollisions = await prisma.team.findFirst({
    where: {
      slug: slug,
      metadata: {
        path: ["isOrganization"],
        equals: true,
      },
    },
  });

  if (slugCollisions || RESERVED_SUBDOMAINS.includes(slug))
    throw new TRPCError({ code: "BAD_REQUEST", message: "organization_url_taken" });
  if (userCollisions) throw new TRPCError({ code: "BAD_REQUEST", message: "admin_email_taken" });

  const password = createHash("md5")
    .update(`${adminEmail}${process.env.MY_APP_ENCRYPTION_KEY}`)
    .digest("hex");
  const hashedPassword = await hashPassword(password);

  const emailDomain = adminEmail.split("@")[1];

  if (check === false) {
    const createOwnerOrg = await prisma.user.create({
      data: {
        username: adminUsername,
        email: adminEmail,
        emailVerified: new Date(),
        password: hashedPassword,
        organization: {
          create: {
            name,
            metadata: {
              isOrganization: true,
              isOrganizationVerified: false,
              orgAutoAcceptEmail: emailDomain,
            },
          },
        },
      },
    });

    if (IS_PRODUCTION) await vercelCreateDomain(slug);

    await prisma.membership.create({
      data: {
        userId: createOwnerOrg.id,
        role: MembershipRole.OWNER,
        accepted: true,
        teamId: createOwnerOrg.organizationId!,
      },
    });

    return { user: { ...createOwnerOrg, password } };
  } else {
    const language = await getTranslation(input.language ?? "en", "common");

    const secret = createHash("md5")
      .update(adminEmail + process.env.MY_APP_ENCRYPTION_KEY)
      .digest("hex");

    totp.options = { step: 900 };
    const code = totp.generate(secret);

    await sendOrganizationEmailVerification({
      user: {
        email: adminEmail,
      },
      code,
      language,
    });
  }

  // Sync Services: Close.com
  //closeComUpsertOrganizationUser(createTeam, ctx.user, MembershipRole.OWNER);

  return { checked: true };
};
