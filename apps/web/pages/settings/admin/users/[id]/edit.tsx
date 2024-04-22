import type { ShipMvpPageWrapper } from "@components/PageWrapper";
import PageWrapper from "@components/PageWrapper";
import UsersEditView from "@components/teams/users-edit-view";

const Page = UsersEditView as ShipMvpPageWrapper;
Page.PageWrapper = PageWrapper;

export default Page;
