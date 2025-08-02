import React, { useEffect, useState } from 'react';
import { ProtectedRoute } from '@/lib/auth/AuthContext';

interface InvoiceItem {
  description: string;
  amount: { amount: number; currency: string };
}

interface Invoice {
  id: string;
  customerName: string;
  items: InvoiceItem[];
}

interface CreateInvoiceItemDto {
  description: string;
  amount: number;
}

const API = '/api/invoices';

export default function InvoiceManager() {
  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const [selected, setSelected] = useState<Invoice | null>(null);
  const [form, setForm] = useState({
    customerName: '',
    items: [{ description: '', amount: 0 }],
  });
  const [editId, setEditId] = useState<string | null>(null);
  const [touched, setTouched] = useState<{ [key: string]: boolean }>({});

  useEffect(() => {
    fetch(API)
      .then(r => r.json())
      .then(setInvoices);
  }, []);

  const handleChange = (idx: number, field: string, value: string | number) => {
    setForm(f => ({
      ...f,
      items: f.items.map((item, i) =>
        i === idx ? { ...item, [field]: value } : item
      ),
    }));
    // Mark field as touched
    setTouched(t => ({ ...t, [`${idx}-${field}`]: true }));
  };

  const addItem = () =>
    setForm(f => ({
      ...f,
      items: [...f.items, { description: '', amount: 0 }],
    }));

  const removeItem = (idx: number) => {
    if (form.items.length > 1) {
      setForm(f => ({ ...f, items: f.items.filter((_, i) => i !== idx) }));
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const payload = {
      customerName: form.customerName,
      items: form.items.map(i => ({
        description: i.description,
        amount: Number(i.amount),
      })),
    };
    if (editId) {
      await fetch(`${API}/${editId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
      });
    } else {
      await fetch(API, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
      });
    }
    setForm({ customerName: '', items: [{ description: '', amount: 0 }] });
    setEditId(null);
    setTouched({});
    fetch(API)
      .then(r => r.json())
      .then(setInvoices);
  };

  const startEdit = (inv: Invoice) => {
    setEditId(inv.id);
    setForm({
      customerName: inv.customerName,
      items: inv.items.map(i => ({
        description: i.description,
        amount: i.amount.amount,
      })),
    });
  };

  return (
    <ProtectedRoute>
      <div className="max-w-4xl mx-auto p-6">
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">Invoices</h1>
          <p className="text-gray-600 mt-2">Create and manage your invoices</p>
        </div>
        <div className="bg-white border border-gray-200 rounded-lg shadow-sm p-6 mb-6">
          <h2 className="text-lg font-semibold mb-4">
            {editId ? 'Edit Invoice' : 'Create New Invoice'}
          </h2>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Customer Name
              </label>
              <input
                className="border border-gray-300 rounded-md p-3 w-full focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="Enter customer name"
                value={form.customerName}
                onChange={e =>
                  setForm(f => ({ ...f, customerName: e.target.value }))
                }
                required
              />
            </div>

            <div>
              <h3 className="text-base font-medium text-gray-900 mb-3">
                Line Items
              </h3>
              <p className="text-sm text-gray-600 mb-4">
                Add the products or services for this invoice
              </p>
              <div className="space-y-3">
                <div className="grid grid-cols-12 gap-2 text-sm font-medium text-gray-700">
                  <div className="col-span-8">Description</div>
                  <div className="col-span-3 text-right">Amount</div>
                  <div className="col-span-1"></div>
                </div>
                {form.items.map((item, idx) => (
                  <div key={idx} className="space-y-1">
                    <div className="grid grid-cols-12 gap-2 items-start">
                      <div className="col-span-8">
                        <input
                          className="border border-gray-300 rounded-md p-2 w-full focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                          placeholder="Product or service description"
                          value={item.description}
                          onChange={e =>
                            handleChange(idx, 'description', e.target.value)
                          }
                          onBlur={() =>
                            setTouched(t => ({
                              ...t,
                              [`${idx}-description`]: true,
                            }))
                          }
                          required
                        />
                        {/* Reserve space for validation message */}
                        <div className="h-5 mt-1">
                          {touched[`${idx}-description`] && !item.description && (
                            <p className="text-red-500 text-xs">
                              Description is required
                            </p>
                          )}
                        </div>
                      </div>
                      <div className="col-span-3">
                        <input
                          className="border border-gray-300 rounded-md p-2 w-full text-right focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                          type="number"
                          min="0"
                          step="0.01"
                          placeholder="0.00"
                          value={item.amount || ''}
                          onChange={e =>
                            handleChange(idx, 'amount', e.target.value)
                          }
                          onBlur={() =>
                            setTouched(t => ({ ...t, [`${idx}-amount`]: true }))
                          }
                          required
                        />
                        {/* Reserve space for validation message */}
                        <div className="h-5 mt-1 text-right">
                          {touched[`${idx}-amount`] &&
                            (!item.amount || Number(item.amount) <= 0) && (
                              <p className="text-red-500 text-xs whitespace-nowrap">
                                Amount required
                              </p>
                            )}
                        </div>
                      </div>
                      <div className="col-span-1 flex justify-center items-start pt-2">
                        {form.items.length > 1 && (
                          <button
                            type="button"
                            onClick={() => removeItem(idx)}
                            className="text-gray-400 hover:text-red-500 p-1"
                          >
                            <svg
                              className="w-4 h-4"
                              fill="none"
                              stroke="currentColor"
                              viewBox="0 0 24 24"
                            >
                              <path
                                strokeLinecap="round"
                                strokeLinejoin="round"
                                strokeWidth={2}
                                d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
                              />
                            </svg>
                          </button>
                        )}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
              <div className="flex justify-between items-center pt-4 border-t border-gray-200">
                <button
                  type="button"
                  className="text-blue-600 hover:text-blue-800 font-medium flex items-center gap-1"
                  onClick={addItem}
                >
                  <svg
                    className="w-4 h-4"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M12 6v6m0 0v6m0-6h6m-6 0H6"
                    />
                  </svg>
                  Add Item
                </button>
                <div className="text-right">
                  <div className="text-sm text-gray-600">Total:</div>
                  <div className="text-xl font-bold">
                    $
                    {form.items
                      .reduce((sum, item) => sum + (Number(item.amount) || 0), 0)
                      .toFixed(2)}
                  </div>
                </div>
              </div>
            </div>

            <div className="flex justify-between items-center pt-6 border-t border-gray-200">
              <button
                type="submit"
                className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-2 rounded-md font-medium transition-colors"
              >
                {editId ? 'Update Invoice' : 'Create Invoice'}
              </button>
              {editId && (
                <button
                  type="button"
                  className="ml-3 text-gray-600 hover:text-gray-800 px-4 py-2 border border-gray-300 rounded-md transition-colors"
                  onClick={() => {
                    setEditId(null);
                    setForm({
                      customerName: '',
                      items: [{ description: '', amount: 0 }],
                    });
                    setTouched({});
                  }}
                >
                  Cancel
                </button>
              )}
            </div>
          </form>
        </div>
        <div className="bg-white border border-gray-200 rounded-lg shadow-sm overflow-hidden">
          <table className="w-full">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Customer
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Items
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Total
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {invoices.map(inv => (
                <tr key={inv.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                    {inv.customerName}
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-900">
                    <div className="space-y-1">
                      {inv.items.map((item, i) => (
                        <div key={i} className="flex justify-between">
                          <span>{item.description}</span>
                          <span className="font-medium">
                            ${item.amount.amount.toFixed(2)}
                          </span>
                        </div>
                      ))}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                    $
                    {inv.items
                      .reduce((sum, item) => sum + item.amount.amount, 0)
                      .toFixed(2)}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-blue-600">
                    <button
                      className="hover:text-blue-800 font-medium"
                      onClick={() => startEdit(inv)}
                    >
                      Edit
                    </button>
                  </td>
                </tr>
              ))}
              {invoices.length === 0 && (
                <tr>
                  <td colSpan={4} className="px-6 py-8 text-center text-gray-500">
                    No invoices yet. Create your first invoice above.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </ProtectedRoute>
  );
}
