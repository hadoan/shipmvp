import { serverSideTranslations } from "next-i18next/serverSideTranslations";
import { useRouter } from "next/router";
import { useEffect, useMemo, useState } from "react";

import Shell from "@shipmvp/features/shell/Shell";
import { WEBAPP_URL } from "@shipmvp/lib/constants";
import { useLocale } from "@shipmvp/lib/hooks/useLocale";
import { trpc } from "@shipmvp/trpc/react";
import { Alert, Button, Label, showToast } from "@shipmvp/ui";
import { Plus } from "@shipmvp/ui/components/icon";

import PageWrapper from "@components/PageWrapper";
import SkeletonLoaderTeamList from "@components/teams/SkeletonloaderTeamList";
import TeamList from "@components/teams/TeamList";

function Teams() {
  const { t } = useLocale();
  const [errorMessage, setErrorMessage] = useState("");
  const trpcContext = trpc.useContext();
  const router = useRouter();

  const [inviteTokenChecked, setInviteTokenChecked] = useState(false);

  const { data, isLoading } = trpc.viewer.teams.list.useQuery(undefined, {
    onError: (e) => {
      setErrorMessage(e.message);
    },
  });

  const { mutate: inviteMemberByToken } = trpc.viewer.teams.inviteMemberByToken.useMutation({
    onSuccess: (teamName) => {
      trpcContext.viewer.teams.list.invalidate();
      showToast(t("team_invite_received", { teamName }), "success");
    },
    onError: (e) => {
      showToast(e.message, "error");
    },
    onSettled: () => {
      setInviteTokenChecked(true);
    },
  });

  const teams = useMemo(() => data?.filter((m) => m.accepted) || [], [data]);
  const invites = useMemo(() => data?.filter((m) => !m.accepted) || [], [data]);

  useEffect(() => {
    if (!router) return;
    if (router.query.token) inviteMemberByToken({ token: router.query.token as string });
    else setInviteTokenChecked(true);
  }, [router, inviteMemberByToken, setInviteTokenChecked]);

  if (isLoading) {
    return <SkeletonLoaderTeamList />;
  }

  return (
    <Shell
      heading={t("teams")}
      hideHeadingOnMobile
      subtitle={t("create_manage_teams_collaborative")}
      CTA={
        <Button
          variant="fab"
          StartIcon={Plus}
          type="button"
          href={`${WEBAPP_URL}/settings/teams/new?returnTo=${WEBAPP_URL}/teams`}>
          {t("new")}
        </Button>
      }>
      {!!errorMessage && <Alert severity="error" title={errorMessage} />}
      {invites.length > 0 && (
        <div className="bg-subtle mb-6 rounded-md p-5">
          <Label className=" text-emphasis pb-2 font-semibold">{t("pending_invites")}</Label>
          <TeamList teams={invites} pending />
        </div>
      )}
      {teams.length > 0 ? <TeamList teams={teams} /> : <></>}
    </Shell>
  );
}

export const getStaticProps = async () => {
  return {
    props: {
      ...(await serverSideTranslations("en", ["common"])),
    },
  };
};

Teams.requiresLicense = false;
Teams.PageWrapper = PageWrapper;

export default Teams;
