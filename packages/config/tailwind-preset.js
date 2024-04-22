const plugin = require("tailwindcss/plugin");
const { fontFamily } = require("tailwindcss/defaultTheme");

/** @type {import('tailwindcss').Config} */
const subtleColor = "#E5E7EB";
module.exports = {
  content: [
    "./pages/**/*.{js,ts,jsx,tsx}",
    "./components/**/*.{js,ts,jsx,tsx}",
    "../../packages/app-store/**/*{components,pages}/**/*.{js,ts,jsx,tsx}",
    "../../packages/features/**/*.{js,ts,jsx,tsx}",
    "../../packages/ui/**/*.{js,ts,jsx,tsx}",
    "../../packages/atoms/**/*.{js,ts,jsx,tsx}",
  ],
  darkMode: "class",
  theme: {
    extend: {
      colors: {
        emphasis: "var(--shipmvp-bg-emphasis)",
        default: "var(--shipmvp-bg, white)",
        subtle: "var(--shipmvp-bg-subtle)",
        muted: "var(--shipmvp-bg-muted)",
        inverted: "var(--shipmvp-bg-inverted)",
        info: "var(--shipmvp-bg-info)",
        success: "var(--shipmvp-bg-success)",
        attention: "var(--shipmvp-bg-attention)",
        error: "var(--shipmvp-bg-error)",
        darkerror: "var(--shipmvp-bg-dark-error)",
        black: "#111111",
        brand: {
          default: "var(--shipmvp-brand,#111827)",
          emphasis: "var(--shipmvp-brand-emphasis,#101010)",
          subtle: "var(--shipmvp-brand-subtle,#9CA3AF)",
          accent: "var(--shipmvp-brand-accent,white)",
        },
        gray: {
          50: "#F9FAFB",
          100: "#F3F4F6",
          200: "#E5E7EB",
          300: "#D1D5DB",
          400: "#9CA3AF",
          500: "#6B7280",
          600: "#4B5563",
          700: "#374151",
          800: "#1F2937",
          900: "#111827",
        },
        darkgray: {
          50: "#101010",
          100: "#1c1c1c",
          200: "#2b2b2b",
          300: "#444444",
          400: "#575757",
          500: "#767676",
          600: "#a5a5a5",
          700: "#d6d6d6",
          800: "#e8e8e8",
          900: "#f3f4f6",
        },
      },
      borderColor: {
        emphasis: "var(--shipmvp-border-emphasis, #9CA3AF)",
        default: "var(--shipmvp-border, #D1D5DB)",
        subtle: `var(--shipmvp-border-subtle, ${subtleColor})`,
        muted: "var(--shipmvp-border-muted, #F3F4F6)",
        booker: `var(--shipmvp-border-booker, ${subtleColor})`,
        error: "var(--shipmvp-border-error, #AA2E26)",
      },
      textColor: {
        emphasis: "var(--shipmvp-text-emphasis, #111827)",
        default: "var(--shipmvp-text, #374151)",
        subtle: "var(--shipmvp-text-subtle, #6B7280)",
        muted: "var(--shipmvp-text-muted, #9CA3AF)",
        inverted: "var(--shipmvp-text-inverted, white)",
        info: "var(--shipmvp-text-info, #253985)",
        success: "var(--shipmvp-text-success, #285231)",
        attention: "var(--shipmvp-text-attention, #73321B)",
        error: "var(--shipmvp-text-error, #752522)",
        brand: "var(--shipmvp-brand-text,'white')",
      },
      fill: {
        emphasis: "var(--shipmvp-text-emphasis, #111827)",
        default: "var(--shipmvp-text, #374151)",
        subtle: "var(--shipmvp-text-subtle, #6B7280)",
        muted: "var(--shipmvp-text-muted, #9CA3AF)",
        inverted: "var(--shipmvp-text-inverted, white)",
        info: "var(--shipmvp-text-info, #253985)",
        success: "var(--shipmvp-text-success, #285231)",
        attention: "var(--shipmvp-text-attention, #73321B)",
        error: "var(--shipmvp-text-error, #752522)",
        brand: "var(--shipmvp-brand-text)",
      },
      screens: {
        pwa: { raw: "(display-mode: standalone)" },
      },
      keyframes: {
        "fade-in-up": {
          from: { opacity: 0, transform: "translateY(10px)" },
          to: { opacity: 1, transform: "none" },
        },
        spinning: {
          "100%": { transform: "rotate(360deg)" },
        },
      },
      animation: {
        "fade-in-up": "fade-in-up 600ms var(--animation-delay, 0ms) cubic-bezier(.21,1.02,.73,1) forwards",
        spinning: "spinning 0.75s linear infinite",
      },
      boxShadow: {
        dropdown: "0px 2px 6px -1px rgba(0, 0, 0, 0.08)",
      },
      borderWidth: {
        "booker-width": "var(--shipmvp-border-booker-width, 1px)",
      },
      fontFamily: {
        myapp: ["var(--font-myapp)", ...fontFamily.serif],
        sans: ["var(--font-inter)", ...fontFamily.sans],
        mono: ["Roboto Mono", "monospace"],
      },
      maxHeight: (theme) => ({
        0: "0",
        97: "25rem",
        ...theme("spacing"),
        full: "100%",
        screen: "100vh",
      }),
      minHeight: (theme) => ({
        0: "0",
        ...theme("spacing"),
        full: "100%",
        screen: "100vh",
      }),
      minWidth: (theme) => ({
        0: "0",
        ...theme("spacing"),
        full: "100%",
        screen: "100vw",
      }),
      maxWidth: (theme, { breakpoints }) => ({
        0: "0",
        ...theme("spacing"),
        ...breakpoints(theme("screens")),
        full: "100%",
        screen: "100vw",
      }),
      backgroundImage: {
        "gradient-primary": "radial-gradient(162.05% 170% at 109.58% 35%, #667593 0%, #E3E3E3 100%)",
      },
    },
  },
  plugins: [
    require("@tailwindcss/forms"),
    require("@tailwindcss/typography"),
    require("tailwind-scrollbar"),
    require("tailwindcss-radix")(),
    require("@savvywombat/tailwindcss-grid-areas"),
    plugin(({ addVariant }) => {
      addVariant("mac", ".mac &");
      addVariant("windows", ".windows &");
      addVariant("ios", ".ios &");
    }),
    plugin(({ addBase, theme }) => {
      addBase({
        hr: {
          borderColor: theme("subtle"),
        },
      });
    }),
  ],
  variants: {
    scrollbar: ["rounded", "dark"],
  },
};
