import type { ShipMvpPageWrapper } from "@components/PageWrapper";
import PageWrapper from "@components/PageWrapper";
import TeamProfileView from "@components/teams/team-profile-view";

const Page = TeamProfileView as ShipMvpPageWrapper;
Page.PageWrapper = PageWrapper;

export default Page;
