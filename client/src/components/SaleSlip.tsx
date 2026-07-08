import type { Sale } from '../types/api';
import { formatCurrency, formatDateTime } from '../utils/format';

interface SaleSlipProps {
  sale: Sale;
}

export function SaleSlip({ sale }: SaleSlipProps) {
  return (
    <div className="sale-slip">
      <div className="sale-slip-brand">
        <h2>Adeel & Brother</h2>
        <p>Cement & Sirya Agency</p>
      </div>
      <p className="sale-slip-number">{sale.slipNumber}</p>
      <div className="sale-slip-meta">
        <p><strong>Customer:</strong> {sale.customerName}</p>
        <p><strong>Mobile:</strong> {sale.customerMobile || '—'}</p>
        <p><strong>Date:</strong> {formatDateTime(sale.transactionDate)}</p>
        {sale.notes && <p><strong>Notes:</strong> {sale.notes}</p>}
      </div>
      <table className="sale-slip-table">
        <thead>
          <tr>
            <th>Item</th>
            <th>Qty</th>
            <th>Rate</th>
            <th>Total</th>
          </tr>
        </thead>
        <tbody>
          {sale.items.map((item) => (
            <tr key={item.productId}>
              <td>{item.productName}</td>
              <td>{item.quantity}</td>
              <td>{formatCurrency(item.unitPrice)}</td>
              <td>{formatCurrency(item.lineTotal)}</td>
            </tr>
          ))}
        </tbody>
      </table>
      <div className="sale-slip-totals">
        <div className="sale-slip-total-row">
          <span>Total</span>
          <strong>{formatCurrency(sale.totalAmount)}</strong>
        </div>
        <div className="sale-slip-total-row">
          <span>Paid</span>
          <strong>{formatCurrency(sale.amountPaid)}</strong>
        </div>
        {sale.balanceDue > 0 && (
          <div className="sale-slip-total-row balance">
            <span>Balance Due</span>
            <strong>{formatCurrency(sale.balanceDue)}</strong>
          </div>
        )}
      </div>
      <p className="sale-slip-footer">Thank you for your business</p>
    </div>
  );
}

function escapeHtml(value: string): string {
  return value
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}

const PRINT_STYLES = `
  * { box-sizing: border-box; margin: 0; padding: 0; }
  body { font-family: 'Segoe UI', system-ui, sans-serif; color: #1e293b; padding: 12px; }
  .sale-slip { max-width: 360px; margin: 0 auto; font-size: 13px; line-height: 1.4; }
  .sale-slip-brand { text-align: center; border-bottom: 2px solid #1e293b; padding-bottom: 8px; margin-bottom: 10px; }
  .sale-slip-brand h2 { font-size: 18px; margin-bottom: 2px; }
  .sale-slip-brand p { font-size: 12px; color: #64748b; }
  .sale-slip-number { text-align: center; font-size: 15px; font-weight: 700; margin-bottom: 10px; }
  .sale-slip-meta p { margin-bottom: 4px; font-size: 12px; }
  .sale-slip-table { width: 100%; border-collapse: collapse; margin: 12px 0; font-size: 12px; }
  .sale-slip-table th { text-align: left; border-bottom: 1px solid #1e293b; padding: 4px 2px; font-size: 11px; }
  .sale-slip-table td { padding: 4px 2px; border-bottom: 1px solid #e2e8f0; }
  .sale-slip-table th:nth-child(2), .sale-slip-table td:nth-child(2),
  .sale-slip-table th:nth-child(3), .sale-slip-table td:nth-child(3),
  .sale-slip-table th:nth-child(4), .sale-slip-table td:nth-child(4) { text-align: right; }
  .sale-slip-totals { border-top: 2px solid #1e293b; padding-top: 8px; }
  .sale-slip-total-row { display: flex; justify-content: space-between; padding: 3px 0; font-size: 13px; }
  .sale-slip-total-row.balance strong { color: #dc2626; }
  .sale-slip-footer { text-align: center; margin-top: 14px; font-size: 11px; color: #64748b; }
  @media print {
    body { padding: 0; }
    @page { margin: 10mm; }
  }
`;

function buildSaleSlipHtml(sale: Sale): string {
  const itemRows = sale.items
    .map(
      (item) => `
        <tr>
          <td>${escapeHtml(item.productName)}</td>
          <td>${item.quantity}</td>
          <td>${escapeHtml(formatCurrency(item.unitPrice))}</td>
          <td>${escapeHtml(formatCurrency(item.lineTotal))}</td>
        </tr>
      `,
    )
    .join('');

  const balanceRow =
    sale.balanceDue > 0
      ? `
        <div class="sale-slip-total-row balance">
          <span>Balance Due</span>
          <strong>${escapeHtml(formatCurrency(sale.balanceDue))}</strong>
        </div>
      `
      : '';

  const notesRow = sale.notes
    ? `<p><strong>Notes:</strong> ${escapeHtml(sale.notes)}</p>`
    : '';

  return `<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="utf-8" />
    <title>Slip ${escapeHtml(sale.slipNumber)}</title>
    <style>${PRINT_STYLES}</style>
  </head>
  <body>
    <div class="sale-slip">
      <div class="sale-slip-brand">
        <h2>Adeel &amp; Brother</h2>
        <p>Cement &amp; Sirya Agency</p>
      </div>
      <p class="sale-slip-number">${escapeHtml(sale.slipNumber)}</p>
      <div class="sale-slip-meta">
        <p><strong>Customer:</strong> ${escapeHtml(sale.customerName)}</p>
        <p><strong>Mobile:</strong> ${escapeHtml(sale.customerMobile || '—')}</p>
        <p><strong>Date:</strong> ${escapeHtml(formatDateTime(sale.transactionDate))}</p>
        ${notesRow}
      </div>
      <table class="sale-slip-table">
        <thead>
          <tr>
            <th>Item</th>
            <th>Qty</th>
            <th>Rate</th>
            <th>Total</th>
          </tr>
        </thead>
        <tbody>${itemRows}</tbody>
      </table>
      <div class="sale-slip-totals">
        <div class="sale-slip-total-row">
          <span>Total</span>
          <strong>${escapeHtml(formatCurrency(sale.totalAmount))}</strong>
        </div>
        <div class="sale-slip-total-row">
          <span>Paid</span>
          <strong>${escapeHtml(formatCurrency(sale.amountPaid))}</strong>
        </div>
        ${balanceRow}
      </div>
      <p class="sale-slip-footer">Thank you for your business</p>
    </div>
  </body>
</html>`;
}

export function printSaleSlip(sale: Sale) {
  const iframe = document.createElement('iframe');
  iframe.setAttribute(
    'style',
    'position:fixed;right:0;bottom:0;width:0;height:0;border:0;visibility:hidden',
  );
  document.body.appendChild(iframe);

  const printWindow = iframe.contentWindow;
  const doc = iframe.contentDocument ?? printWindow?.document;
  if (!printWindow || !doc) {
    iframe.remove();
    return;
  }

  const cleanup = (() => {
    let done = false;
    return () => {
      if (done) return;
      done = true;
      iframe.remove();
    };
  })();

  printWindow.onafterprint = cleanup;
  setTimeout(cleanup, 30_000);

  doc.open();
  doc.write(buildSaleSlipHtml(sale));
  doc.close();

  const triggerPrint = () => {
    printWindow.focus();
    printWindow.print();
  };

  if (doc.readyState === 'complete') {
    triggerPrint();
  } else {
    iframe.onload = triggerPrint;
  }
}
