import type { ShipMvpPageWrapper } from "@components/PageWrapper";
import PageWrapper from "@components/PageWrapper";
import UsersListingView from "@components/teams/users-listing-view";

const Page = UsersListingView as ShipMvpPageWrapper;
Page.PageWrapper = PageWrapper;

export default Page;
