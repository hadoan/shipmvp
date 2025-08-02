// Color theme configuration for ShipMvp
export const themeConfig = {
  light: {
    // Brand colors
    brand: {
      primary: 'hsl(217.2 91.2% 59.8%)', // Blue-600
      secondary: 'hsl(262.1 83.3% 57.8%)', // Purple-600
    },

    // Sidebar colors
    sidebar: {
      background: 'hsl(0 0% 100%)', // White
      foreground: 'hsl(240 5.3% 26.1%)', // Gray-700
      border: 'hsl(220 13% 91%)', // Gray-200
      accent: 'hsl(240 4.8% 95.9%)', // Gray-50 hover
      accentForeground: 'hsl(240 5.9% 10%)', // Gray-900
    },

    // Navigation button colors
    navigation: {
      active: {
        background: 'hsl(217.2 91.2% 59.8%)', // Blue-600
        foreground: 'hsl(0 0% 100%)', // White
        hover: 'hsl(217.2 91.2% 54%)', // Blue-700
      },
      inactive: {
        background: 'transparent',
        foreground: 'hsl(215.4 16.3% 46.9%)', // Gray-500
        hover: {
          background: 'hsl(240 4.8% 95.9%)', // Gray-50
          foreground: 'hsl(240 5.9% 10%)', // Gray-900
        },
      },
    },
  },

  dark: {
    // Brand colors
    brand: {
      primary: 'hsl(217.2 91.2% 59.8%)', // Blue-500
      secondary: 'hsl(262.1 83.3% 57.8%)', // Purple-500
    },

    // Sidebar colors
    sidebar: {
      background: 'hsl(240 5.9% 10%)', // Gray-900
      foreground: 'hsl(240 4.8% 95.9%)', // Gray-50
      border: 'hsl(240 3.7% 15.9%)', // Gray-800
      accent: 'hsl(240 3.7% 15.9%)', // Gray-800 hover
      accentForeground: 'hsl(240 4.8% 95.9%)', // Gray-50
    },

    // Navigation button colors
    navigation: {
      active: {
        background: 'hsl(217.2 91.2% 59.8%)', // Blue-500
        foreground: 'hsl(0 0% 100%)', // White
        hover: 'hsl(217.2 91.2% 64%)', // Blue-400
      },
      inactive: {
        background: 'transparent',
        foreground: 'hsl(215 20.2% 65.1%)', // Gray-400
        hover: {
          background: 'hsl(240 3.7% 15.9%)', // Gray-800
          foreground: 'hsl(240 4.8% 95.9%)', // Gray-50
        },
      },
    },
  },
} as const;

export type ThemeMode = keyof typeof themeConfig;
