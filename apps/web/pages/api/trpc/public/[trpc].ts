import { createNextApiHandler } from "@shipmvp/trpc/server/createNextApiHandler";
import { publicViewerRouter } from "@shipmvp/trpc/server/routers/publicViewer/_router";

export default createNextApiHandler(publicViewerRouter, true);
