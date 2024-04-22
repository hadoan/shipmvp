import { useLocale } from "@shipmvp/lib/hooks/useLocale";
import { Badge } from "@shipmvp/ui";

export function TeamInviteBadge() {
  const { t } = useLocale();

  return <Badge variant="default">{t("invite_team_notifcation_badge")}</Badge>;
}
