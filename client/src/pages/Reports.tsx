import { useState } from 'react';
import { api } from '../api/client';
import { formatCurrency, formatDate, toInputDate } from '../utils/format';

type ReportType =
  | 'daily-sales' | 'monthly-sales' | 'customer-balances' | 'dealer-outstanding'
  | 'inventory' | 'low-stock' | 'purchases' | 'profit' | 'advance-bookings';

function printReport(title: string, contentHtml: string) {
  const html = `<!DOCTYPE html><html><head><meta charset="utf-8" /><title>${title}</title>
    <style>
      body { font-family: Segoe UI, sans-serif; padding: 20mm; font-size: 12px; color: #1e293b; }
      h1 { font-size: 18px; margin-bottom: 8px; }
      table { width: 100%; border-collapse: collapse; margin-top: 12px; }
      th, td { border: 1px solid #cbd5e1; padding: 6px 8px; text-align: left; }
      th { background: #f1f5f9; }
      .right { text-align: right; }
      @media print { @page { size: A4; margin: 15mm; } }
    </style></head><body>
    <h1>Adeel and Brothers — ${title}</h1>
    <p>Generated: ${new Date().toLocaleString()}</p>
    ${contentHtml}
  </body></html>`;
  const iframe = document.createElement('iframe');
  iframe.style.cssText = 'position:fixed;width:0;height:0;border:0;visibility:hidden';
  document.body.appendChild(iframe);
  const win = iframe.contentWindow;
  const doc = iframe.contentDocument ?? win?.document;
  if (!win || !doc) { iframe.remove(); return; }
  const cleanup = () => iframe.remove();
  win.onafterprint = cleanup;
  setTimeout(cleanup, 30000);
  doc.open(); doc.write(html); doc.close();
  win.focus(); win.print();
}

