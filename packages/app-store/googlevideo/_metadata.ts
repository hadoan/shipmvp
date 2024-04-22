import { validJson } from "@shipmvp/lib/jsonUtils";
import type { AppMeta } from "@shipmvp/types/App";

import _package from "./package.json";

export const metadata = {
  name: "Google Meet",
  description: _package.description,
  installed: !!(process.env.GOOGLE_API_CREDENTIALS && validJson(process.env.GOOGLE_API_CREDENTIALS)),
  slug: "google-meet",
  category: "conferencing",
  categories: ["conferencing"],
  type: "google_video",
  title: "Google Meet",
  variant: "conferencing",
  logo: "logo.webp",
  publisher: "Tekfriend.co",
  url: "https://shipmvp.co/",
  isGlobal: false,
  email: "help@shipmvp.co",
  appData: {
    location: {
      linkType: "dynamic",
      type: "integrations:google:meet",
      label: "Google Meet",
    },
  },
  dirName: "googlevideo",
  dependencies: ["google-calendar"],
} as AppMeta;

export default metadata;
