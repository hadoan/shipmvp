import type { User as UserAuth } from "next-auth";
import { signOut, useSession } from "next-auth/react";
import Link from "next/link";
import type { NextRouter } from "next/router";
import { useRouter } from "next/router";
import type { Dispatch, ReactNode, SetStateAction } from "react";
import React, { Fragment, useEffect, useState, useRef } from "react";
import { Toaster } from "react-hot-toast";

import { KBarContent, KBarRoot, KBarTrigger } from "@shipmvp/features/kbar/Kbar";
import AdminPasswordBanner from "@shipmvp/features/users/components/AdminPasswordBanner";
import VerifyEmailBanner from "@shipmvp/features/users/components/VerifyEmailBanner";
import classNames from "@shipmvp/lib/classNames";
import { APP_NAME, WEBAPP_URL } from "@shipmvp/lib/constants";
import getBrandColours from "@shipmvp/lib/getBrandColours";
import { useIsomorphicLayoutEffect } from "@shipmvp/lib/hooks/useIsomorphicLayoutEffect";
import { useLocale } from "@shipmvp/lib/hooks/useLocale";
import type { User } from "@shipmvp/prisma/client";
import { trpc } from "@shipmvp/trpc/react";
import useAvatarQuery from "@shipmvp/trpc/react/hooks/useAvatarQuery";
import useEmailVerifyCheck from "@shipmvp/trpc/react/hooks/useEmailVerifyCheck";
import useMeQuery from "@shipmvp/trpc/react/hooks/useMeQuery";
import type { SVGComponent } from "@shipmvp/types/SVGComponent";
import {
  Avatar,
  Button,
  Dropdown,
  DropdownItem,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuPortal,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
  ErrorBoundary,
  HeadSeo,
  Logo,
  SkeletonText,
  Tooltip,
  showToast,
  useAppDefaultTheme,
  ButtonOrLink,
} from "@shipmvp/ui";
import {
  ArrowLeft,
  ArrowRight,
  BarChart,
  Calendar,
  Clock,
  ExternalLink,
  FileText,
  Grid,
  HelpCircle,
  Link as LinkIcon,
  LogOut,
  Moon,
  MoreHorizontal,
  ChevronDown,
  Copy,
  Settings,
  Users,
  Zap,
  User as UserIcon,
} from "@shipmvp/ui/components/icon";

import { TeamInviteBadge } from "./TeamInviteBadge";

export const ONBOARDING_NEXT_REDIRECT = {
  redirect: {
    permanent: false,
    destination: "/getting-started",
  },
} as const;

export const shouldShowOnboarding = (
  user: Pick<User, "createdDate" | "completedOnboarding" | "organizationId">
) => {
  return !user.completedOnboarding && !user.organizationId;
};

