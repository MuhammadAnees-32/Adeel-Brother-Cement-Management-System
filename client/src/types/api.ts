export interface Product {
  id: string;
  category: string;
  name: string;
  unit: string;
  purchasePrice: number;
  salePrice: number;
  stockQuantity: number;
  isActive: boolean;
}

export interface SaleItem {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  unitCost: number;
  lineTotal: number;
  lineProfit: number;
}

export interface Sale {
  id: string;
  slipNumber: string;
  customerId?: string;
  customerName: string;
  customerMobile: string;
  transactionDate: string;
  totalAmount: number;
  amountPaid: number;
  balanceDue: number;
  previousBalance: number;
  totalCost: number;
  totalProfit: number;
  notes?: string;
  items: SaleItem[];
}

export interface Expense {
  id: string;
  expenseDate: string;
  category: string;
  description: string;
  amount: number;
}

export interface InventoryItem {
  id: string;
  category: string;
  name: string;
  unit: string;
  stockQuantity: number;
  purchasePrice: number;
  salePrice: number;
  stockValue: number;
  dealerName?: string;
  totalPurchased: number;
  totalSold: number;
  remainingStock: number;
}

export interface SalesSummary {
  period: string;
  totalSales: number;
  totalCost: number;
  totalProfit: number;
  transactionCount: number;
}

export interface ProductSales {
  productName: string;
  category: string;
  quantitySold: number;
  totalSales: number;
  totalProfit: number;
}

export interface Dashboard {
  todaySales: number;
  todayProfit: number;
  todayExpenses: number;
  netProfitToday: number;
  weekSales: number;
  monthSales: number;
  yearSales: number;
  totalExpensesThisMonth: number;
  netProfitThisMonth: number;
  totalOutstanding: number;
  inventory: InventoryItem[];
  salesByPeriod: SalesSummary[];
  topProducts: ProductSales[];
  customersWithBalance: Customer[];
}

export interface CreateSaleRequest {
  customerName: string;
  customerMobile: string;
  customerId?: string;
  transactionDate?: string;
  amountPaid?: number;
  notes?: string;
  items: { productId: string; quantity: number; unitPrice?: number }[];
}

export interface Customer {
  id: string;
  name: string;
  phone?: string;
  address?: string;
  balance: number;
}

export interface CustomerPayment {
  id: string;
  customerId: string;
  customerName: string;
  amount: number;
  paymentDate: string;
  notes?: string;
}

export interface CustomerHistory {
  customer: Customer;
  sales: Sale[];
  payments: CustomerPayment[];
}

export interface RecordPaymentRequest {
  amount: number;
  paymentDate?: string;
  notes?: string;
}

export interface CreateExpenseRequest {
  expenseDate: string;
  category: string;
  description: string;
  amount: number;
}

export interface CreateProductRequest {
  category: string;
  name: string;
  unit: string;
  purchasePrice: number;
  salePrice: number;
  stockQuantity: number;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  username: string;
  role: string;
  allowedScreens: string[];
}

export interface UserAccount {
  id: string;
  username: string;
  role: 'Admin' | 'Salesman';
  allowedScreens: string[];
  isActive: boolean;
}

export interface ScreenInfo {
  key: string;
  label: string;
}

export interface CurrentUserResponse {
  username: string;
  role: string;
  allowedScreens: string[];
}

export interface CreateUserRequest {
  username: string;
  password: string;
  role: string;
  allowedScreens?: string[];
}

export interface UpdateUserRequest {
  password?: string;
  role: string;
  allowedScreens: string[];
  isActive: boolean;
}

export interface BackupInfo {
  dateFolder: string;
  folderPath: string;
  fileCount: number;
  lastUpdated: string;
}

export interface BackupResult {
  dateFolder: string;
  folderPath: string;
  files: string[];
  createdAt: string;
  message: string;
}

export interface CustomerLookup {
  exists: boolean;
  customer?: Customer;
  message: string;
}

export interface KhataEntry {
  date: string;
  type: string;
  description: string;
  reference?: string;
  previousBalance: number;
  purchaseAmount: number;
  paymentReceived: number;
  remainingBalance: number;
}

export interface KhataBook {
  customer: Customer;
  entries: KhataEntry[];
  currentBalance: number;
}

export interface Dealer {
  id: string;
  name: string;
  phone?: string;
  address?: string;
  outstandingBalance: number;
}

export interface DealerPurchase {
  id: string;
  dealerId: string;
  dealerName: string;
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  totalAmount: number;
  amountPaid: number;
  balanceDue: number;
  purchaseDate: string;
  notes?: string;
}

export interface DealerPayment {
  id: string;
  dealerId: string;
  dealerName: string;
  amount: number;
  paymentDate: string;
  notes?: string;
}

export interface DealerHistory {
  dealer: Dealer;
  purchases: DealerPurchase[];
  payments: DealerPayment[];
}

export interface AdvanceBooking {
  id: string;
  customerId: string;
  customerName: string;
  customerMobile: string;
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  totalAmount: number;
  advancePaid: number;
  remainingAmount: number;
  deliveryDate: string;
  bookedDate: string;
  status: string;
  invoiceId?: string;
  notes?: string;
}

export interface SyncStatus {
  lastSuccessfulSync?: string;
  lastMessage?: string;
  oneDriveConfigured: boolean;
}

export interface SyncResult {
  success: boolean;
  message: string;
  syncedAt: string;
  backupPath?: string;
  serverCopyPath?: string;
  oneDriveCopyPath?: string;
}

export interface SalesReport {
  title: string;
  from: string;
  to: string;
  totalSales: number;
  totalCost: number;
  totalProfit: number;
  transactionCount: number;
  sales: Sale[];
}

export interface CustomerBalanceReport {
  customers: Customer[];
  totalOutstanding: number;
}

export interface DealerOutstandingReport {
  dealers: Dealer[];
  totalOutstanding: number;
}

export interface InventoryReport {
  items: InventoryItem[];
  totalStockValue: number;
}

export interface LowStockReport {
  items: InventoryItem[];
}

export interface PurchaseReport {
  from: string;
  to: string;
  purchases: DealerPurchase[];
  totalPurchases: number;
  totalPaid: number;
  totalOutstanding: number;
}

export interface ProfitReport {
  from: string;
  to: string;
  totalSales: number;
  totalCost: number;
  grossProfit: number;
  totalExpenses: number;
  netProfit: number;
}

export interface AdvanceBookingReport {
  pending: AdvanceBooking[];
  delivered: AdvanceBooking[];
  all: AdvanceBooking[];
}
