import NextAuth from "next-auth";

import { AUTH_OPTIONS } from "@shipmvp/features/auth/lib/next-auth-options";

export default NextAuth(AUTH_OPTIONS);
