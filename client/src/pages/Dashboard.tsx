import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../api/client';
import type { BackupInfo, Dashboard } from '../types/api';
import { formatCurrency } from '../utils/format';

export function DashboardPage() {
  const [data, setData] = useState<Dashboard | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [balanceSearch, setBalanceSearch] = useState('');
  const [backups, setBackups] = useState<BackupInfo[]>([]);
  const [backupMessage, setBackupMessage] = useState('');
  const [backupError, setBackupError] = useState('');
  const [backingUp, setBackingUp] = useState(false);

  useEffect(() => {
    api.getDashboard()
      .then(setData)
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false));

    api.getBackups().then(setBackups).catch(() => {});
  }, []);

  async function handleBackup() {
    setBackingUp(true);
    setBackupError('');
    setBackupMessage('');
    try {
      const result = await api.createBackup();
      setBackupMessage(result.message);
      const list = await api.getBackups();
      setBackups(list);
    } catch (e) {
      setBackupError(e instanceof Error ? e.message : 'Backup failed');
    } finally {
      setBackingUp(false);
    }
  }

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
      <header className="page-header page-header-row">
        <div>
          <h2>Dashboard</h2>
          <p>Business overview — sales, stock & profit</p>
        </div>
      </header>

      <section className="card backup-card">
        <div className="backup-card-header">
          <div>
            <h3>Daily Data Backup</h3>
            <p>One click saves all data into dated Excel files — easy to find later.</p>
          </div>
          <button
            type="button"
            className="btn btn-primary"
            onClick={handleBackup}
            disabled={backingUp}
          >
            {backingUp ? 'Backing up...' : 'Backup Now'}
          </button>
        </div>
        {backupMessage && <div className="backup-success">{backupMessage}</div>}
        {backupError && <div className="page-error">{backupError}</div>}
        <div className="backup-help">
          <strong>Where files are saved:</strong>
          <code>data\backups\YYYY-MM-DD\</code>
          <span> — e.g. Products.xlsx, Customers.xlsx, Transactions.xlsx, Full-Backup.xlsx</span>
        </div>
        {backups.length > 0 && (
          <div className="backup-list">
            <h4>Recent Backups</h4>
            <table className="data-table">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Files</th>
                  <th>Folder</th>
                </tr>
              </thead>
              <tbody>
                {backups.map((b) => (
                  <tr key={b.dateFolder}>
                    <td>{b.dateFolder}</td>
                    <td>{b.fileCount} Excel files</td>
                    <td className="backup-path">{b.folderPath}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

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
