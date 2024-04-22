import type { UserPermissionRole } from "@prisma/client";

import { hashPassword } from "@shipmvp/features/auth/lib/hashPassword";

import prisma from ".";

// import mainAppStore from "./seed-app-store";

async function createUser(opts: {
  user: {
    email: string;
    password: string;
    username: string;
    name: string;
    completedOnboarding?: boolean;
    timeZone?: string;
    role?: UserPermissionRole;
  };
}) {
  const userData = {
    ...opts.user,
    password: await hashPassword(opts.user.password),
    emailVerified: new Date(),
    completedOnboarding: opts.user.completedOnboarding ?? true,
    locale: "en",
  };

  const user = await prisma.user.upsert({
    where: { email_username: { email: opts.user.email, username: opts.user.username } },
    update: userData,
    create: userData,
  });

  console.log(
    `👤 Upserted '${opts.user.username}' with email "${opts.user.email}" & password "${opts.user.password}". Booking page 👉 ${process.env.NEXT_PUBLIC_WEBAPP_URL}/${opts.user.username}`
  );

  return user;
}

async function main() {
  await createUser({
    user: {
      email: "test@example.com",
      password: "test",
      username: "test",
      name: "Test Example",
    },
  });

  await createUser({
    user: {
      email: "admin@example.com",
      /** To comply with admin password requirements  */
      password: "ADMINadmin2022!",
      username: "admin",
      name: "Admin Example",
      role: "ADMIN",
    },
  });
}

main()
  //   .then(() => mainAppStore())
  .then(() => {
    console.log("ok");
  })
  .catch((e) => {
    console.error(e);
    process.exit(1);
  })
  .finally(async () => {
    await prisma.$disconnect();
  });
