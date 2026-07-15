import { useEffect, useState } from 'react';
import { api } from '../api/client';
import type { Dealer, DealerHistory, Product } from '../types/api';
import { formatCurrency, formatDate, toInputDate } from '../utils/format';

export function DealersPage() {
  const [dealers, setDealers] = useState<Dealer[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [selected, setSelected] = useState<Dealer | null>(null);
  const [history, setHistory] = useState<DealerHistory | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [showAddDealer, setShowAddDealer] = useState(false);
  const [showPurchase, setShowPurchase] = useState(false);
  const [showPayment, setShowPayment] = useState(false);
  const [dealerForm, setDealerForm] = useState({ name: '', phone: '', address: '' });
  const [purchaseForm, setPurchaseForm] = useState({
    productId: '', quantity: 1, unitPrice: 0, amountPaid: '' as number | '', purchaseDate: toInputDate(), notes: '',
  });
  const [paymentAmount, setPaymentAmount] = useState<number | ''>('');

  const load = () => {
    setLoading(true);
    Promise.all([api.getDealers(), api.getProducts()])
      .then(([d, p]) => { setDealers(d); setProducts(p); })
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, []);

  const selectDealer = async (dealer: Dealer) => {
    setSelected(dealer);
    setError('');
    try {
      setHistory(await api.getDealerHistory(dealer.id));
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load history');
    }
  };

  const addDealer = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await api.createDealer(dealerForm);
      setSuccess('Dealer added');
      setShowAddDealer(false);
      setDealerForm({ name: '', phone: '', address: '' });
      load();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed');
    }
  };

  const submitPurchase = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selected) return;
    try {
      const paid = purchaseForm.amountPaid === '' ? undefined : purchaseForm.amountPaid;
      await api.recordDealerPurchase({
        dealerId: selected.id,
        productId: purchaseForm.productId,
        quantity: purchaseForm.quantity,
        unitPrice: purchaseForm.unitPrice,
        amountPaid: paid,
        purchaseDate: new Date(purchaseForm.purchaseDate).toISOString(),
        notes: purchaseForm.notes || undefined,
      });
      setSuccess('Purchase recorded — stock increased automatically');
      setShowPurchase(false);
      load();
      await selectDealer(selected);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed');
    }
  };

  const submitPayment = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selected || paymentAmount === '' || paymentAmount <= 0) return;
    try {
      await api.recordDealerPayment(selected.id, { amount: paymentAmount });
      setSuccess('Payment recorded');
      setShowPayment(false);
      setPaymentAmount('');
      load();
      await selectDealer(selected);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed');
    }
  };

  if (loading) return <div className="page-loading">Loading dealers...</div>;

  return (
    <div className="page">
      <header className="page-header page-header-row">
        <div>
          <h2>Dealer Management</h2>
          <p>Unique dealer profiles, purchases & outstanding balances</p>
        </div>
        <button type="button" className="btn primary" onClick={() => setShowAddDealer(!showAddDealer)}>
          {showAddDealer ? 'Cancel' : '+ Add Dealer'}
        </button>
      </header>

      {error && <div className="alert error">{error}</div>}
      {success && <div className="alert success">{success}</div>}

      {showAddDealer && (
        <section className="card">
          <h3>New Dealer</h3>
          <form onSubmit={addDealer} className="form-grid">
            <label>Name *<input value={dealerForm.name} onChange={(e) => setDealerForm({ ...dealerForm, name: e.target.value })} required /></label>
            <label>Phone<input value={dealerForm.phone} onChange={(e) => setDealerForm({ ...dealerForm, phone: e.target.value })} /></label>
            <label className="full-width">Address<input value={dealerForm.address} onChange={(e) => setDealerForm({ ...dealerForm, address: e.target.value })} /></label>
            <div className="form-action"><button type="submit" className="btn primary">Save Dealer</button></div>
          </form>
        </section>
      )}

      <div className="grid-2">
        <section className="card">
          <h3>Dealers ({dealers.length})</h3>
          <table className="data-table">
            <thead><tr><th>Name</th><th>Outstanding</th><th></th></tr></thead>
            <tbody>
              {dealers.map((d) => (
                <tr key={d.id} className={selected?.id === d.id ? 'selected-row' : ''}>
                  <td>{d.name}</td>
                  <td className={d.outstandingBalance > 0 ? 'text-red' : ''}>{formatCurrency(d.outstandingBalance)}</td>
                  <td><button type="button" className="btn small" onClick={() => selectDealer(d)}>Open</button></td>
                </tr>
              ))}
            </tbody>
          </table>
        </section>

        {selected && history && (
          <section className="card">
            <div className="page-header-row">
              <h3>{selected.name}</h3>
              <div className="slip-actions">
                <button type="button" className="btn" onClick={() => setShowPurchase(true)}>Record Purchase</button>
                <button type="button" className="btn" onClick={() => setShowPayment(true)}>Pay Dealer</button>
              </div>
            </div>
            <p>Outstanding: <strong>{formatCurrency(history.dealer.outstandingBalance)}</strong></p>

            {showPurchase && (
              <form onSubmit={submitPurchase} className="form-grid nested-form">
                <label>Product
                  <select value={purchaseForm.productId} onChange={(e) => {
                    const p = products.find((x) => x.id === e.target.value);
                    setPurchaseForm({ ...purchaseForm, productId: e.target.value, unitPrice: p?.purchasePrice ?? 0 });
                  }} required>
                    <option value="">Select...</option>
                    {products.map((p) => <option key={p.id} value={p.id}>{p.name}</option>)}
                  </select>
                </label>
                <label>Quantity<input type="number" min={1} value={purchaseForm.quantity} onChange={(e) => setPurchaseForm({ ...purchaseForm, quantity: Number(e.target.value) })} /></label>
                <label>Unit Price<input type="number" min={0} value={purchaseForm.unitPrice} onChange={(e) => setPurchaseForm({ ...purchaseForm, unitPrice: Number(e.target.value) })} /></label>
                <label>Amount Paid<input type="number" min={0} value={purchaseForm.amountPaid} onChange={(e) => setPurchaseForm({ ...purchaseForm, amountPaid: e.target.value === '' ? '' : Number(e.target.value) })} placeholder="Leave empty for full payment" /></label>
                <label>Date<input type="date" value={purchaseForm.purchaseDate} onChange={(e) => setPurchaseForm({ ...purchaseForm, purchaseDate: e.target.value })} /></label>
                <div className="form-action"><button type="submit" className="btn primary">Save Purchase</button></div>
              </form>
            )}

            {showPayment && (
              <form onSubmit={submitPayment} className="form-grid nested-form">
                <label>Payment Amount<input type="number" min={1} max={selected.outstandingBalance} value={paymentAmount} onChange={(e) => setPaymentAmount(e.target.value === '' ? '' : Number(e.target.value))} required /></label>
                <div className="form-action"><button type="submit" className="btn primary">Record Payment</button></div>
              </form>
            )}

            <h4 className="section-title">Purchase History</h4>
            <table className="data-table compact">
              <thead><tr><th>Date</th><th>Product</th><th>Qty</th><th>Total</th><th>Paid</th><th>Due</th></tr></thead>
              <tbody>
                {history.purchases.map((p) => (
                  <tr key={p.id}>
                    <td>{formatDate(p.purchaseDate)}</td>
                    <td>{p.productName}</td>
                    <td>{p.quantity}</td>
                    <td>{formatCurrency(p.totalAmount)}</td>
                    <td>{formatCurrency(p.amountPaid)}</td>
                    <td>{formatCurrency(p.balanceDue)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </section>
        )}
      </div>
    </div>
  );
}
