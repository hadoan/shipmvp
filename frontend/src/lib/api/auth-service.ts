import { $fetch } from 'ofetch';
import type { components } from './schema';
import { getApiConfig, getAuthConfig } from '../config';

// API Types
export type LoginDto =
  components['schemas']['ShipMvp.Application.Identity.LoginDto'];
export type AuthResultDto =
  components['schemas']['ShipMvp.Application.Identity.AuthResultDto'];
export type UserDto =
  components['schemas']['ShipMvp.Application.Identity.UserDto'];

// Configuration
const apiConfig = getApiConfig();
const authConfig = getAuthConfig();

// Auth Service
export class AuthService {
  private static get tokenKey() {
    return authConfig.tokenKey;
  }
  private static get userKey() {
    return authConfig.userKey;
  }

  static async login(credentials: LoginDto): Promise<AuthResultDto> {
    try {
      const response = await $fetch<AuthResultDto>('/api/auth/login', {
        baseURL: apiConfig.baseUrl,
        method: 'POST',
        body: credentials,
      });

      if (response.success && response.token && response.user) {
        // Store token and user in localStorage
        localStorage.setItem(this.tokenKey, response.token);
        localStorage.setItem(this.userKey, JSON.stringify(response.user));
      }

      return response;
    } catch (error) {
      console.error('Login error:', error);
      throw new Error('Login failed');
    }
  }

  static async logout(): Promise<void> {
    try {
      await $fetch('/api/auth/logout', {
        baseURL: apiConfig.baseUrl,
        method: 'POST',
        headers: {
          Authorization: `Bearer ${this.getToken()}`,
        },
      });
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      // Always clear local storage
      this.clearAuth();
    }
  }

  static async refreshToken(): Promise<AuthResultDto | null> {
    const token = this.getToken();
    if (!token) return null;

    try {
      const response = await $fetch<AuthResultDto>('/api/auth/refresh', {
        baseURL: apiConfig.baseUrl,
        method: 'POST',
        body: { token },
      });

      if (response.success && response.token && response.user) {
        localStorage.setItem(this.tokenKey, response.token);
        localStorage.setItem(this.userKey, JSON.stringify(response.user));
        return response;
      }

      // If refresh fails, clear auth
      this.clearAuth();
      return null;
    } catch (error) {
      console.error('Token refresh error:', error);
      this.clearAuth();
      return null;
    }
  }

  static getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  static getUser(): UserDto | null {
    const userStr = localStorage.getItem(this.userKey);
    if (!userStr) return null;

    try {
      return JSON.parse(userStr) as UserDto;
    } catch {
      return null;
    }
  }

  static isAuthenticated(): boolean {
    return !!this.getToken() && !!this.getUser();
  }

  static clearAuth(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
  }

  // Create an authenticated API call
  static async authenticatedFetch<T>(
    url: string,
    options: Record<string, unknown> = {}
  ): Promise<T> {
    const token = this.getToken();
    const headers = (options.headers as Record<string, string>) || {};

    try {
      return await $fetch<T>(url, {
        baseURL: apiConfig.baseUrl,
        ...options,
        headers: {
          ...headers,
          ...(token && { Authorization: `Bearer ${token}` }),
        },
      });
    } catch (error: unknown) {
      // If we get a 401, try to refresh the token
      if (
        error &&
        typeof error === 'object' &&
        'status' in error &&
        error.status === 401
      ) {
        const refreshResult = await this.refreshToken();
        if (!refreshResult) {
          // Redirect to login if refresh fails
          window.location.href = authConfig.loginPath;
          throw error;
        }

        // Retry the request with the new token
        const newToken = this.getToken();
        return await $fetch<T>(url, {
          baseURL: apiConfig.baseUrl,
          ...options,
          headers: {
            ...headers,
            ...(newToken && { Authorization: `Bearer ${newToken}` }),
          },
        });
      }

      throw error;
    }
  }
}
