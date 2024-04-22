import { TFunction } from "next-i18next";

import { MY_APP_URL } from "@shipmvp/lib/constants";

import { BaseEmailHtml, CallToAction } from "../components";

export const DisabledAppEmail = (
  props: {
    appName: string;
    appType: string[];
    t: TFunction;
    title?: string;
  } & Partial<React.ComponentProps<typeof BaseEmailHtml>>
) => {
  const { title, appName, t, appType } = props;

  return (
    <BaseEmailHtml subject={t("app_disabled", { appName: appName })}>
      {appType.some((type) => type === "payment") ? (
        <>
          <p>
            <>{t("disabled_app_affects_event_type", { appName: appName })}</>
          </p>
          <p style={{ fontWeight: 400, lineHeight: "24px" }}>
            <>{t("payment_disabled_still_able_to_book")}</>
          </p>

          <hr style={{ marginBottom: "24px" }} />

          <CallToAction label={t("edit_event_type")} href={`${MY_APP_URL}`} />
        </>
      ) : title ? (
        <>
          <p>
            <>{(t("app_disabled_with_event_type"), { appName: appName, title: title })}</>
          </p>

          <hr style={{ marginBottom: "24px" }} />

          <CallToAction label={t("edit_event_type")} href={`${MY_APP_URL}`} />
        </>
      ) : appType.some((type) => type === "video") ? (
        <>
          <p>
            <>{t("app_disabled_video", { appName: appName })}</>
          </p>

          <hr style={{ marginBottom: "24px" }} />

          <CallToAction label={t("navigate_installed_apps")} href={`${MY_APP_URL}/apps/installed`} />
        </>
      ) : appType.some((type) => type === "calendar") ? (
        <>
          <p>
            <>{t("admin_has_disabled", { appName: appName })}</>
          </p>
          <p style={{ fontWeight: 400, lineHeight: "24px" }}>
            <>{t("disabled_calendar")}</>
          </p>

          <hr style={{ marginBottom: "24px" }} />

          <CallToAction label={t("navigate_installed_apps")} href={`${MY_APP_URL}/apps/installed`} />
        </>
      ) : (
        <>
          <p>
            <>{t("admin_has_disabled", { appName: appName })}</>
          </p>

          <hr style={{ marginBottom: "24px" }} />

          <CallToAction label={t("navigate_installed_apps")} href={`${MY_APP_URL}/apps/installed`} />
        </>
      )}
    </BaseEmailHtml>
  );
};
