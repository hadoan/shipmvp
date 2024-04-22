import type { GetServerSidePropsContext } from "next";
import { useRouter } from "next/router";

import { getServerSession } from "@shipmvp/features/auth/lib/getServerSession";
import { APP_NAME } from "@shipmvp/lib/constants";
import { useLocale } from "@shipmvp/lib/hooks/useLocale";
import prisma from "@shipmvp/prisma";
import { UserPermissionRole } from "@shipmvp/prisma/enums";
import type { inferSSRProps } from "@shipmvp/types/inferSSRProps";
import { Meta, WizardForm } from "@shipmvp/ui";

import PageWrapper from "@components/PageWrapper";
import { AdminUser } from "@components/setup/AdminUser";

import { ssrInit } from "@server/lib/ssr";

export function Setup(props: inferSSRProps<typeof getServerSideProps>) {
  const { t } = useLocale();
  const router = useRouter();
  const setStep = (newStep: number) => {
    router.replace(`/auth/setup?step=${newStep || 1}`, undefined, { shallow: true });
  };

  const steps: React.ComponentProps<typeof WizardForm>["steps"] = [
    {
      title: t("administrator_user"),
      description: t("lets_create_first_administrator_user"),
      content: (setIsLoading) => (
        <AdminUser
          onSubmit={() => {
            setIsLoading(true);
          }}
          onSuccess={() => {
            setStep(2);
          }}
          onError={() => {
            setIsLoading(false);
          }}
        />
      ),
    },
  ];

  steps.push({
    title: t("enable_apps"),
    description: t("enable_apps_description", { appName: APP_NAME }),
    contentClassname: "!pb-0 mb-[-1px]",
    content: (setIsLoading) => {
      const currentStep = 3;
      return <></>;
    },
  });

  return (
    <>
      <Meta title={t("setup")} description={t("setup_description")} />
      <main className="bg-subtle flex items-center md:h-screen print:h-full">
        <WizardForm
          href="/auth/setup"
          steps={steps}
          nextLabel={t("next_step_text")}
          finishLabel={t("finish")}
          prevLabel={t("prev_step")}
          stepLabel={(currentStep, maxSteps) => t("current_step_of_total", { currentStep, maxSteps })}
        />
      </main>
    </>
  );
}

Setup.PageWrapper = PageWrapper;
export default Setup;

export const getServerSideProps = async (context: GetServerSidePropsContext) => {
  const { req, res } = context;

  const ssr = await ssrInit(context);
  const userCount = await prisma.user.count();

  const session = await getServerSession({ req, res });

  if (session?.user.role && session?.user.role !== UserPermissionRole.ADMIN) {
    return {
      redirect: {
        destination: `/404`,
        permanent: false,
      },
    };
  }

  const isFreeLicense = true;

  return {
    props: {
      trpcState: ssr.dehydrate(),
      isFreeLicense,
      userCount,
    },
  };
};
