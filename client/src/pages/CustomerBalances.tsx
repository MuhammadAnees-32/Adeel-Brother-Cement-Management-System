import { useEffect, useState } from 'react';
import { api } from '../api/client';
import type { Customer, CustomerHistory } from '../types/api';
import { formatCurrency, formatDate, formatDateTime, toInputDate } from '../utils/format';

export function CustomerBalancesPage() {
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [loading, setLoading] = useState(true);
  const [historyLoading, setHistoryLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [selected, setSelected] = useState<Customer | null>(null);
  const [history, setHistory] = useState<CustomerHistory | null>(null);
  const [showPaymentForm, setShowPaymentForm] = useState(false);
  const [paymentAmount, setPaymentAmount] = useState<number | ''>('');
  const [paymentDate, setPaymentDate] = useState(toInputDate());
  const [paymentNotes, setPaymentNotes] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const load = () => {
    setLoading(true);
    api.getCustomers(true)
      .then(setCustomers)
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, []);

  const totalBalance = customers.reduce((sum, c) => sum + c.balance, 0);

  const selectCustomer = async (customer: Customer) => {
    setSelected(customer);
    setShowPaymentForm(false);
    setPaymentAmount('');
    setError('');
    setSuccess('');
    setHistoryLoading(true);
    try {
      const data = await api.getCustomerHistory(customer.id);
      setHistory(data);
      setSelected(data.customer);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load history');
      setHistory(null);
    } finally {
      setHistoryLoading(false);
    }
  };

  const openPaymentForm = () => {
    if (!selected) return;
    setPaymentAmount('');
    setPaymentDate(toInputDate());
    setPaymentNotes('');
    setShowPaymentForm(true);
    setError('');
    setSuccess('');
  };

  const outstanding = history?.customer.balance ?? selected?.balance ?? 0;
  const enteredAmount = paymentAmount === '' ? 0 : paymentAmount;
  const remainingAfterPayment = Math.max(0, outstanding - enteredAmount);

  const submitPayment = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selected) return;
    if (enteredAmount <= 0) {
      setError('Payment amount must be greater than zero');
      return;
    }
    if (enteredAmount > outstanding) {
      setError(`Payment cannot exceed outstanding balance of ${formatCurrency(outstanding)}`);
      return;
    }

    setSubmitting(true);
    try {
      setError('');
      await api.recordPayment(selected.id, {
        amount: enteredAmount,
        paymentDate: new Date(paymentDate).toISOString(),
        notes: paymentNotes || undefined,
      });
      const remainingText =
        remainingAfterPayment > 0
          ? ` Remaining balance: ${formatCurrency(remainingAfterPayment)}.`
          : ' Balance cleared.';
      setSuccess(`Received ${formatCurrency(enteredAmount)} from ${selected.name}.${remainingText}`);
      setShowPaymentForm(false);
      setPaymentAmount('');
      load();
      await selectCustomer(selected);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Payment failed');
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return <div className="page-loading">Loading customer balances...</div>;

  return (
    <div className="page">
      <header className="page-header">
        <h2>Customer Balances</h2>
        <p>Customers who owe money (udhaar / balance)</p>
      </header>

      <div className="stat-grid">
        <div className="stat-card accent-red">
          <span className="stat-label">Total Outstanding</span>
          <strong>{formatCurrency(totalBalance)}</strong>
        </div>
        <div className="stat-card">
          <span className="stat-label">Customers with Balance</span>
          <strong>{customers.length}</strong>
        </div>
      </div>

      {error && <div className="alert error">{error}</div>}
      {success && <div className="alert success">{success}</div>}

      <div className="grid-2">
        <section className="card">
          <h3>Balance List</h3>
          {customers.length === 0 ? (
            <p className="empty-state">No customer balances — all paid up!</p>
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
                {customers.map((customer) => (
                  <tr
                    key={customer.id}
                    className={selected?.id === customer.id ? 'selected' : 'clickable'}
                    onClick={() => selectCustomer(customer)}
                  >
                    <td><strong>{customer.name}</strong></td>
                    <td>{customer.phone || '—'}</td>
                    <td className="balance-due">{formatCurrency(customer.balance)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </section>

        <section className="card customer-detail-panel">
          {!selected ? (
            <p className="empty-state">Click a customer to view complete purchase & payment history</p>
          ) : historyLoading ? (
            <p className="empty-state">Loading history...</p>
          ) : history ? (
            <>
              <div className="customer-detail-header">
                <div>
                  <h3>{history.customer.name}</h3>
                  <p className="detail-meta">{history.customer.phone || '—'}</p>
                  <p className="balance-highlight inline">
                    Outstanding: <strong>{formatCurrency(history.customer.balance)}</strong>
                  </p>
                </div>
                {history.customer.balance > 0 && (
                  <button className="btn primary" type="button" onClick={openPaymentForm}>
                    Receive Payment
                  </button>
                )}
              </div>

              {showPaymentForm && (
                <form onSubmit={submitPayment} className="payment-form-inline form-grid">
                  <p className="full-width payment-hint">
                    Enter partial or full payment. Outstanding: <strong>{formatCurrency(outstanding)}</strong>
                  </p>
                  <label>
                    Amount Received (PKR)
                    <input
                      type="number"
                      min={1}
                      max={outstanding}
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
                  {enteredAmount > 0 && enteredAmount <= outstanding && (
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

              <h4 className="section-title">Purchase History</h4>
              {history.sales.length === 0 ? (
                <p className="empty">No purchases recorded</p>
              ) : (
                <div className="history-list">
                  {history.sales.map((sale) => (
                    <div key={sale.id} className="history-sale-card">
                      <div className="history-sale-header">
                        <div>
                          <strong>{sale.slipNumber}</strong>
                          <span className="detail-meta">{formatDateTime(sale.transactionDate)}</span>
                        </div>
                        <div className="history-sale-totals">
                          <span>Total: {formatCurrency(sale.totalAmount)}</span>
                          <span>Paid: {formatCurrency(sale.amountPaid)}</span>
                          {sale.balanceDue > 0 && (
                            <span className="balance-due">Balance: {formatCurrency(sale.balanceDue)}</span>
                          )}
                        </div>
                      </div>
                      <table className="data-table compact">
                        <thead>
                          <tr><th>Item</th><th>Qty</th><th>Rate</th><th>Total</th></tr>
                        </thead>
                        <tbody>
                          {sale.items.map((item) => (
                            <tr key={`${sale.id}-${item.productId}`}>
                              <td>{item.productName}</td>
                              <td>{item.quantity}</td>
                              <td>{formatCurrency(item.unitPrice)}</td>
                              <td>{formatCurrency(item.lineTotal)}</td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  ))}
                </div>
              )}

              <h4 className="section-title">Balance Payments Received</h4>
              {history.payments.length === 0 ? (
                <p className="empty">No balance payments recorded yet</p>
              ) : (
                <table className="data-table">
                  <thead>
                    <tr>
                      <th>Date</th>
                      <th>Amount</th>
                      <th>Notes</th>
                    </tr>
                  </thead>
                  <tbody>
                    {history.payments.map((payment) => (
                      <tr key={payment.id}>
                        <td>{formatDate(payment.paymentDate)}</td>
                        <td>{formatCurrency(payment.amount)}</td>
                        <td>{payment.notes || '—'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </>
          ) : null}
        </section>
      </div>
    </div>
  );
}
