import { validJson } from "@shipmvp/lib/jsonUtils";
import type { AppMeta } from "@shipmvp/types/App";

import _package from "./package.json";

export const metadata = {
  name: "Google Calendar",
  description: _package.description,
  installed: !!(process.env.GOOGLE_API_CREDENTIALS && validJson(process.env.GOOGLE_API_CREDENTIALS)),
  type: "google_calendar",
  title: "Google Calendar",
  variant: "calendar",
  category: "calendar",
  categories: ["calendar"],
  logo: "icon.svg",
  publisher: "Tekfriend.co",
  slug: "google-calendar",
  url: "https://shipmvp.co/",
  email: "help@shipmvp.co",
  dirName: "googlecalendar",
} as AppMeta;

export default metadata;
