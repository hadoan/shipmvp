import { TooltipProvider } from "@radix-ui/react-tooltip";
import { SessionProvider } from "next-auth/react";
import { EventCollectionProvider } from "next-collect/client";
import { appWithTranslation } from "next-i18next";
import type { SSRConfig } from "next-i18next";
import type { AppProps as NextAppProps, AppProps as NextJsAppProps } from "next/app";
import type { NextRouter } from "next/router";
import type { ComponentProps, ReactNode } from "react";

import { trpc } from "@shipmvp/trpc/react";
import { MetaProvider } from "@shipmvp/ui";

import usePublicPage from "@lib/hooks/usePublicPage";
import type { WithNonceProps } from "@lib/withNonce";

import { useViewerI18n } from "@components/I18nLanguageHandler";

const I18nextAdapter = appWithTranslation<NextJsAppProps<SSRConfig> & { children: React.ReactNode }>(
  ({ children }) => <>{children}</>
);
// Workaround for https://github.com/vercel/next.js/issues/8592
export type AppProps = Omit<
  NextAppProps<WithNonceProps & { themeBasis?: string } & Record<string, unknown>>,
  "Component"
> & {
  Component: NextAppProps["Component"] & {
    getLayout?: (page: React.ReactElement, router: NextRouter) => ReactNode;
    PageWrapper?: (props: AppProps) => JSX.Element;
  };

  /** Will be defined only is there was an error */
  err?: Error;
};

type AppPropsWithChildren = AppProps & {
  children: ReactNode;
};

const CustomI18nextProvider = (props: AppPropsWithChildren) => {
  /**
   * i18n should never be clubbed with other queries, so that it's caching can be managed independently.
   * We intend to not cache i18n query
   **/
  const { i18n, locale } = useViewerI18n().data ?? {
    locale: "en",
  };

  const passedProps = {
    ...props,
    pageProps: {
      ...props.pageProps,
      ...i18n,
    },
    router: locale ? { locale } : props.router,
  } as unknown as ComponentProps<typeof I18nextAdapter>;
  return <I18nextAdapter {...passedProps} />;
};

const AppProviders = (props: AppPropsWithChildren) => {
  const session = trpc.viewer.public.session.useQuery().data;
  // No need to have intercom on public pages - Good for Page Performance
  const isPublicPage = usePublicPage();

  const RemainingProviders = (
    <EventCollectionProvider options={{ apiPath: "/api/collect-events" }}>
      <SessionProvider session={session || undefined}>
        <CustomI18nextProvider {...props}>
          <TooltipProvider>
            {/* color-scheme makes background:transparent not work which is required by embed. We need to ensure next-theme adds color-scheme to `body` instead of `html`(https://github.com/pacocoursey/next-themes/blob/main/src/index.tsx#L74). Once that's done we can enable color-scheme support */}
            <MetaProvider>{props.children}</MetaProvider>
          </TooltipProvider>
        </CustomI18nextProvider>
      </SessionProvider>
    </EventCollectionProvider>
  );

  if (isPublicPage) {
    return RemainingProviders;
  }

  return RemainingProviders;
};

export default AppProviders;
