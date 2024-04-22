import { TFunction } from "next-i18next";

import { renderEmail } from "..";
import BaseEmail from "./_base-email";

export default class DisabledAppEmail extends BaseEmail {
  email: string;
  appName: string;
  appType: string[];
  t: TFunction;
  title?: string;

  constructor(email: string, appName: string, appType: string[], t: TFunction, title?: string) {
    super();
    this.email = email;
    this.appName = appName;
    this.appType = appType;
    this.t = t;
    this.title = title;
  }

  protected getNodeMailerPayload(): Record<string, unknown> {
    return {
      from: `Tekfriend.co <${this.getMailerOptions().from}>`,
      to: this.email,
      subject: this.title
        ? this.t("disabled_app_affects_event_type", { appName: this.appName })
        : this.t("admin_has_disabled", { appName: this.appName }),
      html: renderEmail("DisabledAppEmail", {
        title: this.title,
        appName: this.appName,
        appType: this.appType,
        t: this.t,
      }),
      text: this.getTextBody(),
    };
  }

  protected getTextBody(): string {
    return this.appType.some((type) => type === "payment")
      ? this.t("disable_payment_app", { appName: this.appName, title: this.title })
      : this.appType.some((type) => type === "video")
      ? this.t("app_disabled_video", { appName: this.appName })
      : this.t("app_disabled", { appName: this.appName });
  }
}
