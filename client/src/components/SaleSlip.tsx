import type { Sale } from '../types/api';
import { formatCurrency, formatDate } from '../utils/format';

interface SaleSlipProps {
  sale: Sale;
}

const SHOP = {
  name: 'Adeel & Brother',
  subtitle: 'Cement & Sirya Agency',
  address: 'Main Market, Your City',
  phone: '03XX-XXXXXXX',
};

const LOADING_AMOUNT = 0;
const TRANSPORT_AMOUNT = 0;

function formatQty(value: number, unit?: string): string {
  const qty = Number.isInteger(value) ? String(value) : value.toFixed(2);
  return unit ? `${qty}-${unit}` : qty;
}

function slipTotals(sale: Sale) {
  const subTotal = sale.totalAmount;
  const previousBalance = sale.previousBalance;
  const loading = LOADING_AMOUNT;
  const transport = TRANSPORT_AMOUNT;
  const grandTotal = subTotal + previousBalance + loading + transport;
  const payment = sale.amountPaid;
  const balance = previousBalance + sale.balanceDue;
  const weightTotal = sale.items
    .filter((item) => (item.unit ?? '').toLowerCase() === 'kg')
    .reduce((sum, item) => sum + item.quantity, 0);

  return { subTotal, previousBalance, loading, transport, grandTotal, payment, balance, weightTotal };
}

type SummaryRow = {
  key: keyof ReturnType<typeof slipTotals>;
  label: string;
  urdu: string;
  highlight?: string;
};

const SUMMARY_ROWS: SummaryRow[] = [
  { key: 'subTotal', label: 'Sub Total', urdu: '' },
  { key: 'previousBalance', label: 'Previous Balance', urdu: 'پچھلا بقایا' },
  { key: 'loading', label: 'Loading', urdu: 'مزدوری' },
  { key: 'transport', label: 'Transport', urdu: 'گاڑی خرچہ' },
  { key: 'grandTotal', label: 'Grand Total', urdu: 'کُل رقم', highlight: 'grand' },
  { key: 'payment', label: 'Payment', urdu: 'ادا شدہ رقم' },
  { key: 'balance', label: 'Balance', urdu: 'بقایا رقم', highlight: 'balance' },
];

