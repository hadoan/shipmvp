import type { ShipMvpPageWrapper } from "@components/PageWrapper";
import PageWrapper from "@components/PageWrapper";
import UsersAddView from "@components/teams/users-add-view";

const Page = UsersAddView as ShipMvpPageWrapper;
Page.PageWrapper = PageWrapper;

export default Page;
