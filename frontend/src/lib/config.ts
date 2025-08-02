/**
 * Application Configuration
 * Centralized configuration for ShipMvp frontend application
 */

interface Config {
  /** API Configuration */
  api: {
    /** Base URL for the backend API */
    baseUrl: string;
    /** Timeout for API requests in milliseconds */
    timeout: number;
    /** Default headers for API requests */
    defaultHeaders: Record<string, string>;
  };

  /** Authentication Configuration */
  auth: {
    /** localStorage key for authentication token */
    tokenKey: string;
    /** localStorage key for user information */
    userKey: string;
    /** Token refresh endpoint */
    refreshEndpoint: string;
    /** Login redirect path */
    loginPath: string;
    /** Default redirect after login */
    defaultRedirectAfterLogin: string;
  };

  /** Application Settings */
  app: {
    /** Application name */
    name: string;
    /** Application version */
    version: string;
    /** Environment (development, production, etc.) */
    environment: string;
    /** Enable debug mode */
    debug: boolean;
  };

  /** UI Configuration */
  ui: {
    /** Default page size for paginated data */
    defaultPageSize: number;
    /** Theme configuration */
    theme: {
      /** Default theme */
      defaultTheme: 'light' | 'dark' | 'system';
      /** localStorage key for theme preference */
      storageKey: string;
    };
    /** Toast configuration */
    toast: {
      /** Default duration for toast notifications */
      defaultDuration: number;
      /** Maximum number of visible toasts */
      maxVisible: number;
    };
  };

  /** Feature Flags */
  features: {
    /** Enable user management features */
    userManagement: boolean;
    /** Enable invoice management features */
    invoiceManagement: boolean;
    /** Enable dark mode toggle */
    darkMode: boolean;
    /** Enable development tools */
    devTools: boolean;
  };

  /** External Services */
  external: {
    /** Swagger/OpenAPI documentation URL */
    swaggerUrl: string;
  };
}

// Environment variable helpers
const getEnvVar = (key: string, defaultValue: string = ''): string => {
  return import.meta.env[key] || defaultValue;
};

const getEnvVarAsNumber = (key: string, defaultValue: number): number => {
  const value = import.meta.env[key];
  return value ? parseInt(value, 10) : defaultValue;
};

const getEnvVarAsBoolean = (key: string, defaultValue: boolean): boolean => {
  const value = import.meta.env[key];
  if (value === undefined) return defaultValue;
  return value.toLowerCase() === 'true';
};

// Configuration object
export const config: Config = {
  api: {
    baseUrl: getEnvVar('VITE_API_BASE_URL', 'http://localhost:5000'),
    timeout: getEnvVarAsNumber('VITE_API_TIMEOUT', 30000),
    defaultHeaders: {
      'Content-Type': 'application/json',
    },
  },

  auth: {
    tokenKey: getEnvVar('VITE_AUTH_TOKEN_KEY', 'auth_token'),
    userKey: getEnvVar('VITE_AUTH_USER_KEY', 'auth_user'),
    refreshEndpoint: '/api/auth/refresh',
    loginPath: '/login',
    defaultRedirectAfterLogin: '/',
  },

  app: {
    name: getEnvVar('VITE_APP_NAME', 'ShipMvp'),
    version: getEnvVar('VITE_APP_VERSION', '1.0.0'),
    environment: getEnvVar('VITE_ENVIRONMENT', 'development'),
    debug: getEnvVarAsBoolean('VITE_DEBUG', import.meta.env.DEV),
  },

  ui: {
    defaultPageSize: getEnvVarAsNumber('VITE_UI_DEFAULT_PAGE_SIZE', 10),
    theme: {
      defaultTheme: getEnvVar('VITE_UI_DEFAULT_THEME', 'system') as
        | 'light'
        | 'dark'
        | 'system',
      storageKey: getEnvVar('VITE_UI_THEME_STORAGE_KEY', 'ui-theme'),
    },
    toast: {
      defaultDuration: getEnvVarAsNumber('VITE_UI_TOAST_DURATION', 4000),
      maxVisible: getEnvVarAsNumber('VITE_UI_TOAST_MAX_VISIBLE', 5),
    },
  },

  features: {
    userManagement: getEnvVarAsBoolean('VITE_FEATURE_USER_MANAGEMENT', true),
    invoiceManagement: getEnvVarAsBoolean(
      'VITE_FEATURE_INVOICE_MANAGEMENT',
      true
    ),
    darkMode: getEnvVarAsBoolean('VITE_FEATURE_DARK_MODE', true),
    devTools: getEnvVarAsBoolean('VITE_FEATURE_DEV_TOOLS', import.meta.env.DEV),
  },

  external: {
    swaggerUrl: `${getEnvVar('VITE_API_BASE_URL', 'http://localhost:5000')}/swagger`,
  },
};

// Helper functions for accessing configuration
export const getApiConfig = () => config.api;
export const getAuthConfig = () => config.auth;
export const getAppConfig = () => config.app;
export const getUIConfig = () => config.ui;
export const getFeaturesConfig = () => config.features;
export const getExternalConfig = () => config.external;

// Type exports for external use
export type { Config };

// Development logging (only in development)
if (config.app.debug) {
  console.log('📋 Application Configuration:', config);
}
