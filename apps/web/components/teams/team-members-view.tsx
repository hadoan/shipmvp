import { useSession } from "next-auth/react";
import { useRouter } from "next/router";
import { useState } from "react";

import { getLayout } from "@shipmvp/features/settings/layouts/SettingsLayout";
import { useLocale } from "@shipmvp/lib/hooks/useLocale";
import { MembershipRole } from "@shipmvp/prisma/enums";
import { trpc } from "@shipmvp/trpc/react";
import type { RouterOutputs } from "@shipmvp/trpc/react";
import { Button, Meta, TextField, showToast } from "@shipmvp/ui";
import { Plus } from "@shipmvp/ui/components/icon";

import InviteLinkSettingsModal from "./InviteLinkSettingsModal";
import MemberInvitationModal from "./MemberInvitationModal";
import MemberListItem from "./MemberListItem";
import TeamInviteList from "./TeamInviteList";

type Team = RouterOutputs["viewer"]["teams"]["get"];

interface MembersListProps {
  team: Team | undefined;
}

const checkIfExist = (comp: string, query: string) =>
  comp.toLowerCase().replace(/\s+/g, "").includes(query.toLowerCase().replace(/\s+/g, ""));

function MembersList(props: MembersListProps) {
  const { team } = props;
  const { t } = useLocale();
  const [query, setQuery] = useState<string>("");

  const members = team?.members;
  const membersList = members
    ? members && query === ""
      ? members
      : members.filter((member) => {
          const email = member.email ? checkIfExist(member.email, query) : false;
          const username = member.username ? checkIfExist(member.username, query) : false;
          const name = member.name ? checkIfExist(member.name, query) : false;

          return email || username || name;
        })
    : undefined;
  return (
    <div className="flex flex-col gap-y-3">
      <TextField
        type="search"
        autoComplete="false"
        onChange={(e) => setQuery(e.target.value)}
        value={query}
        placeholder={`${t("search")}...`}
      />
      {membersList?.length && team ? (
        <ul className="divide-subtle border-subtle divide-y rounded-md border ">
          {membersList.map((member) => {
            return <MemberListItem key={member.id} team={team} member={member} />;
          })}
        </ul>
      ) : null}
    </div>
  );
}

const MembersView = () => {
  const { t, i18n } = useLocale();

  const router = useRouter();
  const session = useSession();
  const utils = trpc.useContext();
  const teamId = Number(router.query.id);

  const showDialog = router.query.inviteModal === "true";
  const [showMemberInvitationModal, setShowMemberInvitationModal] = useState(showDialog);
  const [showInviteLinkSettingsModal, setInviteLinkSettingsModal] = useState(false);

  const { data: team, isLoading } = trpc.viewer.teams.get.useQuery(
    { teamId },
    {
      onError: () => {
        router.push("/settings");
      },
    }
  );

  const inviteMemberMutation = trpc.viewer.teams.inviteMember.useMutation();

  const isInviteOpen = !team?.membership.accepted;

  const isAdmin =
    team && (team.membership.role === MembershipRole.OWNER || team.membership.role === MembershipRole.ADMIN);

  return (
    <>
      <Meta
        title={t("team_members")}
        description={t("members_team_description")}
        CTA={
          isAdmin ? (
            <Button
              type="button"
              color="primary"
              StartIcon={Plus}
              className="ml-auto"
              onClick={() => setShowMemberInvitationModal(true)}
              data-testid="new-member-button">
              {t("add")}
            </Button>
          ) : (
            <></>
          )
        }
      />
      {!isLoading && (
        <>
          <div>
            {team && (
              <>
                {isInviteOpen && (
                  <TeamInviteList
                    teams={[
                      {
                        id: team.id,
                        accepted: team.membership.accepted || false,
                        logo: team.logo,
                        name: team.name,
                        slug: team.slug,
                        role: team.membership.role,
                      },
                    ]}
                  />
                )}
              </>
            )}
            <MembersList team={team} />
            <hr className="border-subtle my-8" />

            <hr className="border-subtle my-8" />
          </div>
          {showMemberInvitationModal && team && (
            <MemberInvitationModal
              isOpen={showMemberInvitationModal}
              members={team.members}
              teamId={team.id}
              token={team.inviteToken?.token}
              onExit={() => setShowMemberInvitationModal(false)}
              onSubmit={(values, resetFields) => {
                inviteMemberMutation.mutate(
                  {
                    teamId,
                    language: i18n.language,
                    role: values.role,
                    usernameOrEmail: values.emailOrUsername,
                    sendEmailInvitation: values.sendInviteEmail,
                  },
                  {
                    onSuccess: async (data) => {
                      await utils.viewer.teams.get.invalidate();
                      setShowMemberInvitationModal(false);
                      if (data.sendEmailInvitation) {
                        if (Array.isArray(data.usernameOrEmail)) {
                          showToast(
                            t("email_invite_team_bulk", {
                              userCount: data.usernameOrEmail.length,
                            }),
                            "success"
                          );
                          resetFields();
                        } else {
                          showToast(
                            t("email_invite_team", {
                              email: data.usernameOrEmail,
                            }),
                            "success"
                          );
                        }
                      }
                    },
                    onError: (error) => {
                      showToast(error.message, "error");
                    },
                  }
                );
              }}
              onSettingsOpen={() => {
                setShowMemberInvitationModal(false);
                setInviteLinkSettingsModal(true);
              }}
            />
          )}
          {showInviteLinkSettingsModal && team?.inviteToken && (
            <InviteLinkSettingsModal
              isOpen={showInviteLinkSettingsModal}
              teamId={team.id}
              token={team.inviteToken.token}
              expiresInDays={team.inviteToken.expiresInDays || undefined}
              onExit={() => {
                setInviteLinkSettingsModal(false);
                setShowMemberInvitationModal(true);
              }}
            />
          )}
        </>
      )}
    </>
  );
};

MembersView.getLayout = getLayout;

export default MembersView;
