import logger from "@shipmvp/lib/logger";
import type { Calendar } from "@shipmvp/types/Calendar";
import type { CredentialPayload } from "@shipmvp/types/Credential";

import appStore from "..";

const log = logger.getChildLogger({ prefix: ["CalendarManager"] });

export const getCalendar = async (credential: CredentialPayload | null): Promise<Calendar | null> => {
  if (!credential || !credential.key) return null;
  let { type: calendarType } = credential;
  if (calendarType?.endsWith("_other_calendar")) {
    calendarType = calendarType.split("_other_calendar")[0];
  }
  const calendarAppImportFn = appStore[calendarType.split("_").join("") as keyof typeof appStore];

  if (!calendarAppImportFn) {
    log.warn(`calendar of type ${calendarType} is not implemented`);
    return null;
  }

  const calendarApp = (await calendarAppImportFn()) as any;
  if (!(calendarApp && "lib" in calendarApp && "CalendarService" in calendarApp.lib)) {
    log.warn(`calendar of type ${calendarType} is not implemented`);
    return null;
  }
  log.info("calendarApp", calendarApp.lib.CalendarService);
  const CalendarService = calendarApp.lib.CalendarService;
  return new CalendarService(credential);
};
