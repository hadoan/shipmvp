import { merge } from "lodash";
import type { NextSeoProps } from "next-seo";
import { NextSeo } from "next-seo";
import { useRouter } from "next/router";

import type { AppImageProps, MeetingImageProps } from "@shipmvp/lib/OgImages";
import { constructAppImage, constructGenericImage, constructMeetingImage } from "@shipmvp/lib/OgImages";
import { getBrowserInfo } from "@shipmvp/lib/browser/browser.utils";
import { APP_NAME, WEBSITE_URL } from "@shipmvp/lib/constants";
import { seoConfig, getSeoImage, buildCanonical } from "@shipmvp/lib/next-seo.config";
import { truncateOnWord } from "@shipmvp/lib/text";

export type HeadSeoProps = {
  title: string;
  description: string;
  siteName?: string;
  url?: string;
  canonical?: string;
  nextSeoProps?: NextSeoProps;
  app?: AppImageProps;
  meeting?: MeetingImageProps;
  isBrandingHidden?: boolean;
};

/**
 * Build full seo tags from title, desc, canonical and url
 */
const buildSeoMeta = (pageProps: {
  title: string;
  description: string;
  image: string;
  siteName?: string;
  url?: string;
  canonical?: string;
}): NextSeoProps => {
  const { title, description, image, canonical, siteName = seoConfig.headSeo.siteName } = pageProps;
  return {
    title: title,
    canonical: canonical,
    openGraph: {
      site_name: siteName,
      type: "website",
      title: title,
      description: description,
      images: [
        {
          url: image,
        },
      ],
    },
    additionalMetaTags: [
      {
        property: "name",
        content: title,
      },
      {
        property: "description",
        content: description,
      },
      {
        name: "description",
        content: description,
      },
      {
        property: "image",
        content: image,
      },
    ],
  };
};

export const HeadSeo = (props: HeadSeoProps): JSX.Element => {
  // The below code sets the defaultUrl for our canonical tags

  // Get the current URL from the window object
  const { url } = getBrowserInfo();
  // Check if the URL is from shipmvp.co
  const isCalcom =
    url && (new URL(url).hostname.endsWith("shipmvp.co") || new URL(url).hostname.endsWith("shipmvp.dev"));
  // Get the router's path
  const path = useRouter().asPath;
  const selfHostedOrigin = WEBSITE_URL || "https://shipmvp.co";
  // Set the default URL to either the current URL (if self-hosted) or https://shipmvp.co canonical URL
  const defaultUrl = isCalcom
    ? buildCanonical({ path, origin: "https://shipmvp.co" })
    : buildCanonical({ path, origin: selfHostedOrigin });

  const {
    title,
    description,
    siteName,
    canonical = defaultUrl,
    nextSeoProps = {},
    app,
    meeting,
    isBrandingHidden,
  } = props;

  const image = getSeoImage("ogImage") + constructGenericImage({ title, description });
  const truncatedDescription = truncateOnWord(description, 158);
  const pageTitle = `${title}${isBrandingHidden ? "" : ` | ${APP_NAME}`}`;
  let seoObject = buildSeoMeta({
    title: pageTitle,
    image,
    description: truncatedDescription,
    canonical,
    siteName,
  });

  if (meeting) {
    const pageImage = getSeoImage("ogImage") + constructMeetingImage(meeting);
    seoObject = buildSeoMeta({
      title: pageTitle,
      description: truncatedDescription,
      image: pageImage,
      canonical,
      siteName,
    });
  }

  if (app) {
    const pageImage =
      getSeoImage("ogImage") + constructAppImage({ ...app, description: truncatedDescription });
    seoObject = buildSeoMeta({
      title: pageTitle,
      description: truncatedDescription,
      image: pageImage,
      canonical,
      siteName,
    });
  }

  const seoProps: NextSeoProps = merge(nextSeoProps, seoObject);

  return <NextSeo {...seoProps} />;
};

export default HeadSeo;
