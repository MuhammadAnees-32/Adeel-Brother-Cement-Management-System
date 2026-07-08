import { useEffect, useState } from 'react';
import { api } from '../api/client';
import { printSaleSlip, SaleSlip } from '../components/SaleSlip';
import type { Customer, Sale } from '../types/api';
import { formatCurrency, formatDateTime, toInputDate } from '../utils/format';

export function SalesHistoryPage() {
  const [sales, setSales] = useState<Sale[]>([]);
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [selected, setSelected] = useState<Sale | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [showPaymentForm, setShowPaymentForm] = useState(false);
  const [paymentAmount, setPaymentAmount] = useState<number | ''>('');
  const [paymentDate, setPaymentDate] = useState(toInputDate());
  const [paymentNotes, setPaymentNotes] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const loadSales = () =>
    api.getSales()
      .then(setSales)
      .catch((e) => setError(e.message));

  useEffect(() => {
    Promise.all([loadSales(), api.getCustomers(false).then(setCustomers)])
      .finally(() => setLoading(false));
  }, []);

  const selectSale = (sale: Sale) => {
    setSelected(sale);
    setShowPaymentForm(false);
    setPaymentAmount('');
    setError('');
    setSuccess('');
  };

  const findCustomerForSale = (sale: Sale): Customer | undefined => {
    if (sale.customerId) {
      return customers.find((c) => c.id === sale.customerId);
    }
    const normalizedMobile = sale.customerMobile.replace(/\D/g, '');
    return customers.find((c) => (c.phone ?? '').replace(/\D/g, '') === normalizedMobile);
  };

  const customer = selected ? findCustomerForSale(selected) : undefined;
  const customerBalance = customer?.balance ?? 0;
  const slipBalance = selected?.balanceDue ?? 0;
  const enteredAmount = paymentAmount === '' ? 0 : paymentAmount;
  const remainingAfterPayment = Math.max(0, customerBalance - enteredAmount);

  const openPaymentForm = () => {
    setPaymentAmount('');
    setPaymentDate(toInputDate());
    setPaymentNotes('');
    setShowPaymentForm(true);
    setError('');
    setSuccess('');
  };

  const submitPayment = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selected || !customer) return;
    if (enteredAmount <= 0) {
      setError('Payment amount must be greater than zero');
      return;
    }
    if (enteredAmount > customerBalance) {
      setError(`Payment cannot exceed customer balance of ${formatCurrency(customerBalance)}`);
      return;
    }

    setSubmitting(true);
    try {
      setError('');
      await api.recordPayment(customer.id, {
        amount: enteredAmount,
        paymentDate: new Date(paymentDate).toISOString(),
        notes: paymentNotes || `Payment for slip ${selected.slipNumber}`,
      });
      const remainingText =
        remainingAfterPayment > 0
          ? ` Remaining balance: ${formatCurrency(remainingAfterPayment)}.`
          : ' Balance cleared.';
      setSuccess(`Received ${formatCurrency(enteredAmount)} from ${customer.name}.${remainingText}`);
      setShowPaymentForm(false);
      setPaymentAmount('');
      const [updatedSales, updatedCustomers] = await Promise.all([
        api.getSales(),
        api.getCustomers(false),
      ]);
      setSales(updatedSales);
      setCustomers(updatedCustomers);
      setSelected(updatedSales.find((s) => s.id === selected.id) ?? null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Payment failed');
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return <div className="page-loading">Loading sales...</div>;
  if (error && sales.length === 0) return <div className="page-error">{error}</div>;

  return (
    <div className="page">
      <header className="page-header">
        <h2>Sales History</h2>
        <p>All customer transactions & slips</p>
      </header>

      {error && <div className="alert error">{error}</div>}
      {success && <div className="alert success">{success}</div>}

      <div className="grid-2">
        <section className="card">
          <table className="data-table">
            <thead>
              <tr>
                <th>Slip #</th>
                <th>Customer</th>
                <th>Date</th>
                <th>Amount</th>
                <th>Balance</th>
              </tr>
            </thead>
            <tbody>
              {sales.length === 0 ? (
                <tr><td colSpan={5} className="empty">No sales recorded yet</td></tr>
              ) : (
                sales.map((sale) => (
                  <tr
                    key={sale.id}
                    className={selected?.id === sale.id ? 'selected' : 'clickable'}
                    onClick={() => selectSale(sale)}
                  >
                    <td>{sale.slipNumber}</td>
                    <td>{sale.customerName}</td>
                    <td>{formatDateTime(sale.transactionDate)}</td>
                    <td>{formatCurrency(sale.totalAmount)}</td>
                    <td className={sale.balanceDue > 0 ? 'balance-due' : ''}>
                      {sale.balanceDue > 0 ? formatCurrency(sale.balanceDue) : '—'}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </section>

        <section className="card">
          {selected ? (
            <>
              <div className="slip-header">
                <h3>Slip Details</h3>
                <div className="slip-actions">
                  {slipBalance > 0 && customer && customerBalance > 0 && (
                    <button className="btn primary" type="button" onClick={openPaymentForm}>
                      Receive Payment
                    </button>
                  )}
                  <button className="btn" onClick={() => printSaleSlip(selected)} type="button">
                    Print Slip
                  </button>
                </div>
              </div>

              {showPaymentForm && customer && (
                <form onSubmit={submitPayment} className="payment-form-inline form-grid">
                  <p className="full-width payment-hint">
                    Slip balance: <strong>{formatCurrency(slipBalance)}</strong>
                    {' · '}
                    Customer owes: <strong>{formatCurrency(customerBalance)}</strong>
                  </p>
                  <label>
                    Amount Received (PKR)
                    <input
                      type="number"
                      min={1}
                      max={customerBalance}
                      step={1}
                      value={paymentAmount}
                      onChange={(e) => setPaymentAmount(e.target.value === '' ? '' : Number(e.target.value))}
                      placeholder="e.g. 500"
                      required
                      autoFocus
                    />
                  </label>
                  <label>
                    Payment Date
                    <input type="date" value={paymentDate} onChange={(e) => setPaymentDate(e.target.value)} />
                  </label>
                  {enteredAmount > 0 && enteredAmount <= customerBalance && (
                    <p className="full-width payment-hint">
                      Remaining after payment: <strong>{formatCurrency(remainingAfterPayment)}</strong>
                    </p>
                  )}
                  <label className="full-width">
                    Notes
                    <input value={paymentNotes} onChange={(e) => setPaymentNotes(e.target.value)} placeholder="Optional" />
                  </label>
                  <div className="form-action full-width">
                    <button className="btn primary" type="submit" disabled={submitting}>
                      {submitting ? 'Saving...' : 'Save Payment'}
                    </button>
                    <button className="btn secondary" type="button" onClick={() => setShowPaymentForm(false)}>
                      Cancel
                    </button>
                  </div>
                </form>
              )}

              <SaleSlip sale={selected} />
            </>
          ) : (
            <p className="empty-state">Select a sale to view slip details</p>
          )}
        </section>
      </div>
    </div>
  );
}
