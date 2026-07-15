import type {
  CreateExpenseRequest,
  CreateProductRequest,
  CreateSaleRequest,
  CreateUserRequest,
  Customer,
  CustomerHistory,
  CustomerPayment,
  CustomerLookup,
  KhataBook,
  Dealer,
  DealerHistory,
  DealerPurchase,
  DealerPayment,
  AdvanceBooking,
  SyncStatus,
  SyncResult,
  SalesReport,
  CustomerBalanceReport,
  DealerOutstandingReport,
  InventoryReport,
  LowStockReport,
  PurchaseReport,
  ProfitReport,
  AdvanceBookingReport,
  Dashboard,
  Expense,
  InventoryItem,
  LoginRequest,
  LoginResponse,
  CurrentUserResponse,
  BackupInfo,
  BackupResult,
  Product,
  RecordPaymentRequest,
  Sale,
  ScreenInfo,
  UpdateUserRequest,
  UserAccount,
} from '../types/api';

const API_BASE = '/api';

function getToken(): string | null {
  const raw = sessionStorage.getItem('abc_auth');
  if (!raw) return null;
  try {
    const parsed = JSON.parse(raw) as { token?: string };
    return parsed.token ?? null;
  } catch {
    return null;
  }
}

async function request<T>(url: string, options?: RequestInit): Promise<T> {
  const token = getToken();
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options?.headers as Record<string, string> | undefined),
  };
  if (token) headers.Authorization = `Bearer ${token}`;

  const response = await fetch(`${API_BASE}${url}`, {
    ...options,
    headers,
  });

  if (response.status === 401) {
    sessionStorage.removeItem('abc_auth');
    if (!window.location.pathname.startsWith('/login')) {
      window.location.href = '/login';
    }
    throw new Error('Session expired. Please sign in again.');
  }

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: response.statusText }));
    throw new Error(error.message || 'Request failed');
  }

  if (response.status === 204) return undefined as T;
  return response.json();
}

export const api = {
  login: (data: LoginRequest) =>
    request<LoginResponse>('/auth/login', { method: 'POST', body: JSON.stringify(data) }),
  getMe: () => request<CurrentUserResponse>('/auth/me'),
  getUsers: () => request<UserAccount[]>('/users'),
  getScreens: () => request<ScreenInfo[]>('/users/screens'),
  createUser: (data: CreateUserRequest) =>
    request<UserAccount>('/users', { method: 'POST', body: JSON.stringify(data) }),
  updateUser: (id: string, data: UpdateUserRequest) =>
    request<UserAccount>(`/users/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  deleteUser: (id: string) => request<void>(`/users/${id}`, { method: 'DELETE' }),
  getDashboard: () => request<Dashboard>('/dashboard'),
  createBackup: () => request<BackupResult>('/backup', { method: 'POST' }),
  getBackups: (limit = 10) => request<BackupInfo[]>(`/backup?limit=${limit}`),
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
  searchCustomers: (q: string) => request<Customer[]>(`/customers/search?q=${encodeURIComponent(q)}`),
  lookupCustomer: (name: string, mobile: string) =>
    request<CustomerLookup>(`/customers/lookup?name=${encodeURIComponent(name)}&mobile=${encodeURIComponent(mobile)}`),
  getKhataBook: (id: string) => request<KhataBook>(`/customers/${id}/khata`),
  syncExcel: () => request<SyncResult>('/sync', { method: 'POST' }),
  getSyncStatus: () => request<SyncStatus>('/sync/status'),
  getDealers: () => request<Dealer[]>('/dealers'),
  getDealerHistory: (id: string) => request<DealerHistory>(`/dealers/${id}/history`),
  createDealer: (data: { name: string; phone?: string; address?: string }) =>
    request<Dealer>('/dealers', { method: 'POST', body: JSON.stringify(data) }),
  recordDealerPurchase: (data: {
    dealerId: string; productId: string; quantity: number; unitPrice: number;
    amountPaid?: number; purchaseDate?: string; notes?: string;
  }) => request<DealerPurchase>('/dealers/purchases', { method: 'POST', body: JSON.stringify(data) }),
  recordDealerPayment: (id: string, data: RecordPaymentRequest) =>
    request<DealerPayment>(`/dealers/${id}/payments`, { method: 'POST', body: JSON.stringify(data) }),
  getBookings: () => request<AdvanceBooking[]>('/bookings'),
  createBooking: (data: {
    customerName: string; customerMobile: string; productId: string;
    quantity: number; unitPrice: number; advancePaid: number;
    deliveryDate: string; notes?: string;
  }) => request<AdvanceBooking>('/bookings', { method: 'POST', body: JSON.stringify(data) }),
  deliverBooking: (id: string, amountPaid?: number) =>
    request<Sale>(`/bookings/${id}/deliver`, {
      method: 'POST',
      body: JSON.stringify(amountPaid != null ? { amount: amountPaid } : {}),
    }),
  getDailySalesReport: (date?: string) =>
    request<SalesReport>(`/reports/daily-sales${date ? `?date=${date}` : ''}`),
  getMonthlySalesReport: (year?: number, month?: number) => {
    const params = new URLSearchParams();
    if (year) params.set('year', String(year));
    if (month) params.set('month', String(month));
    const q = params.toString();
    return request<SalesReport>(`/reports/monthly-sales${q ? `?${q}` : ''}`);
  },
  getCustomerBalanceReport: () => request<CustomerBalanceReport>('/reports/customer-balances'),
  getDealerOutstandingReport: () => request<DealerOutstandingReport>('/reports/dealer-outstanding'),
  getInventoryReport: () => request<InventoryReport>('/reports/inventory'),
  getLowStockReport: () => request<LowStockReport>('/reports/low-stock'),
  getPurchaseReport: (from: string, to: string) =>
    request<PurchaseReport>(`/reports/purchases?from=${from}&to=${to}`),
  getProfitReport: (from: string, to: string) =>
    request<ProfitReport>(`/reports/profit?from=${from}&to=${to}`),
  getAdvanceBookingReport: () => request<AdvanceBookingReport>('/reports/advance-bookings'),
};
