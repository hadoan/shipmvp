import authedProcedure from "../../procedures/authedProcedure";
import { router } from "../../trpc";
import { ZDeleteMeInputSchema } from "./deleteMe.schema";
import { ZUpdateProfileInputSchema } from "./updateProfile.schema";

type AppsRouterHandlerCache = {
  me?: typeof import("./me.handler").meHandler;
  updateProfile?: typeof import("./updateProfile.handler").updateProfileHandler;
  avatar?: typeof import("./avatar.handler").avatarHandler;
  deleteMe?: typeof import("./deleteMe.handler").deleteMeHandler;
  deleteMeWithoutPassword?: typeof import("./deleteMeWithoutPassword.handler").deleteMeWithoutPasswordHandler;
};

const UNSTABLE_HANDLER_CACHE: AppsRouterHandlerCache = {};

export const loggedInViewerRouter = router({
  me: authedProcedure.query(async ({ ctx }) => {
    if (!UNSTABLE_HANDLER_CACHE.me) {
      UNSTABLE_HANDLER_CACHE.me = (await import("./me.handler")).meHandler;
    }

    // Unreachable code but required for type safety
    if (!UNSTABLE_HANDLER_CACHE.me) {
      throw new Error("Failed to load handler");
    }

    return UNSTABLE_HANDLER_CACHE.me({ ctx });
  }),
  avatar: authedProcedure.query(async ({ ctx }) => {
    if (!UNSTABLE_HANDLER_CACHE.avatar) {
      UNSTABLE_HANDLER_CACHE.avatar = (await import("./avatar.handler")).avatarHandler;
    }

    // Unreachable code but required for type safety
    if (!UNSTABLE_HANDLER_CACHE.avatar) {
      throw new Error("Failed to load handler");
    }

    return UNSTABLE_HANDLER_CACHE.avatar({ ctx });
  }),
  updateProfile: authedProcedure.input(ZUpdateProfileInputSchema).mutation(async ({ ctx, input }) => {
    if (!UNSTABLE_HANDLER_CACHE.updateProfile) {
      UNSTABLE_HANDLER_CACHE.updateProfile = (await import("./updateProfile.handler")).updateProfileHandler;
    }

    // Unreachable code but required for type safety
    if (!UNSTABLE_HANDLER_CACHE.updateProfile) {
      throw new Error("Failed to load handler");
    }

    return UNSTABLE_HANDLER_CACHE.updateProfile({ ctx, input });
  }),

  deleteMe: authedProcedure.input(ZDeleteMeInputSchema).mutation(async ({ ctx, input }) => {
    if (!UNSTABLE_HANDLER_CACHE.deleteMe) {
      UNSTABLE_HANDLER_CACHE.deleteMe = (await import("./deleteMe.handler")).deleteMeHandler;
    }

    // Unreachable code but required for type safety
    if (!UNSTABLE_HANDLER_CACHE.deleteMe) {
      throw new Error("Failed to load handler");
    }

    return UNSTABLE_HANDLER_CACHE.deleteMe({ ctx, input });
  }),

  deleteMeWithoutPassword: authedProcedure.mutation(async ({ ctx }) => {
    if (!UNSTABLE_HANDLER_CACHE.deleteMeWithoutPassword) {
      UNSTABLE_HANDLER_CACHE.deleteMeWithoutPassword = (
        await import("./deleteMeWithoutPassword.handler")
      ).deleteMeWithoutPasswordHandler;
    }

    // Unreachable code but required for type safety
    if (!UNSTABLE_HANDLER_CACHE.deleteMeWithoutPassword) {
      throw new Error("Failed to load handler");
    }

    return UNSTABLE_HANDLER_CACHE.deleteMeWithoutPassword({ ctx });
  }),
});
