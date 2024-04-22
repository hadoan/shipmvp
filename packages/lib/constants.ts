export const WEBAPP_URL = process.env.NEXT_PUBLIC_WEBAPP_URL || "http://localhost:3000";
export const WEBSITE_URL = process.env.NEXT_PUBLIC_WEBSITE_URL || "https://shipmvp.tech";
export const APP_NAME = process.env.NEXT_PUBLIC_APP_NAME || "shipmvp.tech";
export const SUPPORT_MAIL_ADDRESS = process.env.NEXT_PUBLIC_SUPPORT_MAIL_ADDRESS || "help@shipmvp.tech";
export const COMPANY_NAME = process.env.NEXT_PUBLIC_COMPANY_NAME || "shipmvp.tech, Inc.";
export const SENDER_NAME = process.env.NEXT_PUBLIC_SENDGRID_SENDER_NAME || "shipmvp.tech";

export const MY_APP_URL = new URL(WEBAPP_URL).hostname.endsWith(".vercel.app") ? WEBAPP_URL : WEBSITE_URL;

export const IS_PRODUCTION = process.env.NODE_ENV === "production";

export const LOGO = "/logo-white-word.svg";
export const LOGO_ICON = "/icon-white.svg";
export const FAVICON_16 = "/favicon-16x16.png";
export const FAVICON_32 = "/favicon-32x32.png";
export const APPLE_TOUCH_ICON = "/apple-touch-icon.png";
export const MSTILE_ICON = "/mstile-150x150.png";
export const ANDROID_CHROME_ICON_192 = "/android-chrome-192x192.png";
export const ANDROID_CHROME_ICON_256 = "/android-chrome-256x256.png";

export const SEO_IMG_DEFAULT = `${WEBSITE_URL}/og-image.png`;
export const SEO_IMG_OGIMG = `${MY_APP_URL}/_next/image?w=1200&q=100&url=${encodeURIComponent(
  "/api/social/og/image"
)}`;
export const FULL_NAME_LENGTH_MAX_LIMIT = 50;