function useRedirectToLoginIfUnauthenticated(isPublic = false) {
  const { data: session, status } = useSession();
  const loading = status === "loading";
  const router = useRouter();

  useEffect(() => {
    if (isPublic) {
      return;
    }

    if (!loading && !session) {
      router.replace({
        pathname: "/auth/login",
        query: {
          callbackUrl: `${WEBAPP_URL}${location.pathname}${location.search}`,
        },
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loading, session, isPublic]);

  return {
    loading: loading && !session,
    session,
  };
}

function useRedirectToOnboardingIfNeeded() {
  const router = useRouter();
  const query = useMeQuery();
  const user = query.data;
  // const flags = useFlagMap();

  const { data: email } = useEmailVerifyCheck();

  const needsEmailVerification = !email?.isVerified;

  const isRedirectingToOnboarding = user && shouldShowOnboarding(user);

  useEffect(() => {
    if (isRedirectingToOnboarding && !needsEmailVerification) {
      router.replace({
        pathname: "/getting-started",
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isRedirectingToOnboarding, needsEmailVerification]);

  return {
    isRedirectingToOnboarding,
  };
}

const Layout = (props: LayoutProps) => {
  const pageTitle = typeof props.heading === "string" && !props.title ? props.heading : props.title;
  const bannerRef = useRef<HTMLDivElement | null>(null);
  const [bannersHeight, setBannersHeight] = useState<number>(0);

  useIsomorphicLayoutEffect(() => {
    const resizeObserver = new ResizeObserver((entries) => {
      const { offsetHeight } = entries[0].target as HTMLElement;
      setBannersHeight(offsetHeight);
    });

    const currentBannerRef = bannerRef.current;

    if (currentBannerRef) {
      resizeObserver.observe(currentBannerRef);
    }

    return () => {
      if (currentBannerRef) {
        resizeObserver.unobserve(currentBannerRef);
      }
    };
  }, [bannerRef]);

  return (
    <>
      {!props.withoutSeo && (
        <HeadSeo
          title={pageTitle ?? APP_NAME}
          description={props.subtitle ? props.subtitle?.toString() : ""}
        />
      )}
      <div>
        <Toaster position="bottom-right" />
      </div>

      {/* todo: only run this if timezone is different */}
      <div className="flex min-h-screen flex-col">
        <div ref={bannerRef} className="sticky top-0 z-10 w-full divide-y divide-black">
          <AdminPasswordBanner />
          <VerifyEmailBanner />
        </div>
        <div className="flex flex-1" data-testid="dashboard-shell">
          {props.SidebarContainer || <SideBarContainer bannersHeight={bannersHeight} />}
          <div className="flex w-0 flex-1 flex-col">
            <MainContainer {...props} />
          </div>
        </div>
      </div>
    </>
  );
};

type DrawerState = [isOpen: boolean, setDrawerOpen: Dispatch<SetStateAction<boolean>>];

type LayoutProps = {
  centered?: boolean;
  title?: string;
  heading?: ReactNode;
  subtitle?: ReactNode;
  headerClassName?: string;
  children: ReactNode;
  CTA?: ReactNode;
  large?: boolean;
  MobileNavigationContainer?: ReactNode;
  SidebarContainer?: ReactNode;
  TopNavContainer?: ReactNode;
  drawerState?: DrawerState;
  HeadingLeftIcon?: ReactNode;
  backPath?: string | boolean; // renders back button to specified path
  // use when content needs to expand with flex
  flexChildrenContainer?: boolean;
  isPublic?: boolean;
  withoutMain?: boolean;
  // Gives you the option to skip HeadSEO and render your own.
  withoutSeo?: boolean;
  // Gives the ability to include actions to the right of the heading
  actions?: JSX.Element;
  beforeCTAactions?: JSX.Element;
  afterHeading?: ReactNode;
  smallHeading?: boolean;
  hideHeadingOnMobile?: boolean;
};

const useBrandColors = () => {
  const { data: user } = useMeQuery();
  const brandTheme = getBrandColours({
    lightVal: user?.brandColor,
    darkVal: user?.darkBrandColor,
  });
  useAppDefaultTheme(brandTheme);
};

const KBarWrapper = ({ children, withKBar = false }: { withKBar: boolean; children: React.ReactNode }) =>
  withKBar ? (
    <KBarRoot>
      {children}
      <KBarContent />
    </KBarRoot>
  ) : (
    <>{children}</>
  );

const PublicShell = (props: LayoutProps) => {
  const { status } = useSession();
  return (
    <KBarWrapper withKBar={status === "authenticated"}>
      <Layout {...props} />
    </KBarWrapper>
  );
};

export default function Shell(props: LayoutProps) {
  // if a page is unauthed and isPublic is true, the redirect does not happen.
  useRedirectToLoginIfUnauthenticated(props.isPublic);
  useRedirectToOnboardingIfNeeded();
  // System Theme is automatically supported using ThemeProvider. If we intend to use user theme throughout the app we need to uncomment this.
  // useTheme(profile.theme);
  useBrandColors();

  return !props.isPublic ? (
    <KBarWrapper withKBar>
      <Layout {...props} />
    </KBarWrapper>
  ) : (
    <PublicShell {...props} />
  );
}

interface UserDropdownProps {
  small?: boolean;
}

function UserDropdown({ small }: UserDropdownProps) {
  const { t } = useLocale();
  const { data: user } = useMeQuery();
  const { data: avatar } = useAvatarQuery();
  useEffect(() => {
    // eslint-disable-next-line @typescript-eslint/ban-ts-comment
    //@ts-ignore
    const Beacon = window.Beacon;
    // window.Beacon is defined when user actually opens up HelpScout and username is available here. On every re-render update session info, so that it is always latest.
    Beacon &&
      Beacon("session-data", {
        username: user?.username || "Unknown",
        screenResolution: `${screen.width}x${screen.height}`,
      });
  });

  const utils = trpc.useContext();
  const [helpOpen, setHelpOpen] = useState(false);
  const [menuOpen, setMenuOpen] = useState(false);
  if (!user) {
    return null;
  }
  const onHelpItemSelect = () => {
    setHelpOpen(false);
    setMenuOpen(false);
  };

  // Prevent rendering dropdown if user isn't available.
  // We don't want to show nameless user.
  if (!user) {
    return null;
  }
  return (
    <Dropdown open={menuOpen}>
      <DropdownMenuTrigger asChild onClick={() => setMenuOpen((menuOpen) => !menuOpen)}>
        <button
          className={classNames(
            "hover:bg-emphasis group mx-0 flex cursor-pointer appearance-none items-center rounded-full text-left outline-none focus:outline-none focus:ring-0 md:rounded-none lg:rounded",
            small ? "p-2" : "px-2 py-1.5"
          )}>
          <span
            className={classNames(
              small ? "h-4 w-4" : "h-5 w-5 ltr:mr-2 rtl:ml-2",
              "relative flex-shrink-0 rounded-full bg-gray-300"
            )}>
            <Avatar
              size={small ? "xs" : "xsm"}
              imageSrc={avatar?.avatar || WEBAPP_URL + "/" + user.username + "/avatar.png"}
              alt={user.username || "Nameless User"}
              className="overflow-hidden"
            />
            <span
              className={classNames(
                "border-muted absolute -bottom-1 -right-1 rounded-full border bg-green-500",
                "bg-green-500",
                small ? "-bottom-0.5 -right-0.5 h-2.5 w-2.5" : "-bottom-0.5 -right-0 h-2 w-2"
              )}
            />
          </span>
          {!small && (
            <span className="flex flex-grow items-center gap-2">
              <span className="line-clamp-1 flex-grow text-sm leading-none">
                <span className="text-emphasis block font-medium">{user.name || "Nameless User"}</span>
              </span>
              <ChevronDown
                className="group-hover:text-subtle text-muted h-4 w-4 flex-shrink-0 rtl:mr-4"
                aria-hidden="true"
              />
            </span>
          )}
        </button>
      </DropdownMenuTrigger>

      <DropdownMenuPortal>
        {/* <FreshChatProvider> */}
        <DropdownMenuContent
          align="start"
          onInteractOutside={() => {
            setMenuOpen(false);
            setHelpOpen(false);
          }}
          className="group overflow-hidden rounded-md">
          (
          <>
            <DropdownMenuItem>
              <DropdownItem
                type="button"
                StartIcon={(props) => (
                  <UserIcon className={classNames("text-default", props.className)} aria-hidden="true" />
                )}
                href="/settings/my-account/profile">
                {t("my_profile")}
              </DropdownItem>
            </DropdownMenuItem>
            <DropdownMenuItem>
              <DropdownItem
                type="button"
                StartIcon={(props) => (
                  <Settings className={classNames("text-default", props.className)} aria-hidden="true" />
                )}
                href="/settings/my-account/general">
                {t("my_settings")}
              </DropdownItem>
            </DropdownMenuItem>
            <DropdownMenuItem>
              <DropdownItem
                type="button"
                StartIcon={(props) => (
                  <Moon className={classNames("text-default", props.className)} aria-hidden="true" />
                )}
                onClick={() => {
                  utils.viewer.me.invalidate();
                }}>
                {t("set_as_away")}
              </DropdownItem>
            </DropdownMenuItem>
            <DropdownMenuSeparator />

            <DropdownMenuItem>
              <DropdownItem
                type="button"
                StartIcon={(props) => <HelpCircle aria-hidden="true" {...props} />}
                onClick={() => setHelpOpen(true)}>
                {t("help")}
              </DropdownItem>
            </DropdownMenuItem>

            <DropdownMenuSeparator />

            <DropdownMenuItem>
              <DropdownItem
                type="button"
                StartIcon={(props) => <LogOut aria-hidden="true" {...props} />}
                onClick={() => signOut({ callbackUrl: "/auth/logout" })}>
                {t("sign_out")}
              </DropdownItem>
            </DropdownMenuItem>
          </>
          )
        </DropdownMenuContent>
        {/* </FreshChatProvider> */}
      </DropdownMenuPortal>
    </Dropdown>
  );
}

export type NavigationItemType = {
  name: string;
  href: string;
  onClick?: React.MouseEventHandler<HTMLAnchorElement | HTMLButtonElement>;
  target?: HTMLAnchorElement["target"];
  badge?: React.ReactNode;
  icon?: SVGComponent;
  child?: NavigationItemType[];
  pro?: true;
  onlyMobile?: boolean;
  onlyDesktop?: boolean;
  isCurrent?: ({
    item,
    isChild,
    router,
  }: {
    item: Pick<NavigationItemType, "href">;
    isChild?: boolean;
    router: NextRouter;
  }) => boolean;
};

const requiredCredentialNavigationItems = ["Routing Forms"];
const MORE_SEPARATOR_NAME = "more";

const navigation: NavigationItemType[] = [
  {
    name: "event_types_page_title",
    href: "/users",
    icon: LinkIcon,
  },
  {
    name: "bookings",
    href: "/bookings/upcoming",
    icon: Calendar,
    isCurrent: ({ router }) => {
      const path = router.asPath.split("?")[0];
      return path.startsWith("/bookings");
    },
  },
  {
    name: "availability",
    href: "/availability",
    icon: Clock,
  },
  {
    name: "teams",
    href: "/teams",
    icon: Users,
    onlyDesktop: true,
    badge: <TeamInviteBadge />,
  },
  {
    name: "apps",
    href: "/apps",
    icon: Grid,
    isCurrent: ({ router, item }) => {
      const path = router.asPath.split("?")[0];
      // During Server rendering path is /v2/apps but on client it becomes /apps(weird..)
      return (
        (path.startsWith(item.href) || path.startsWith("/v2" + item.href)) && !path.includes("routing-forms/")
      );
    },
    child: [
      {
        name: "app_store",
        href: "/apps",
        isCurrent: ({ router, item }) => {
          const path = router.asPath.split("?")[0];
          // During Server rendering path is /v2/apps but on client it becomes /apps(weird..)
          return (
            (path.startsWith(item.href) || path.startsWith("/v2" + item.href)) &&
            !path.includes("routing-forms/") &&
            !path.includes("/installed")
          );
        },
      },
      {
        name: "installed_apps",
        href: "/apps/installed/calendar",
        isCurrent: ({ router }) => {
          const path = router.asPath;
          return path.startsWith("/apps/installed/") || path.startsWith("/v2/apps/installed/");
        },
      },
    ],
  },
  {
    name: MORE_SEPARATOR_NAME,
    href: "/more",
    icon: MoreHorizontal,
  },
  {
    name: "Routing Forms",
    href: "/apps/routing-forms/forms",
    icon: FileText,
    isCurrent: ({ router }) => {
      return router.asPath.startsWith("/apps/routing-forms/");
    },
  },
  {
    name: "workflows",
    href: "/workflows",
    icon: Zap,
  },
  {
    name: "insights",
    href: "/insights",
    icon: BarChart,
  },
];

const moreSeparatorIndex = navigation.findIndex((item) => item.name === MORE_SEPARATOR_NAME);
// We create all needed navigation items for the different use cases
const { desktopNavigationItems, mobileNavigationBottomItems, mobileNavigationMoreItems } = navigation.reduce<
  Record<string, NavigationItemType[]>
>(
  (items, item, index) => {
    // We filter out the "more" separator in` desktop navigation
    if (item.name !== MORE_SEPARATOR_NAME) items.desktopNavigationItems.push(item);
    // Items for mobile bottom navigation
    if (index < moreSeparatorIndex + 1 && !item.onlyDesktop) {
      items.mobileNavigationBottomItems.push(item);
    } // Items for the "more" menu in mobile navigation
    else {
      items.mobileNavigationMoreItems.push(item);
    }
    return items;
  },
  { desktopNavigationItems: [], mobileNavigationBottomItems: [], mobileNavigationMoreItems: [] }
);

const Navigation = () => {
  return (
    <nav className="mt-2 flex-1 md:px-2 lg:mt-4 lg:px-0">
      {desktopNavigationItems.map((item) => (
        <NavigationItem key={item.name} item={item} />
      ))}
      <div className="text-subtle mt-0.5 lg:hidden">
        <KBarTrigger />
      </div>
    </nav>
  );
};

function useShouldDisplayNavigationItem(item: NavigationItemType) {
  const { status } = useSession();
  // const { data: routingForms } = trpc.viewer.appById.useQuery(
  //   { appId: "routing-forms" },
  //   {
  //     enabled: status === "authenticated" && requiredCredentialNavigationItems.includes(item.name),
  //     trpc: {},
  //   }
  // );
  // const flags = useFlagMap();
  // if (isKeyInObject(item.name, flags)) return flags[item.name];
  // return !requiredCredentialNavigationItems.includes(item.name) || routingForms?.isInstalled;
  return !requiredCredentialNavigationItems.includes(item.name);
}

const defaultIsCurrent: NavigationItemType["isCurrent"] = ({ isChild, item, router }) => {
  return isChild ? item.href === router.asPath : item.href ? router.asPath.startsWith(item.href) : false;
};

const NavigationItem: React.FC<{
  index?: number;
  item: NavigationItemType;
  isChild?: boolean;
}> = (props) => {
  const { item, isChild } = props;
  const { t, isLocaleReady } = useLocale();
  const router = useRouter();
  const isCurrent: NavigationItemType["isCurrent"] = item.isCurrent || defaultIsCurrent;
  const current = isCurrent({ isChild: !!isChild, item, router });
  const shouldDisplayNavigationItem = useShouldDisplayNavigationItem(props.item);

  if (!shouldDisplayNavigationItem) return null;

  return (
    <Fragment>
      {item.child &&
        isCurrent({ router, isChild, item }) &&
        item.child.map((item, index) => <NavigationItem index={index} key={item.name} item={item} isChild />)}
    </Fragment>
  );
};

function MobileNavigationContainer() {
  const { status } = useSession();
  if (status !== "authenticated") return null;
  return <MobileNavigation />;
}

const MobileNavigation = () => {
  const isEmbed = false;

  return (
    <>
      <nav
        className={classNames(
          "pwa:pb-2.5 bg-muted border-subtle fixed bottom-0 z-30 -mx-4 flex w-full border-t bg-opacity-40 px-1 shadow backdrop-blur-md md:hidden",
          isEmbed && "hidden"
        )}>
        {mobileNavigationBottomItems.map((item) => (
          <MobileNavigationItem key={item.name} item={item} />
        ))}
      </nav>
      {/* add padding to content for mobile navigation*/}
      <div className="block pt-12 md:hidden" />
    </>
  );
};

const MobileNavigationItem: React.FC<{
  item: NavigationItemType;
  isChild?: boolean;
}> = (props) => {
  const { item, isChild } = props;
  const router = useRouter();
  const { t, isLocaleReady } = useLocale();
  const isCurrent: NavigationItemType["isCurrent"] = item.isCurrent || defaultIsCurrent;
  const current = isCurrent({ isChild: !!isChild, item, router });
  const shouldDisplayNavigationItem = useShouldDisplayNavigationItem(props.item);

  if (!shouldDisplayNavigationItem) return null;
  return (
    <Link
      key={item.name}
      href={item.href}
      className="[&[aria-current='page']]:text-emphasis hover:text-default text-muted relative my-2 min-w-0 flex-1 overflow-hidden rounded-md !bg-transparent p-1 text-center text-xs font-medium focus:z-10 sm:text-sm"
      aria-current={current ? "page" : undefined}>
      {item.badge && <div className="absolute right-1 top-1">{item.badge}</div>}
      {item.icon && (
        <item.icon
          className="[&[aria-current='page']]:text-emphasis  mx-auto mb-1 block h-5 w-5 flex-shrink-0 text-center text-inherit"
          aria-hidden="true"
          aria-current={current ? "page" : undefined}
        />
      )}
      {isLocaleReady ? <span className="block truncate">{t(item.name)}</span> : <SkeletonText />}
    </Link>
  );
};

const MobileNavigationMoreItem: React.FC<{
  item: NavigationItemType;
  isChild?: boolean;
}> = (props) => {
  const { item } = props;
  const { t, isLocaleReady } = useLocale();
  const shouldDisplayNavigationItem = useShouldDisplayNavigationItem(props.item);

  if (!shouldDisplayNavigationItem) return null;

  return (
    <li className="border-subtle border-b last:border-b-0" key={item.name}>
      <Link href={item.href} className="hover:bg-subtle flex items-center justify-between p-5">
        <span className="text-default flex items-center font-semibold ">
          {item.icon && <item.icon className="h-5 w-5 flex-shrink-0 ltr:mr-3 rtl:ml-3" aria-hidden="true" />}
          {isLocaleReady ? t(item.name) : <SkeletonText />}
        </span>
        <ArrowRight className="text-subtle h-5 w-5" />
      </Link>
    </li>
  );
};

type SideBarContainerProps = {
  bannersHeight: number;
};

type SideBarProps = {
  bannersHeight: number;
  user?: UserAuth | null;
};

function SideBarContainer({ bannersHeight }: SideBarContainerProps) {
  const { status, data } = useSession();

  // Make sure that Sidebar is rendered optimistically so that a refresh of pages when logged in have SideBar from the beginning.
  // This improves the experience of refresh on app store pages(when logged in) which are SSG.
  // Though when logged out, app store pages would temporarily show SideBar until session status is confirmed.
  if (status !== "loading" && status !== "authenticated") return null;
  return <SideBar bannersHeight={bannersHeight} user={data?.user} />;
}

function SideBar({ bannersHeight, user }: SideBarProps) {
  const { t, isLocaleReady } = useLocale();
  const router = useRouter();
  const publicPageUrl = "";
  const bottomNavItems: NavigationItemType[] = [
    {
      name: "settings",
      href: user?.organizationId ? `/settings/organizations/profile` : "/settings/my-account/profile",
      icon: Settings,
    },
  ];
  return (
    <div className="relative">
      <aside
        style={{ maxHeight: `calc(100vh - ${bannersHeight}px)`, top: `${bannersHeight}px` }}
        className="desktop-transparent bg-muted border-muted fixed left-0 hidden h-full max-h-screen w-14 flex-col overflow-y-auto overflow-x-hidden border-r md:sticky md:flex lg:w-56 lg:px-3 dark:bg-gradient-to-tr dark:from-[#2a2a2a] dark:to-[#1c1c1c]">
        <div className="flex h-full flex-col justify-between py-3 lg:pt-4">
          <header className="items-center justify-between md:hidden lg:flex">
            <div data-testid="user-dropdown-trigger">
              <span className="hidden lg:inline">
                <UserDropdown />
              </span>
              <span className="hidden md:inline lg:hidden">
                <UserDropdown small />
              </span>
            </div>

            <div className="flex space-x-0.5 rtl:space-x-reverse">
              <button
                color="minimal"
                onClick={() => window.history.back()}
                className="desktop-only hover:text-emphasis text-subtle group flex text-sm font-medium">
                <ArrowLeft className="group-hover:text-emphasis text-subtle h-4 w-4 flex-shrink-0" />
              </button>
              <button
                color="minimal"
                onClick={() => window.history.forward()}
                className="desktop-only hover:text-emphasis text-subtle group flex text-sm font-medium">
                <ArrowRight className="group-hover:text-emphasis text-subtle h-4 w-4 flex-shrink-0" />
              </button>

              <KBarTrigger />
            </div>
          </header>

          <hr className="desktop-only border-subtle absolute -left-3 -right-3 mt-4 block w-full" />

          {/* logo icon for tablet */}
          <Link href="/users" className="text-center md:inline lg:hidden">
            <Logo small icon />
          </Link>

          <Navigation />
        </div>

        <div>
          {/* <Tips /> */}
          {bottomNavItems.map(({ icon: Icon, ...item }, index) => (
            <Tooltip side="right" content={t(item.name)} className="lg:hidden" key={item.name}>
              <ButtonOrLink
                href={item.href || undefined}
                aria-label={t(item.name)}
                target={item.target}
                className={classNames(
                  "text-left",
                  "[&[aria-current='page']]:bg-emphasis  text-default justify-right group flex items-center rounded-md px-2 py-1.5 text-sm font-medium",
                  "[&[aria-current='page']]:text-emphasis mt-0.5 w-full text-sm",
                  isLocaleReady ? "hover:bg-emphasis hover:text-emphasis" : "",
                  index === 0 && "mt-3"
                )}
                aria-current={
                  defaultIsCurrent && defaultIsCurrent({ item: { href: item.href }, router })
                    ? "page"
                    : undefined
                }
                onClick={item.onClick}>
                {!!Icon && (
                  <Icon
                    className={classNames(
                      "h-4 w-4 flex-shrink-0 [&[aria-current='page']]:text-inherit",
                      "md:ltr:mr-2 md:rtl:ml-2"
                    )}
                    aria-hidden="true"
                    aria-current={
                      defaultIsCurrent && defaultIsCurrent({ item: { href: item.href }, router })
                        ? "page"
                        : undefined
                    }
                  />
                )}
                {isLocaleReady ? (
                  <span className="hidden w-full justify-between lg:flex">
                    <div className="flex">{t(item.name)}</div>
                  </span>
                ) : (
                  <SkeletonText style={{ width: `${item.name.length * 10}px` }} className="h-[20px]" />
                )}
              </ButtonOrLink>
            </Tooltip>
          ))}
        </div>
      </aside>
    </div>
  );
}

export function ShellMain(props: LayoutProps) {
  const router = useRouter();
  const { isLocaleReady } = useLocale();

  return (
    <>
      <div
        className={classNames(
          "flex items-center md:mb-6 md:mt-0",
          props.smallHeading ? "lg:mb-7" : "lg:mb-8",
          props.hideHeadingOnMobile ? "mb-0" : "mb-6"
        )}>
        {!!props.backPath && (
          <Button
            variant="icon"
            size="sm"
            color="minimal"
            onClick={() =>
              typeof props.backPath === "string" ? router.push(props.backPath as string) : router.back()
            }
            StartIcon={ArrowLeft}
            aria-label="Go Back"
            className="rounded-md ltr:mr-2 rtl:ml-2"
          />
        )}
        {props.heading && (
          <header
            className={classNames(props.large && "py-8", "flex w-full max-w-full items-center truncate")}>
            {props.HeadingLeftIcon && <div className="ltr:mr-4">{props.HeadingLeftIcon}</div>}
            <div className={classNames("w-full truncate md:block ltr:mr-4 rtl:ml-4", props.headerClassName)}>
              {props.heading && (
                <h3
                  className={classNames(
                    "font-myapp text-emphasis inline max-w-28 truncate text-lg font-semibold tracking-wide sm:max-w-72 sm:text-xl md:block md:max-w-80 xl:max-w-full",
                    props.smallHeading ? "text-base" : "text-xl",
                    props.hideHeadingOnMobile && "hidden"
                  )}>
                  {!isLocaleReady ? <SkeletonText invisible /> : props.heading}
                </h3>
              )}
              {props.subtitle && (
                <p className="text-default hidden text-sm md:block">
                  {!isLocaleReady ? <SkeletonText invisible /> : props.subtitle}
                </p>
              )}
            </div>
            {props.beforeCTAactions}
            {props.CTA && (
              <div
                className={classNames(
                  props.backPath
                    ? "relative"
                    : "pwa:bottom-24 fixed bottom-20 z-40 md:z-auto ltr:right-4 md:ltr:right-0 rtl:left-4 md:rtl:left-0",
                  "flex-shrink-0 md:relative md:bottom-auto md:right-auto"
                )}>
                {props.CTA}
              </div>
            )}
            {props.actions && props.actions}
          </header>
        )}
      </div>
      {props.afterHeading && <>{props.afterHeading}</>}
      <div className={classNames(props.flexChildrenContainer && "flex flex-1 flex-col")}>
        {props.children}
      </div>
    </>
  );
}

function MainContainer({
  MobileNavigationContainer: MobileNavigationContainerProp = <MobileNavigationContainer />,
  TopNavContainer: TopNavContainerProp = <TopNavContainer />,
  ...props
}: LayoutProps) {
  return (
    <main className="bg-default relative z-0 flex-1 focus:outline-none">
      {/* show top navigation for md and smaller (tablet and phones) */}
      {TopNavContainerProp}
      <div className="max-w-full px-4 py-4 md:py-8 lg:px-12">
        <ErrorBoundary>
          {!props.withoutMain ? <ShellMain {...props}>{props.children}</ShellMain> : props.children}
        </ErrorBoundary>
        {/* show bottom navigation for md and smaller (tablet and phones) on pages where back button doesn't exist */}
        {!props.backPath ? MobileNavigationContainerProp : null}
      </div>
    </main>
  );
}

function TopNavContainer() {
  const { status } = useSession();
  if (status !== "authenticated") return null;
  return <TopNav />;
}

function TopNav() {
  const isEmbed = false;
  const { t } = useLocale();
  return (
    <>
      <nav
        style={isEmbed ? { display: "none" } : {}}
        className="bg-muted border-subtle sticky top-0 z-40 flex w-full items-center justify-between border-b bg-opacity-50 px-4 py-1.5 backdrop-blur-lg sm:p-4 md:hidden">
        <Link href="/users">
          <Logo />
        </Link>
        <div className="flex items-center gap-2 self-center">
          <span className="hover:bg-muted hover:text-emphasis text-default group flex items-center rounded-full text-sm font-medium lg:hidden">
            <KBarTrigger />
          </span>
          <button className="hover:bg-muted hover:text-subtle text-muted rounded-full p-1 focus:outline-none focus:ring-2 focus:ring-black focus:ring-offset-2">
            <span className="sr-only">{t("settings")}</span>
            <Link href="/settings/my-account/profile">
              <Settings className="text-default h-4 w-4" aria-hidden="true" />
            </Link>
          </button>
          <UserDropdown small />
        </div>
      </nav>
    </>
  );
}

export const MobileNavigationMoreItems = () => (
  <ul className="border-subtle mt-2 rounded-md border">
    {mobileNavigationMoreItems.map((item) => (
      <MobileNavigationMoreItem key={item.name} item={item} />
    ))}
  </ul>
);
