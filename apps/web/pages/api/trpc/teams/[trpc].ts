import { createNextApiHandler } from "@shipmvp/trpc/server/createNextApiHandler";
import { viewerTeamsRouter } from "@shipmvp/trpc/server/routers/viewer/teams/_router";

export default createNextApiHandler(viewerTeamsRouter);