export function SaleSlip({ sale }: SaleSlipProps) {
  const totals = slipTotals(sale);

  return (
    <div className="sale-slip">
      <div className="sale-slip-brand">
        <h2>{SHOP.name}</h2>
        <p className="subtitle">{SHOP.subtitle}</p>
        <p className="meta">{SHOP.address} | {SHOP.phone}</p>
      </div>

      <div className="sale-slip-head">
        <div><strong>Bill #:</strong> {sale.slipNumber}</div>
        <div><strong>Date:</strong> {formatDate(sale.transactionDate)}</div>
      </div>

      <div className="sale-slip-customer">
        <p><strong>Customer:</strong> {sale.customerName}</p>
        <p><strong>Contact:</strong> {sale.customerMobile || '—'}</p>
      </div>

      <table className="sale-slip-table">
        <thead>
          <tr>
            <th className="center">Sr</th>
            <th>Product</th>
            <th className="num">Qty</th>
            <th className="num">Price</th>
            <th className="num">Total</th>
          </tr>
        </thead>
        <tbody>
          {sale.items.map((item, index) => (
            <tr key={item.productId}>
              <td className="center">{index + 1}</td>
              <td>{item.productName}</td>
              <td className="num">{formatQty(item.quantity, item.unit)}</td>
              <td className="num">{formatCurrency(item.unitPrice)}</td>
              <td className="num">{formatCurrency(item.lineTotal)}</td>
            </tr>
          ))}
        </tbody>
      </table>

      <table className="sale-slip-summary">
        <tbody>
          {SUMMARY_ROWS.map((row) => (
            <tr key={row.key} className={row.highlight ?? ''}>
              <td className="label">
                {row.label}
                {row.urdu && <span className="urdu">{row.urdu}</span>}
              </td>
              <td className="amount">{formatCurrency(totals[row.key])}</td>
            </tr>
          ))}
        </tbody>
      </table>

      {totals.weightTotal > 0 && (
        <p className="sale-slip-weight">
          <strong>Total Weight:</strong> {totals.weightTotal.toFixed(2)} Kg
        </p>
      )}

      <div className="sale-slip-extra">
        <p><strong>Remarks:</strong> {sale.notes || '________________________________'}</p>
        <p><strong>Driver:</strong> ____________________ <strong>Vehicle:</strong> ____________________</p>
      </div>

      <p className="sale-slip-footer">
        <span className="urdu">بل کے بغیر مال واپس یا تبدیل نہیں ہو گا</span>
      </p>
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
  body {
    font-family: 'Segoe UI', Tahoma, Arial, sans-serif;
    color: #111;
    padding: 8px;
    background: #fff;
  }
  .sale-slip {
    max-width: 420px;
    margin: 0 auto;
    font-size: 12px;
    line-height: 1.35;
    border: 2px solid #111;
    padding: 10px;
  }
  .sale-slip-brand {
    text-align: center;
    border-bottom: 2px solid #111;
    padding-bottom: 8px;
    margin-bottom: 8px;
  }
  .sale-slip-brand h2 {
    font-size: 20px;
    letter-spacing: 0.5px;
    text-transform: uppercase;
    margin-bottom: 2px;
  }
  .sale-slip-brand .subtitle {
    font-size: 13px;
    font-weight: 600;
  }
  .sale-slip-brand .meta {
    margin-top: 4px;
    font-size: 11px;
    color: #333;
  }
  .sale-slip-head {
    display: flex;
    justify-content: space-between;
    gap: 8px;
    margin-bottom: 8px;
    font-size: 12px;
    border-bottom: 1px dashed #666;
    padding-bottom: 6px;
  }
  .sale-slip-head strong { font-weight: 700; }
  .sale-slip-customer {
    margin-bottom: 8px;
    font-size: 12px;
  }
  .sale-slip-customer p { margin-bottom: 2px; }
  .sale-slip-table {
    width: 100%;
    border-collapse: collapse;
    margin: 8px 0;
    font-size: 11px;
  }
  .sale-slip-table th,
  .sale-slip-table td {
    border: 1px solid #111;
    padding: 3px 4px;
    vertical-align: top;
  }
  .sale-slip-table th {
    text-align: center;
    font-weight: 700;
    background: #f5f5f5;
  }
  .sale-slip-table td.num,
  .sale-slip-table th.num { text-align: right; }
  .sale-slip-table td.center,
  .sale-slip-table th.center { text-align: center; }
  .sale-slip-summary {
    width: 100%;
    border-collapse: collapse;
    margin-top: 8px;
    font-size: 12px;
  }
  .sale-slip-summary td {
    border: 1px solid #111;
    padding: 4px 6px;
  }
  .sale-slip-summary td.label { width: 58%; }
  .sale-slip-summary td.amount {
    text-align: right;
    font-weight: 700;
    width: 42%;
  }
  .sale-slip-summary tr.grand td {
    font-size: 13px;
    background: #f8f8f8;
  }
  .sale-slip-summary tr.balance td {
    color: #b91c1c;
    font-weight: 700;
  }
  .urdu {
    font-family: 'Jameel Noori Nastaleeq', 'Noto Nastaliq Urdu', 'Urdu Typesetting', serif;
    direction: rtl;
    unicode-bidi: plaintext;
    display: inline-block;
    margin-left: 6px;
    font-size: 13px;
  }
  .sale-slip-weight {
    margin-top: 6px;
    font-size: 11px;
    text-align: right;
  }
  .sale-slip-extra {
    margin-top: 8px;
    font-size: 11px;
    border-top: 1px dashed #666;
    padding-top: 6px;
  }
  .sale-slip-extra p { margin-bottom: 3px; }
  .sale-slip-footer {
    margin-top: 10px;
    text-align: center;
    font-size: 12px;
    border-top: 2px solid #111;
    padding-top: 8px;
  }
  .sale-slip-footer .urdu {
    display: block;
    margin: 0;
    line-height: 1.6;
  }
  @media print {
    body { padding: 0; }
    .sale-slip { border-width: 1px; max-width: none; }
    @page { margin: 8mm; size: auto; }
  }
`;

function buildItemRows(sale: Sale): string {
  return sale.items
    .map(
      (item, index) => `
        <tr>
          <td class="center">${index + 1}</td>
          <td>${escapeHtml(item.productName)}</td>
          <td class="num">${escapeHtml(formatQty(item.quantity, item.unit))}</td>
          <td class="num">${escapeHtml(formatCurrency(item.unitPrice))}</td>
          <td class="num">${escapeHtml(formatCurrency(item.lineTotal))}</td>
        </tr>
      `,
    )
    .join('');
}

function buildSummaryRows(sale: Sale): string {
  const totals = slipTotals(sale);
  return SUMMARY_ROWS
    .map(
      (row) => `
        <tr class="${row.highlight ?? ''}">
          <td class="label">
            ${row.label}
            ${row.urdu ? `<span class="urdu">${row.urdu}</span>` : ''}
          </td>
          <td class="amount">${escapeHtml(formatCurrency(totals[row.key]))}</td>
        </tr>
      `,
    )
    .join('');
}

function buildSaleSlipHtml(sale: Sale): string {
  const totals = slipTotals(sale);
  const weightRow = totals.weightTotal > 0
    ? `<p class="sale-slip-weight"><strong>Total Weight:</strong> ${totals.weightTotal.toFixed(2)} Kg</p>`
    : '';
  const remarks = sale.notes
    ? `<p><strong>Remarks:</strong> ${escapeHtml(sale.notes)}</p>`
    : '<p><strong>Remarks:</strong> ________________________________</p>';

  return `<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="utf-8" />
    <title>Bill ${escapeHtml(sale.slipNumber)}</title>
    <style>${PRINT_STYLES}</style>
  </head>
  <body>
    <div class="sale-slip">
      <div class="sale-slip-brand">
        <h2>${escapeHtml(SHOP.name)}</h2>
        <p class="subtitle">${escapeHtml(SHOP.subtitle)}</p>
        <p class="meta">${escapeHtml(SHOP.address)} | ${escapeHtml(SHOP.phone)}</p>
      </div>
      <div class="sale-slip-head">
        <div><strong>Bill #:</strong> ${escapeHtml(sale.slipNumber)}</div>
        <div><strong>Date:</strong> ${escapeHtml(formatDate(sale.transactionDate))}</div>
      </div>
      <div class="sale-slip-customer">
        <p><strong>Customer:</strong> ${escapeHtml(sale.customerName)}</p>
        <p><strong>Contact:</strong> ${escapeHtml(sale.customerMobile || '—')}</p>
      </div>
      <table class="sale-slip-table">
        <thead>
          <tr>
            <th class="center">Sr</th>
            <th>Product</th>
            <th class="num">Qty</th>
            <th class="num">Price</th>
            <th class="num">Total</th>
          </tr>
        </thead>
        <tbody>${buildItemRows(sale)}</tbody>
      </table>
      <table class="sale-slip-summary">
        <tbody>${buildSummaryRows(sale)}</tbody>
      </table>
      ${weightRow}
      <div class="sale-slip-extra">
        ${remarks}
        <p><strong>Driver:</strong> ____________________ <strong>Vehicle:</strong> ____________________</p>
      </div>
      <p class="sale-slip-footer">
        <span class="urdu">بل کے بغیر مال واپس یا تبدیل نہیں ہو گا</span>
      </p>
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
