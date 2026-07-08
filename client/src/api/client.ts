import type {
  CreateExpenseRequest,
  CreateProductRequest,
  CreateSaleRequest,
  Customer,
  CustomerHistory,
  CustomerPayment,
  Dashboard,
  Expense,
  InventoryItem,
  Product,
  RecordPaymentRequest,
  Sale,
} from '../types/api';

const API_BASE = '/api';

async function request<T>(url: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE}${url}`, {
    headers: { 'Content-Type': 'application/json' },
    ...options,
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: response.statusText }));
    throw new Error(error.message || 'Request failed');
  }

  if (response.status === 204) return undefined as T;
  return response.json();
}

export const api = {
  getDashboard: () => request<Dashboard>('/dashboard'),
  getProducts: (category?: string) =>
    request<Product[]>(category ? `/products?category=${category}` : '/products'),
  updateProduct: (id: string, data: { purchasePrice: number; salePrice: number; stockQuantity: number }) =>
    request<Product>(`/products/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  createProduct: (data: CreateProductRequest) =>
    request<Product>('/products', { method: 'POST', body: JSON.stringify(data) }),
  deleteProduct: (id: string) =>
    request<void>(`/products/${id}`, { method: 'DELETE' }),
  getSales: () => request<Sale[]>('/sales'),
  getSale: (id: string) => request<Sale>(`/sales/${id}`),
  createSale: (data: CreateSaleRequest) =>
    request<Sale>('/sales', { method: 'POST', body: JSON.stringify(data) }),
  getInventory: () => request<InventoryItem[]>('/inventory'),
  adjustStock: (productId: string, quantity: number, reason: string) =>
    request<InventoryItem>(`/inventory/${productId}/adjust`, {
      method: 'POST',
      body: JSON.stringify({ quantity, reason }),
    }),
  setStock: (productId: string, quantity: number, reason: string) =>
    request<InventoryItem>(`/inventory/${productId}/set`, {
      method: 'PUT',
      body: JSON.stringify({ quantity, reason }),
    }),
  getExpenses: () => request<Expense[]>('/expenses'),
  createExpense: (data: CreateExpenseRequest) =>
    request<Expense>('/expenses', { method: 'POST', body: JSON.stringify(data) }),
  deleteExpense: (id: string) =>
    request<void>(`/expenses/${id}`, { method: 'DELETE' }),
  getCustomers: (balanceOnly = false) =>
    request<Customer[]>(`/customers?balanceOnly=${balanceOnly}`),
  getCustomer: (id: string) => request<Customer>(`/customers/${id}`),
  getCustomerHistory: (id: string) => request<CustomerHistory>(`/customers/${id}/history`),
  getCustomerPayments: (id: string) => request<CustomerPayment[]>(`/customers/${id}/payments`),
  recordPayment: (id: string, data: RecordPaymentRequest) =>
    request<CustomerPayment>(`/customers/${id}/payments`, { method: 'POST', body: JSON.stringify(data) }),
};
