import { createNextApiHandler } from "@shipmvp/trpc/server/createNextApiHandler";
import { loggedInViewerRouter } from "@shipmvp/trpc/server/routers/loggedInViewer/_router";

export default createNextApiHandler(loggedInViewerRouter);
