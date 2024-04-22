import classNames from "classnames";

import { useLocale } from "@shipmvp/lib/hooks/useLocale";
import { MembershipRole } from "@shipmvp/prisma/enums";

type PillColor = "blue" | "green" | "red" | "orange";

interface Props {
  text: string;
  color?: PillColor;
}

export default function TeamPill(props: Props) {
  return (
    <div
      className={classNames("text-medium self-center rounded-md px-1 py-0.5 text-xs ltr:mr-1 rtl:ml-1", {
        " bg-subtle text-emphasis": !props.color,
        " bg-info text-blue-800": props.color === "blue",
        " bg-error text-red-800 ": props.color === "red",
        " bg-attention text-orange-800": props.color === "orange",
      })}>
      {props.text}
    </div>
  );
}

export function TeamRole(props: { role: MembershipRole }) {
  const { t } = useLocale();
  const keys: Record<MembershipRole, PillColor | undefined> = {
    [MembershipRole.OWNER]: "blue",
    [MembershipRole.ADMIN]: "red",
    [MembershipRole.MEMBER]: undefined,
  };
  return <TeamPill text={t(props.role.toLowerCase())} color={keys[props.role]} />;
}
