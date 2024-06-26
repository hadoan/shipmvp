import Link from "next/link";
import { useRouter } from "next/router";
import type { ComponentProps } from "react";
import { Fragment } from "react";

import classNames from "@shipmvp/lib/classNames";
import { useLocale } from "@shipmvp/lib/hooks/useLocale";
import type { SVGComponent } from "@shipmvp/types/SVGComponent";

import { ChevronRight, ExternalLink } from "../../icon";
import { Skeleton } from "../../skeleton";

export type VerticalTabItemProps = {
  name: string;
  info?: string;
  icon?: SVGComponent;
  disabled?: boolean;
  children?: VerticalTabItemProps[];
  textClassNames?: string;
  className?: string;
  isChild?: boolean;
  hidden?: boolean;
  disableChevron?: boolean;
  href: string;
  isExternalLink?: boolean;
  linkProps?: Omit<ComponentProps<typeof Link>, "href">;
  avatar?: string;
  iconClassName?: string;
};

const VerticalTabItem = ({
  name,
  href,
  info,
  isChild,
  disableChevron,
  linkProps,
  ...props
}: VerticalTabItemProps) => {
  const { t } = useLocale();
  const { asPath } = useRouter();
  const isCurrent = asPath.startsWith(href);
  return (
    <Fragment key={name}>
      {!props.hidden && (
        <>
          <Link
            key={name}
            href={href}
            {...linkProps}
            target={props.isExternalLink ? "_blank" : "_self"}
            className={classNames(
              props.textClassNames || "text-default text-sm font-medium leading-none",
              "hover:bg-subtle [&[aria-current='page']]:bg-emphasis [&[aria-current='page']]:text-emphasis group-hover:text-default group flex min-h-8 w-64 flex-row items-center rounded-md px-3 py-[10px]",
              props.disabled && "pointer-events-none !opacity-30",
              (isChild || !props.icon) && "ml-7 w-auto ltr:mr-5 rtl:ml-5",
              !info ? "h-6" : "h-14",
              props.className
            )}
            data-testid={`vertical-tab-${name}`}
            aria-current={isCurrent ? "page" : undefined}>
            {props.icon && (
              <props.icon
                className={classNames(
                  "h-[16px] w-[16px] stroke-[2px] md:mt-0 ltr:mr-2 rtl:ml-2",
                  props.iconClassName
                )}
              />
            )}
            <div className="h-fit">
              <span className="flex items-center space-x-2 rtl:space-x-reverse">
                <Skeleton title={t(name)} as="p" className="mt-px min-h-4 max-w-36 truncate">
                  {t(name)}
                </Skeleton>
                {props.isExternalLink ? <ExternalLink /> : null}
              </span>
              {info && (
                <Skeleton as="p" title={t(info)} className="mt-1 max-w-44 truncate text-xs font-normal">
                  {t(info)}
                </Skeleton>
              )}
            </div>
            {!disableChevron && isCurrent && (
              <div className="ml-auto self-center">
                <ChevronRight
                  width={20}
                  height={20}
                  className="text-default h-auto w-[20px] stroke-[1.5px]"
                />
              </div>
            )}
          </Link>
          {props.children?.map((child) => (
            <VerticalTabItem key={child.name} {...child} isChild />
          ))}
        </>
      )}
    </Fragment>
  );
};

export default VerticalTabItem;