export function ReportsPage() {
  const [reportType, setReportType] = useState<ReportType>('daily-sales');
  const [from, setFrom] = useState(toInputDate(new Date(new Date().getFullYear(), new Date().getMonth(), 1)));
  const [to, setTo] = useState(toInputDate());
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [html, setHtml] = useState('');

  const loadReport = async () => {
    setLoading(true);
    setError('');
    try {
      let content = '';
      switch (reportType) {
        case 'daily-sales': {
          const r = await api.getDailySalesReport(to);
          content = `<p><strong>Total Sales:</strong> ${formatCurrency(r.totalSales)} | <strong>Profit:</strong> ${formatCurrency(r.totalProfit)} | <strong>Transactions:</strong> ${r.transactionCount}</p>
            <table><thead><tr><th>Slip</th><th>Customer</th><th>Date</th><th class="right">Amount</th></tr></thead><tbody>
            ${r.sales.map((s) => `<tr><td>${s.slipNumber}</td><td>${s.customerName}</td><td>${formatDate(s.transactionDate)}</td><td class="right">${formatCurrency(s.totalAmount)}</td></tr>`).join('')}
            </tbody></table>`;
          break;
        }
        case 'monthly-sales': {
          const r = await api.getMonthlySalesReport();
          content = `<p><strong>Total Sales:</strong> ${formatCurrency(r.totalSales)} | <strong>Profit:</strong> ${formatCurrency(r.totalProfit)}</p>
            <table><thead><tr><th>Slip</th><th>Customer</th><th>Date</th><th class="right">Amount</th></tr></thead><tbody>
            ${r.sales.map((s) => `<tr><td>${s.slipNumber}</td><td>${s.customerName}</td><td>${formatDate(s.transactionDate)}</td><td class="right">${formatCurrency(s.totalAmount)}</td></tr>`).join('')}
            </tbody></table>`;
          break;
        }
        case 'customer-balances': {
          const r = await api.getCustomerBalanceReport();
          content = `<p><strong>Total Outstanding:</strong> ${formatCurrency(r.totalOutstanding)}</p>
            <table><thead><tr><th>Customer</th><th>Mobile</th><th class="right">Balance</th></tr></thead><tbody>
            ${r.customers.map((c) => `<tr><td>${c.name}</td><td>${c.phone || '—'}</td><td class="right">${formatCurrency(c.balance)}</td></tr>`).join('')}
            </tbody></table>`;
          break;
        }
        case 'dealer-outstanding': {
          const r = await api.getDealerOutstandingReport();
          content = `<p><strong>Total Outstanding:</strong> ${formatCurrency(r.totalOutstanding)}</p>
            <table><thead><tr><th>Dealer</th><th class="right">Outstanding</th></tr></thead><tbody>
            ${r.dealers.map((d) => `<tr><td>${d.name}</td><td class="right">${formatCurrency(d.outstandingBalance)}</td></tr>`).join('')}
            </tbody></table>`;
          break;
        }
        case 'inventory': {
          const r = await api.getInventoryReport();
          content = `<p><strong>Total Stock Value:</strong> ${formatCurrency(r.totalStockValue)}</p>
            <table><thead><tr><th>Product</th><th>Dealer</th><th class="right">Stock</th><th class="right">Buy</th><th class="right">Sell</th><th class="right">Purchased</th><th class="right">Sold</th></tr></thead><tbody>
            ${r.items.map((i) => `<tr><td>${i.name}</td><td>${i.dealerName || '—'}</td><td class="right">${i.stockQuantity}</td><td class="right">${formatCurrency(i.purchasePrice)}</td><td class="right">${formatCurrency(i.salePrice)}</td><td class="right">${i.totalPurchased}</td><td class="right">${i.totalSold}</td></tr>`).join('')}
            </tbody></table>`;
          break;
        }
        case 'low-stock': {
          const r = await api.getLowStockReport();
          content = `<table><thead><tr><th>Product</th><th class="right">Stock</th></tr></thead><tbody>
            ${r.items.map((i) => `<tr><td>${i.name}</td><td class="right">${i.stockQuantity} ${i.unit}</td></tr>`).join('')}
            </tbody></table>`;
          break;
        }
        case 'purchases': {
          const r = await api.getPurchaseReport(from, to);
          content = `<p><strong>Total:</strong> ${formatCurrency(r.totalPurchases)} | <strong>Paid:</strong> ${formatCurrency(r.totalPaid)} | <strong>Due:</strong> ${formatCurrency(r.totalOutstanding)}</p>
            <table><thead><tr><th>Date</th><th>Dealer</th><th>Product</th><th class="right">Total</th></tr></thead><tbody>
            ${r.purchases.map((p) => `<tr><td>${formatDate(p.purchaseDate)}</td><td>${p.dealerName}</td><td>${p.productName}</td><td class="right">${formatCurrency(p.totalAmount)}</td></tr>`).join('')}
            </tbody></table>`;
          break;
        }
        case 'profit': {
          const r = await api.getProfitReport(from, to);
          content = `<p><strong>Sales:</strong> ${formatCurrency(r.totalSales)} | <strong>Cost:</strong> ${formatCurrency(r.totalCost)} | <strong>Gross:</strong> ${formatCurrency(r.grossProfit)} | <strong>Expenses:</strong> ${formatCurrency(r.totalExpenses)} | <strong>Net:</strong> ${formatCurrency(r.netProfit)}</p>`;
          break;
        }
        case 'advance-bookings': {
          const r = await api.getAdvanceBookingReport();
          content = `<h3>Pending (${r.pending.length})</h3><table><thead><tr><th>Customer</th><th>Product</th><th class="right">Advance</th><th class="right">Remaining</th></tr></thead><tbody>
            ${r.pending.map((b) => `<tr><td>${b.customerName}</td><td>${b.productName}</td><td class="right">${formatCurrency(b.advancePaid)}</td><td class="right">${formatCurrency(b.remainingAmount)}</td></tr>`).join('')}
            </tbody></table>`;
          break;
        }
      }
      setHtml(content);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load report');
    } finally {
      setLoading(false);
    }
  };

  const reportLabels: Record<ReportType, string> = {
    'daily-sales': 'Daily Sales Report',
    'monthly-sales': 'Monthly Sales Report',
    'customer-balances': 'Customer Balance Report',
    'dealer-outstanding': 'Dealer Outstanding Report',
    inventory: 'Inventory Report',
    'low-stock': 'Low Stock Report',
    purchases: 'Purchase Report',
    profit: 'Profit Report',
    'advance-bookings': 'Advance Booking Report',
  };

  return (
    <div className="page">
      <header className="page-header">
        <h2>Reports</h2>
        <p>Generate, print and export business reports (PDF via Print)</p>
      </header>

      {error && <div className="alert error">{error}</div>}

      <section className="card">
        <div className="form-grid">
          <label>
            Report Type
            <select value={reportType} onChange={(e) => setReportType(e.target.value as ReportType)}>
              {Object.entries(reportLabels).map(([k, v]) => (
                <option key={k} value={k}>{v}</option>
              ))}
            </select>
          </label>
          {(reportType === 'purchases' || reportType === 'profit') && (
            <>
              <label>From<input type="date" value={from} onChange={(e) => setFrom(e.target.value)} /></label>
              <label>To<input type="date" value={to} onChange={(e) => setTo(e.target.value)} /></label>
            </>
          )}
          <div className="form-action">
            <button type="button" className="btn primary" onClick={loadReport} disabled={loading}>
              {loading ? 'Loading...' : 'Generate Report'}
            </button>
            {html && (
              <button type="button" className="btn" onClick={() => printReport(reportLabels[reportType], html)}>
                Print / Save PDF
              </button>
            )}
          </div>
        </div>
      </section>

      {html && (
        <section className="card report-preview" dangerouslySetInnerHTML={{ __html: html }} />
      )}
    </div>
  );
}
