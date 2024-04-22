import type { TFunction } from "next-i18next";

import type BaseEmail from "@shipmvp/emails/templates/_base-email";

import type { EmailVerifyLink } from "./templates/account-verify-email";
import AccountVerifyEmail from "./templates/account-verify-email";
import DisabledAppEmail from "./templates/disabled-app-email";
import type { Feedback } from "./templates/feedback-email";
import FeedbackEmail from "./templates/feedback-email";
import type { PasswordReset } from "./templates/forgot-password-email";
import ForgotPasswordEmail from "./templates/forgot-password-email";
import type { OrgAutoInvite } from "./templates/org-auto-join-invite";
import OrgAutoJoinEmail from "./templates/org-auto-join-invite";
import TeamInviteEmail from "./templates/team-invite-email";
import type { TeamInvite } from "./templates/team-invite-email";

const sendEmail = (prepare: () => BaseEmail) => {
  return new Promise((resolve, reject) => {
    try {
      const email = prepare();
      resolve(email.sendEmail());
    } catch (e) {
      reject(console.error(`${prepare.constructor.name}.sendEmail failed`, e));
    }
  });
};

export const sendPasswordResetEmail = async (passwordResetEvent: PasswordReset) => {
  await sendEmail(() => new ForgotPasswordEmail(passwordResetEvent));
};

export const sendEmailVerificationLink = async (verificationInput: EmailVerifyLink) => {
  await sendEmail(() => new AccountVerifyEmail(verificationInput));
};

export const sendFeedbackEmail = async (feedback: Feedback) => {
  await sendEmail(() => new FeedbackEmail(feedback));
};
export const sendDisabledAppEmail = async ({
  email,
  appName,
  appType,
  t,
  title = undefined,
}: {
  email: string;
  appName: string;
  appType: string[];
  t: TFunction;
  title?: string;
}) => {
  await sendEmail(() => new DisabledAppEmail(email, appName, appType, t, title));
};

export const sendTeamInviteEmail = async (teamInviteEvent: TeamInvite) => {
  await sendEmail(() => new TeamInviteEmail(teamInviteEvent));
};

export const sendOrganizationAutoJoinEmail = async (orgInviteEvent: OrgAutoInvite) => {
  await sendEmail(() => new OrgAutoJoinEmail(orgInviteEvent));
};
