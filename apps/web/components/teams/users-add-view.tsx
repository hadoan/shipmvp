import { useRouter } from "next/router";

import { getLayout } from "@shipmvp/features/settings/layouts/SettingsLayout";
import { getParserWithGeneric } from "@shipmvp/prisma/zod-utils";
import { trpc } from "@shipmvp/trpc/react";
import { Meta, showToast } from "@shipmvp/ui";

import { userBodySchema } from "@lib/schemas/userBodySchema";

import { UserForm } from "@components/users/UserForm";

const UsersAddView = () => {
  const router = useRouter();
  const utils = trpc.useContext();
  const mutation = trpc.viewer.users.add.useMutation({
    onSuccess: async () => {
      showToast("User added successfully", "success");
      await utils.viewer.users.list.invalidate();
      router.replace(router.asPath.replace("/add", ""));
    },
    onError: (err) => {
      console.error(err.message);
      showToast("There has been an error adding this user.", "error");
    },
  });
  return (
    <>
      <Meta title="Add new user" description="Here you can add a new user." />
      <UserForm
        submitLabel="Add user"
        onSubmit={async (values) => {
          const parser = getParserWithGeneric(userBodySchema);
          const parsedValues = parser(values);
          mutation.mutate(parsedValues as any);
        }}
      />
    </>
  );
};

UsersAddView.getLayout = getLayout;

export default UsersAddView;
