import { IdentityProvider } from "@shipmvp/prisma/enums";

export const identityProviderNameMap: { [key in IdentityProvider]: string } = {
  [IdentityProvider.TEK]: "Tekfriend",
  [IdentityProvider.GOOGLE]: "Google",
  [IdentityProvider.SAML]: "SAML",
};
