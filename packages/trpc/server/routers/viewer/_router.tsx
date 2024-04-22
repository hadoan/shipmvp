import { mergeRouters, router } from "../../trpc";
import { loggedInViewerRouter } from "../loggedInViewer/_router";
import { publicViewerRouter } from "../publicViewer/_router";
import { authRouter } from "./auth/_router";
import { googleWorkspaceRouter } from "./googleWorkspace/_router";
import { viewerTeamsRouter } from "./teams/_router";
import { userAdminRouter } from "./users/trpc-router";

export const viewerRouter = mergeRouters(
  loggedInViewerRouter,
  router({
    loggedInViewerRouter,
    public: publicViewerRouter,
    auth: authRouter,
    googleWorkspace: googleWorkspaceRouter,
    teams: viewerTeamsRouter,
    users: userAdminRouter,
  })
);
