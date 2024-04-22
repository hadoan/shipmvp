import type { ShipMvpPageWrapper } from "@components/PageWrapper";
import PageWrapper from "@components/PageWrapper";
import TeamMembersView from "@components/teams/team-members-view";

const Page = TeamMembersView as ShipMvpPageWrapper;
Page.PageWrapper = PageWrapper;

export default Page;
