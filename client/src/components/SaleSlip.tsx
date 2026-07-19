import type { Sale } from '../types/api';
import { formatCurrency, formatDateTime } from '../utils/format';

interface SaleSlipProps {
  sale: Sale;
}

const SHOP = {
  name: 'Adeel and Brothers',
  subtitle: 'Sarya and Cement Dealer',
  address: 'Lari Adha Oghi',
  phone: '03463850062, 03409258212',
};

function formatQty(value: number, unit?: string): string {
  const qty = Number.isInteger(value) ? String(value) : value.toFixed(2);
  return unit ? `${qty} ${unit}` : qty;
}

function slipTotals(sale: Sale) {
  const subTotal = sale.totalAmount;
  const previousBalance = sale.previousBalance;
  const loading = sale.loadingCharge ?? 0;
  const transport = sale.transportCharge ?? 0;
  const billTotal = subTotal + loading + transport;
  const grandTotal = billTotal;
  const payment = sale.amountPaid;
  const balance = previousBalance + sale.balanceDue;

  return { subTotal, previousBalance, loading, transport, billTotal, grandTotal, payment, balance };
}

type SummaryRow = {
  key: keyof ReturnType<typeof slipTotals>;
  label: string;
  urdu: string;
  highlight?: string;
  hideWhenZero?: boolean;
};

const SUMMARY_ROWS: SummaryRow[] = [
  { key: 'subTotal', label: 'Sub Total', urdu: 'ذیلی کل' },
  { key: 'previousBalance', label: 'Previous Balance', urdu: 'پچھلا بقایا', hideWhenZero: true },
  { key: 'loading', label: 'Loading', urdu: 'مزدوری' },
  { key: 'transport', label: 'Transport', urdu: 'گاڑی خرچہ' },
  { key: 'grandTotal', label: 'Grand Total', urdu: 'کل رقم', highlight: 'grand' },
  { key: 'payment', label: 'Payment', urdu: 'ادا شدہ رقم' },
  { key: 'balance', label: 'Balance', urdu: 'بقایا رقم', highlight: 'balance' },
];

function visibleSummaryRows(sale: Sale) {
  const totals = slipTotals(sale);
  return SUMMARY_ROWS.filter((row) => {
    if (!row.hideWhenZero) return true;
    return totals[row.key] !== 0;
  });
}

