import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../api/client';
import type { Dashboard } from '../types/api';
import { formatCurrency } from '../utils/format';

export function DashboardPage() {
  const [data, setData] = useState<Dashboard | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [balanceSearch, setBalanceSearch] = useState('');

  useEffect(() => {
    api.getDashboard()
      .then(setData)
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false));
  }, []);

  const filteredCustomers = useMemo(() => {
    if (!data) return [];
    const query = balanceSearch.trim().toLowerCase();
    if (!query) return data.customersWithBalance;
    return data.customersWithBalance.filter((c) =>
      c.name.toLowerCase().includes(query) ||
      (c.phone?.toLowerCase().includes(query) ?? false)
    );
  }, [data, balanceSearch]);

  if (loading) return <div className="page-loading">Loading dashboard...</div>;
  if (error) return <div className="page-error">{error}</div>;
  if (!data) return null;

  const lowStock = data.inventory.filter((i) => i.stockQuantity <= 10);

  return (
    <div className="page">
      <header className="page-header">
        <h2>Dashboard</h2>
        <p>Business overview — sales, stock & profit</p>
      </header>

      <div className="stat-grid">
        <div className="stat-card accent-blue">
          <span className="stat-label">Today Sales</span>
          <strong>{formatCurrency(data.todaySales)}</strong>
        </div>
        <div className="stat-card accent-green">
          <span className="stat-label">Today Profit</span>
          <strong>{formatCurrency(data.todayProfit)}</strong>
        </div>
        <div className="stat-card accent-red">
          <span className="stat-label">Today Expenses</span>
          <strong>{formatCurrency(data.todayExpenses)}</strong>
        </div>
        <div className="stat-card accent-purple">
          <span className="stat-label">Net Profit Today</span>
          <strong>{formatCurrency(data.netProfitToday)}</strong>
        </div>
      </div>

      <div className="stat-grid secondary">
        <div className="stat-card">
          <span className="stat-label">This Week</span>
          <strong>{formatCurrency(data.weekSales)}</strong>
        </div>
        <div className="stat-card">
          <span className="stat-label">This Month</span>
          <strong>{formatCurrency(data.monthSales)}</strong>
        </div>
        <div className="stat-card">
          <span className="stat-label">This Year</span>
          <strong>{formatCurrency(data.yearSales)}</strong>
        </div>
        <div className="stat-card accent-red">
          <span className="stat-label">Customer Balance (Udhaar)</span>
          <strong>{formatCurrency(data.totalOutstanding)}</strong>
        </div>
      </div>

      <section className="card">
        <div className="card-header">
          <h3>Customer Balance List</h3>
          <Link to="/customers" className="btn small">View All</Link>
        </div>
        <div className="search-bar">
          <input
            type="search"
            placeholder="Search by customer name or mobile..."
            value={balanceSearch}
            onChange={(e) => setBalanceSearch(e.target.value)}
          />
        </div>
        {data.customersWithBalance.length === 0 ? (
          <p className="empty-state">No customer balances — all paid up!</p>
        ) : filteredCustomers.length === 0 ? (
          <p className="empty-state">No customers match "{balanceSearch}"</p>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Customer</th>
                <th>Mobile</th>
                <th>Balance</th>
              </tr>
            </thead>
            <tbody>
              {filteredCustomers.map((customer) => (
                <tr key={customer.id}>
                  <td><strong>{customer.name}</strong></td>
                  <td>{customer.phone || '—'}</td>
                  <td className="balance-due">{formatCurrency(customer.balance)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </section>

      <div className="grid-2">
        <section className="card">
          <h3>Sales Summary</h3>
          <table className="data-table">
            <thead>
              <tr>
                <th>Period</th>
                <th>Sales</th>
                <th>Cost</th>
                <th>Profit</th>
                <th>Orders</th>
              </tr>
            </thead>
            <tbody>
              {data.salesByPeriod.map((s) => (
                <tr key={s.period}>
                  <td>{s.period}</td>
                  <td>{formatCurrency(s.totalSales)}</td>
                  <td>{formatCurrency(s.totalCost)}</td>
                  <td className="profit">{formatCurrency(s.totalProfit)}</td>
                  <td>{s.transactionCount}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </section>

        <section className="card">
          <h3>Top Products (This Month)</h3>
          <table className="data-table">
            <thead>
              <tr>
                <th>Product</th>
                <th>Category</th>
                <th>Qty Sold</th>
                <th>Sales</th>
                <th>Profit</th>
              </tr>
            </thead>
            <tbody>
              {data.topProducts.length === 0 ? (
                <tr><td colSpan={5} className="empty">No sales this month yet</td></tr>
              ) : (
                data.topProducts.map((p) => (
                  <tr key={p.productName}>
                    <td>{p.productName}</td>
                    <td><span className="badge">{p.category}</span></td>
                    <td>{p.quantitySold}</td>
                    <td>{formatCurrency(p.totalSales)}</td>
                    <td className="profit">{formatCurrency(p.totalProfit)}</td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </section>
      </div>

      <section className="card">
        <div className="card-header">
          <h3>Inventory Status</h3>
          {lowStock.length > 0 && (
            <span className="warning-badge">{lowStock.length} items low stock</span>
          )}
        </div>
        <table className="data-table">
          <thead>
            <tr>
              <th>Category</th>
              <th>Product</th>
              <th>Stock</th>
              <th>Unit</th>
              <th>Value</th>
            </tr>
          </thead>
          <tbody>
            {data.inventory.map((item) => (
              <tr key={item.id} className={item.stockQuantity <= 10 ? 'low-stock' : ''}>
                <td><span className="badge">{item.category}</span></td>
                <td>{item.name}</td>
                <td>{item.stockQuantity}</td>
                <td>{item.unit}</td>
                <td>{formatCurrency(item.stockValue)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </div>
  );
}
