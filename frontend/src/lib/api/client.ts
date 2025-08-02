// API client configuration for ShipMvp backend
import createClient from 'openapi-fetch';
import type { paths } from './schema';
import { getApiConfig } from '../config';

const apiConfig = getApiConfig();

// Create type-safe API client
export const api = createClient<paths>({
  baseUrl: apiConfig.baseUrl,
  headers: apiConfig.defaultHeaders,
});

// API response types
export type ApiResponse<T> =
  | {
      data: T;
      error?: never;
    }
  | {
      data?: never;
      error: {
        message: string;
        status: number;
      };
    };

// Extract response types from the OpenAPI schema
export type InvoiceDto =
  paths['/api/Invoices']['get']['responses']['200']['content']['application/json'][0];
export type CreateInvoiceDto =
  paths['/api/Invoices']['post']['requestBody']['content']['application/json'];
export type GetInvoicesQuery =
  paths['/api/Invoices']['get']['parameters']['query'];
export type InvoiceStatus = 'Draft' | 'Paid' | 'Cancelled';

// Re-export the paths type for use in other files
export type { paths };