export function SaleSlip({ sale }: SaleSlipProps) {
  const totals = slipTotals(sale);
  const rows = visibleSummaryRows(sale);
  const driver = sale.driverName?.trim() || '____________________';
  const vehicle = sale.vehicleNumber?.trim() || '____________________';

  return (
    <div className="sale-slip">
      <div className="sale-slip-brand">
        <h2>{SHOP.name}</h2>
        <p className="subtitle">{SHOP.subtitle}</p>
        <p className="meta">{SHOP.address} | {SHOP.phone}</p>
      </div>

      <div className="sale-slip-head">
        <div><strong>Bill #:</strong> {sale.slipNumber}</div>
        <div><strong>Date:</strong> {formatDateTime(sale.transactionDate)}</div>
      </div>

      <div className="sale-slip-customer">
        <p><strong>Customer:</strong> {sale.customerName}</p>
        <p><strong>Mobile:</strong> {sale.customerMobile || '—'}</p>
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
            <tr key={`${item.productId}-${index}`}>
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
          {rows.map((row) => (
            <tr key={row.key} className={row.highlight ?? ''}>
              <td className="label">
                {row.label}
                {row.urdu && <span className="urdu">{row.urdu}</span>}
              </td>
              <td className="amount">{formatCurrency(totals[row.key] ?? 0)}</td>
            </tr>
          ))}
        </tbody>
      </table>

      <div className="sale-slip-extra">
        <p><strong>Remarks:</strong> {sale.notes || '________________________________'}</p>
        <p><strong>Driver:</strong> {driver} <strong>Vehicle #:</strong> {vehicle}</p>
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
  html, body {
    font-family: Arial, Helvetica, 'Segoe UI', Tahoma, sans-serif;
    color: #000;
    background: #fff;
  }
  .sale-slip {
    max-width: 302px;
    width: 302px;
    margin: 0 auto;
    font-size: 13px;
    line-height: 1.45;
    border: 2px solid #000;
    padding: 10px;
    color: #000;
  }
  .sale-slip-brand {
    text-align: center;
    border-bottom: 2px solid #000;
    padding-bottom: 8px;
    margin-bottom: 8px;
  }
  .sale-slip-brand h2 {
    font-size: 17px;
    font-weight: 800;
    letter-spacing: 0;
    text-transform: uppercase;
    margin-bottom: 3px;
    color: #000;
  }
  .sale-slip-brand .subtitle {
    font-size: 13px;
    font-weight: 700;
    color: #000;
  }
  .sale-slip-brand .meta {
    margin-top: 5px;
    font-size: 11px;
    font-weight: 600;
    color: #000;
  }
  .sale-slip-head {
    display: flex;
    justify-content: space-between;
    gap: 8px;
    margin-bottom: 8px;
    font-size: 12px;
    font-weight: 600;
    border-bottom: 1px solid #000;
    padding-bottom: 6px;
    color: #000;
  }
  .sale-slip-head strong { font-weight: 800; }
  .sale-slip-customer {
    margin-bottom: 8px;
    font-size: 12px;
    font-weight: 600;
    color: #000;
  }
  .sale-slip-customer p { margin-bottom: 3px; }
  .sale-slip-customer strong { font-weight: 800; }
  .sale-slip-table {
    width: 100%;
    border-collapse: collapse;
    margin: 8px 0;
    font-size: 12px;
  }
  .sale-slip-table th,
  .sale-slip-table td {
    border: 1px solid #000;
    padding: 4px 5px;
    vertical-align: top;
    color: #000;
  }
  .sale-slip-table th {
    text-align: center;
    font-weight: 800;
    background: #fff;
  }
  .sale-slip-table td.num,
  .sale-slip-table th.num { text-align: right; font-variant-numeric: tabular-nums; }
  .sale-slip-table td.center,
  .sale-slip-table th.center { text-align: center; }
  .sale-slip-summary {
    width: 100%;
    border-collapse: collapse;
    margin-top: 8px;
    font-size: 12px;
  }
  .sale-slip-summary td {
    border: 1px solid #000;
    padding: 5px 6px;
    color: #000;
  }
  .sale-slip-summary td.label { width: 58%; font-weight: 600; }
  .sale-slip-summary td.amount {
    text-align: right;
    font-weight: 800;
    width: 42%;
    font-variant-numeric: tabular-nums;
  }
  .sale-slip-summary tr.grand td {
    font-size: 13px;
    font-weight: 800;
    background: #fff;
  }
  .sale-slip-summary tr.balance td {
    color: #000;
    font-weight: 800;
  }
  .urdu {
    font-family: 'Noto Nastaliq Urdu', 'Urdu Typesetting', 'Jameel Noori Nastaleeq', Tahoma, serif;
    direction: rtl;
    unicode-bidi: plaintext;
    display: inline-block;
    margin-left: 6px;
    font-size: 13px;
    font-weight: 700;
    color: #000;
  }
  .sale-slip-extra {
    margin-top: 8px;
    font-size: 11px;
    font-weight: 600;
    border-top: 1px solid #000;
    padding-top: 6px;
    color: #000;
  }
  .sale-slip-extra p { margin-bottom: 4px; }
  .sale-slip-extra strong { font-weight: 800; }
  .sale-slip-footer {
    margin-top: 10px;
    text-align: center;
    font-size: 12px;
    border-top: 2px solid #000;
    padding-top: 8px;
    color: #000;
  }
  .sale-slip-footer .urdu {
    display: block;
    margin: 0;
    line-height: 1.7;
    font-size: 14px;
  }
  @media print {
    html, body {
      padding: 0;
      margin: 0;
      color: #000 !important;
      -webkit-font-smoothing: none !important;
      -moz-osx-font-smoothing: unset !important;
      text-rendering: geometricPrecision;
      -webkit-print-color-adjust: exact;
      print-color-adjust: exact;
    }
    .sale-slip {
      border: 2px solid #000;
      max-width: none;
      width: 72mm;
      font-size: 11pt;
      line-height: 1.4;
      padding: 2mm;
    }
    .sale-slip-brand h2 { font-size: 14pt; font-weight: 800; }
    .sale-slip-brand .subtitle { font-size: 11pt; }
    .sale-slip-brand .meta { font-size: 9pt; }
    .sale-slip-head,
    .sale-slip-customer { font-size: 10pt; }
    .sale-slip-table { font-size: 10pt; }
    .sale-slip-table th,
    .sale-slip-table td { padding: 1.2mm 1.5mm; border-color: #000; }
    .sale-slip-table th { background: #fff !important; }
    .sale-slip-summary { font-size: 10pt; }
    .sale-slip-summary tr.grand td { font-size: 11pt; background: #fff !important; }
    .sale-slip-extra { font-size: 9pt; }
    .sale-slip-footer { font-size: 10pt; }
    .sale-slip-footer .urdu { font-size: 12pt; }
    .urdu { font-size: 11pt; }
    @page { margin: 2mm; size: 80mm auto; }
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
  return visibleSummaryRows(sale)
    .map(
      (row) => `
        <tr class="${row.highlight ?? ''}">
          <td class="label">
            ${row.label}
            ${row.urdu ? `<span class="urdu">${row.urdu}</span>` : ''}
          </td>
          <td class="amount">${escapeHtml(formatCurrency(totals[row.key] ?? 0))}</td>
        </tr>
      `,
    )
    .join('');
}

function buildSaleSlipHtml(sale: Sale): string {
  const remarks = sale.notes
    ? `<p><strong>Remarks:</strong> ${escapeHtml(sale.notes)}</p>`
    : '<p><strong>Remarks:</strong> ________________________________</p>';
  const driver = sale.driverName?.trim() || '____________________';
  const vehicle = sale.vehicleNumber?.trim() || '____________________';

  return `<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="utf-8" />
    <title>Bill ${escapeHtml(sale.slipNumber)}</title>
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
    <link href="https://fonts.googleapis.com/css2?family=Noto+Nastaliq+Urdu:wght@400;700&display=swap" rel="stylesheet" />
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
        <div><strong>Date:</strong> ${escapeHtml(formatDateTime(sale.transactionDate))}</div>
      </div>
      <div class="sale-slip-customer">
        <p><strong>Customer:</strong> ${escapeHtml(sale.customerName)}</p>
        <p><strong>Mobile:</strong> ${escapeHtml(sale.customerMobile || '—')}</p>
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
      <div class="sale-slip-extra">
        ${remarks}
        <p><strong>Driver:</strong> ${escapeHtml(driver)} <strong>Vehicle #:</strong> ${escapeHtml(vehicle)}</p>
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
    'position:fixed;left:-10000px;top:0;width:302px;height:800px;border:0;',
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
    // Brief delay so fonts/layout render at full size before print (avoids blurry text)
    setTimeout(() => printWindow.print(), 350);
  };

  if (doc.readyState === 'complete') {
    triggerPrint();
  } else {
    iframe.onload = triggerPrint;
  }
}
