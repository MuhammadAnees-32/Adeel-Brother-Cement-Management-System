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
