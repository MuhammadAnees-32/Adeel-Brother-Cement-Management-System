import type { KhataBook } from '../types/api';
import { formatCurrency, formatDateTime } from '../utils/format';

function escapeHtml(value: string): string {
  return value
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}

const A4_STYLES = `
  * { box-sizing: border-box; margin: 0; padding: 0; }
  body { font-family: 'Segoe UI', system-ui, sans-serif; color: #1e293b; padding: 20mm; font-size: 12px; }
  .header { text-align: center; border-bottom: 2px solid #1e293b; padding-bottom: 12px; margin-bottom: 16px; }
  .header h1 { font-size: 22px; margin-bottom: 4px; }
  .header p { color: #64748b; font-size: 13px; }
  .meta { margin-bottom: 16px; }
  .meta p { margin-bottom: 4px; }
  table { width: 100%; border-collapse: collapse; margin-top: 12px; font-size: 11px; }
  th { background: #f1f5f9; text-align: left; padding: 8px 6px; border: 1px solid #cbd5e1; }
  td { padding: 6px; border: 1px solid #e2e8f0; }
  th:nth-child(n+4), td:nth-child(n+4) { text-align: right; }
  .total { margin-top: 16px; text-align: right; font-size: 14px; font-weight: 700; }
  .footer { margin-top: 24px; text-align: center; font-size: 10px; color: #64748b; }
  @media print { @page { size: A4; margin: 15mm; } body { padding: 0; } }
`;

export function printKhataStatement(khata: KhataBook) {
  const rows = khata.entries.map((e) => `
    <tr>
      <td>${escapeHtml(formatDateTime(e.date))}</td>
      <td>${escapeHtml(e.type)}</td>
      <td>${escapeHtml(e.description)}</td>
      <td>${escapeHtml(formatCurrency(e.previousBalance))}</td>
      <td>${e.purchaseAmount > 0 ? escapeHtml(formatCurrency(e.purchaseAmount)) : '—'}</td>
      <td>${e.paymentReceived > 0 ? escapeHtml(formatCurrency(e.paymentReceived)) : '—'}</td>
      <td><strong>${escapeHtml(formatCurrency(e.remainingBalance))}</strong></td>
    </tr>
  `).join('');

  const html = `<!DOCTYPE html><html><head><meta charset="utf-8" />
    <title>Khata - ${escapeHtml(khata.customer.name)}</title>
    <style>${A4_STYLES}</style></head><body>
    <div class="header"><h1>Adeel &amp; Brother</h1><p>Cement &amp; Sirya Agency — Customer Khata Statement</p></div>
    <div class="meta">
      <p><strong>Customer:</strong> ${escapeHtml(khata.customer.name)}</p>
      <p><strong>Mobile:</strong> ${escapeHtml(khata.customer.phone || '—')}</p>
      <p><strong>Statement Date:</strong> ${escapeHtml(formatDateTime(new Date().toISOString()))}</p>
    </div>
    <table>
      <thead><tr>
        <th>Date</th><th>Type</th><th>Description</th>
        <th>Previous Balance</th><th>New Purchase</th><th>Payment</th><th>Remaining</th>
      </tr></thead>
      <tbody>${rows || '<tr><td colspan="7">No transactions</td></tr>'}</tbody>
    </table>
    <p class="total">Current Balance: ${escapeHtml(formatCurrency(khata.currentBalance))}</p>
    <p class="footer">This is a computer-generated statement. Transaction history is permanent.</p>
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
  doc.open();
  doc.write(html);
  doc.close();
  win.focus();
  win.print();
}
