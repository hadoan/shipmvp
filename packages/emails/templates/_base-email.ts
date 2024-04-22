import { decodeHTML } from "entities";
import { createTransport } from "nodemailer";
import { z } from "zod";

import { getErrorFromUnknown } from "@shipmvp/lib/errors";
import { serverConfig } from "@shipmvp/lib/serverConfig";

export default class BaseEmail {
  name = "";

  protected getTimezone() {
    return "";
  }

  protected getNodeMailerPayload(): Record<string, unknown> {
    return {};
  }
  public async sendEmail() {
    const payload = this.getNodeMailerPayload();
    const parseSubject = z.string().safeParse(payload?.subject);
    const payloadWithUnEscapedSubject = {
      ...payload,
      ...(parseSubject.success && { subject: decodeHTML(parseSubject.data) }),
    };

    new Promise((resolve, reject) =>
      createTransport(this.getMailerOptions().transport).sendMail(
        payloadWithUnEscapedSubject,
        (_err, info) => {
          if (_err) {
            const err = getErrorFromUnknown(_err);
            this.printNodeMailerError(err);
            reject(err);
          } else {
            resolve(info);
          }
        }
      )
    ).catch((e) => console.error("sendEmail", e));
    return new Promise((resolve) => resolve("send mail async"));
  }

  protected getMailerOptions() {
    return {
      transport: serverConfig.transport,
      from: serverConfig.from,
    };
  }

  protected printNodeMailerError(error: Error): void {
    /** Don't clog the logs with unsent emails in E2E */
    if (process.env.NEXT_PUBLIC_IS_E2E) return;
    console.error(`${this.name}_ERROR`, error);
  }
}
