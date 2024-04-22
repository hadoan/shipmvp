import Head from "next/head";

import { useLocale } from "@shipmvp/lib/hooks/useLocale";
import { WizardLayout } from "@shipmvp/ui";

import PageWrapper from "@components/PageWrapper";
import AddNewTeamMembers from "@components/teams/AddNewTeamMembers";

const OnboardTeamMembersPage = () => {
  const { t } = useLocale();
  return (
    <>
      <Head>
        <title>{t("add_team_members")}</title>
        <meta name="description" content={t("add_team_members_description")} />
      </Head>
      <AddNewTeamMembers />
    </>
  );
};

OnboardTeamMembersPage.getLayout = (page: React.ReactElement) => (
  <WizardLayout currentStep={2} maxSteps={2}>
    {page}
  </WizardLayout>
);

OnboardTeamMembersPage.PageWrapper = PageWrapper;

export default OnboardTeamMembersPage;
