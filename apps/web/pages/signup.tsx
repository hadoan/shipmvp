import React from "react";
import type { CSSProperties } from "react";
import { FormProvider, useForm } from "react-hook-form";

import { Alert, Button, EmailField, PasswordField, TextField } from "@shipmvp/ui";

import PageWrapper from "@components/PageWrapper";

type FormValues = {
  username: string;
  email: string;
  password: string;
  apiError: string;
  token?: string;
};

//type SignupProps = inferSSRProps<typeof getServerSideProps>;

export default function Signup() {
  const methods = useForm<FormValues>({
    defaultValues: undefined,
  });
  const {
    register,
    formState: { errors, isSubmitting },
  } = methods;

  return (
    <div
      className="bg-muted flex min-h-screen flex-col justify-center "
      style={
        {
          "--shipmvp-brand": "#111827",
          "--shipmvp-brand-emphasis": "#101010",
          "--shipmvp-brand-text": "white",
          "--shipmvp-brand-subtle": "#9CA3AF",
        } as CSSProperties
      }
      aria-labelledby="modal-title"
      role="dialog"
      aria-modal="true">
      <div className="sm:mx-auto sm:w-full sm:max-w-md">
        <h2 className="font-myapp text-emphasis text-center text-3xl font-extrabold">Create Your Account</h2>
      </div>
      <div className="mt-8 sm:mx-auto sm:w-full sm:max-w-md">
        <div className="bg-default mx-2 p-6 shadow sm:rounded-lg lg:p-8">
          <FormProvider {...methods}>
            <form
              onSubmit={(event) => {
                event.preventDefault();
                event.stopPropagation();
              }}
              className="bg-default space-y-6">
              {errors.apiError && <Alert severity="error" message={errors.apiError?.message} />}
              <div className="space-y-4">
                <TextField
                  id="username"
                  label="User name"
                  placeholder="User name"
                  {...register("username")}
                  required
                />

                <EmailField
                  id="email"
                  label="Email"
                  placeholder="Email"
                  {...register("email")}
                  className="disabled:bg-emphasis disabled:hover:cursor-not-allowed"
                  required
                />
                <PasswordField
                  label="Password"
                  id="password"
                  labelProps={{
                    className: "block text-sm font-medium text-default",
                  }}
                  {...register("password")}
                  required
                  //  hintErrors={["caplow", "min", "num"]}
                  className="border-default mt-1 block w-full rounded-md border px-3 py-2 shadow-sm focus:border-black focus:outline-none focus:ring-black sm:text-sm"
                />
              </div>
              <div className="flex space-x-2 rtl:space-x-reverse">
                <Button type="submit" loading={isSubmitting} className="w-full justify-center">
                  Create account
                </Button>

                <Button color="secondary" className="w-full justify-center">
                  Login Instead
                </Button>
              </div>
            </form>
          </FormProvider>
        </div>
      </div>
    </div>
  );
}
Signup.isThemeSupported = false;
Signup.PageWrapper = PageWrapper;
