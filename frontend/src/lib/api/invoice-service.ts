// Invoice service with type-safe API calls
import { api, type ApiResponse } from './client';
import type { paths } from './schema';

// Extract types from the OpenAPI schema
export type InvoiceDto =
  paths['/api/Invoices/{id}']['get']['responses']['200']['content']['application/json'];
export type CreateInvoiceDto =
  paths['/api/Invoices']['post']['requestBody']['content']['application/json'];
export type UpdateInvoiceDto =
  paths['/api/Invoices/{id}']['put']['requestBody']['content']['application/json'];
export type GetInvoicesResponse =
  paths['/api/Invoices']['get']['responses']['200']['content']['application/json'];
export type InvoiceStatus = 0 | 1 | 2 | 3; // Draft=0, Sent=1, Paid=2, Cancelled=3

export interface GetInvoicesParams {
  customerName?: string;
  status?: InvoiceStatus;
  pageSize?: number;
  pageNumber?: number;
}

class InvoiceService {
  /**
   * Get all invoices with optional filtering
   */
  async getInvoices(
    params: GetInvoicesParams = {}
  ): Promise<GetInvoicesResponse> {
    const { data, error } = await api.GET('/api/Invoices', {
      params: {
        query: params,
      },
    });

    if (error) {
      throw new Error(`Failed to fetch invoices: ${JSON.stringify(error)}`);
    }

    return data;
  }

  /**
   * Get a single invoice by ID
   */
  async getInvoice(id: string): Promise<InvoiceDto> {
    const { data, error } = await api.GET('/api/Invoices/{id}', {
      params: {
        path: { id },
      },
    });

    if (error) {
      throw new Error(`Failed to fetch invoice: ${JSON.stringify(error)}`);
    }

    return data;
  }

  /**
   * Create a new invoice
   */
  async createInvoice(invoice: CreateInvoiceDto): Promise<InvoiceDto> {
    const { data, error } = await api.POST('/api/Invoices', {
      body: invoice,
    });

    if (error) {
      throw new Error(`Failed to create invoice: ${JSON.stringify(error)}`);
    }

    return data;
  }

  /**
   * Update an existing invoice
   */
  async updateInvoice(
    id: string,
    invoice: UpdateInvoiceDto
  ): Promise<InvoiceDto> {
    const { data, error } = await api.PUT('/api/Invoices/{id}', {
      params: {
        path: { id },
      },
      body: invoice,
    });

    if (error) {
      throw new Error(`Failed to update invoice: ${JSON.stringify(error)}`);
    }

    return data;
  }

  /**
   * Mark an invoice as paid
   */
  async markInvoiceAsPaid(id: string): Promise<InvoiceDto> {
    const { data, error } = await api.PATCH('/api/Invoices/{id}/mark-paid', {
      params: {
        path: { id },
      },
    });

    if (error) {
      throw new Error(
        `Failed to mark invoice as paid: ${JSON.stringify(error)}`
      );
    }

    return data;
  }

  /**
   * Delete an invoice
   */
  async deleteInvoice(id: string): Promise<void> {
    const { error } = await api.DELETE('/api/Invoices/{id}', {
      params: {
        path: { id },
      },
    });

    if (error) {
      throw new Error(`Failed to delete invoice: ${JSON.stringify(error)}`);
    }
  }
}

// Export a singleton instance
export const invoiceService = new InvoiceService();
