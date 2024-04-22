import Head from "next/head";

import { useLocale } from "@shipmvp/lib/hooks/useLocale";
import { WizardLayout } from "@shipmvp/ui";

import PageWrapper from "@components/PageWrapper";
import { CreateANewTeamForm } from "@components/teams/CreateANewTeamForm";

const CreateNewTeamPage = () => {
  const { t } = useLocale();
  return (
    <>
      <Head>
        <title>{t("create_new_team")}</title>
        <meta name="description" content={t("create_new_team_description")} />
      </Head>
      <CreateANewTeamForm />
    </>
  );
};
const LayoutWrapper = (page: React.ReactElement) => {
  return (
    <WizardLayout currentStep={1} maxSteps={2}>
      {page}
    </WizardLayout>
  );
};

CreateNewTeamPage.getLayout = LayoutWrapper;
CreateNewTeamPage.PageWrapper = PageWrapper;

export default CreateNewTeamPage;
