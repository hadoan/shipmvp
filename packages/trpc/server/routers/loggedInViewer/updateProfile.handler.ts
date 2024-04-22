import type { Prisma } from "@prisma/client";
import type { NextApiResponse, GetServerSidePropsContext } from "next";

import { checkUsername } from "@shipmvp/lib/server/checkUsername";
import { resizeBase64Image } from "@shipmvp/lib/server/resizeBase64Image";
import slugify from "@shipmvp/lib/slugify";
import { updateWebUser as syncServicesUpdateWebUser } from "@shipmvp/lib/sync/SyncServiceManager";
import { prisma } from "@shipmvp/prisma";
import type { TrpcSessionUser } from "@shipmvp/trpc/server/trpc";

import { TRPCError } from "@trpc/server";

import type { TUpdateProfileInputSchema } from "./updateProfile.schema";

type UpdateProfileOptions = {
  ctx: {
    user: NonNullable<TrpcSessionUser>;
    res?: NextApiResponse | GetServerSidePropsContext["res"];
  };
  input: TUpdateProfileInputSchema;
};

export const updateProfileHandler = async ({ ctx, input }: UpdateProfileOptions) => {
  const { user } = ctx;
  const data: Prisma.UserUpdateInput = {
    ...input,
    metadata: input.metadata as Prisma.InputJsonValue,
  };

  if (input.username && !user.organizationId) {
    const username = slugify(input.username);
    // Only validate if we're changing usernames
    if (username !== user.username) {
      data.username = username;
      const response = await checkUsername(username);
      if (!response.available) {
        throw new TRPCError({ code: "BAD_REQUEST", message: response.message });
      }
    }
  }
  if (input.avatar) {
    data.avatar = await resizeBase64Image(input.avatar);
  }
  const userToUpdate = await prisma.user.findUnique({
    where: {
      id: user.id,
    },
  });

  if (!userToUpdate) {
    throw new TRPCError({ code: "NOT_FOUND", message: "User not found" });
  }
  const updatedUser = await prisma.user.update({
    where: {
      id: user.id,
    },
    data,
    select: {
      id: true,
      username: true,
      email: true,
      metadata: true,
      name: true,
      createdDate: true,
    },
  });

  // Sync Services
  await syncServicesUpdateWebUser(updatedUser);

  return input;
};
