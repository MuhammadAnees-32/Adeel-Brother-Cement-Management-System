import { useEffect, useState } from 'react';
import { api } from '../api/client';
import { printSaleSlip, SaleSlip } from '../components/SaleSlip';
import type { AdvanceBooking, Product, Sale } from '../types/api';
import { formatCurrency, formatDate, toInputDate } from '../utils/format';

export function AdvanceBookingsPage() {
  const [bookings, setBookings] = useState<AdvanceBooking[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [lastSale, setLastSale] = useState<Sale | null>(null);
  const [form, setForm] = useState({
    customerName: '', customerMobile: '', productId: '', quantity: 1,
    unitPrice: 0, advancePaid: 0, deliveryDate: toInputDate(), notes: '',
  });

  const load = () => {
    setLoading(true);
    Promise.all([api.getBookings(), api.getProducts()])
      .then(([b, p]) => { setBookings(b); setProducts(p); })
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, []);

  const total = form.quantity * form.unitPrice;

  const submitBooking = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await api.createBooking({
        ...form,
        deliveryDate: new Date(form.deliveryDate).toISOString(),
      });
      setSuccess('Advance booking saved');
      setShowForm(false);
      load();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed');
    }
  };

  const deliver = async (booking: AdvanceBooking) => {
    const remaining = booking.remainingAmount;
    const paid = window.prompt(
      `Deliver ${booking.productName} x ${booking.quantity}?\nTotal remaining: ${formatCurrency(remaining)}\nEnter amount paid now (or leave default):`,
      String(booking.totalAmount - booking.advancePaid + booking.advancePaid),
    );
    if (paid === null) return;
    try {
      const sale = await api.deliverBooking(booking.id, Number(paid) || booking.totalAmount);
      setLastSale(sale);
      setSuccess(`Delivered — Invoice ${sale.slipNumber} created`);
      load();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Delivery failed');
    }
  };

  if (loading) return <div className="page-loading">Loading bookings...</div>;

  const pending = bookings.filter((b) => b.status === 'Pending');
  const delivered = bookings.filter((b) => b.status === 'Delivered');

  return (
    <div className="page">
      <header className="page-header page-header-row">
        <div>
          <h2>Advance Bookings</h2>
          <p>Book products before delivery — converts to sale invoice on delivery</p>
        </div>
        <button type="button" className="btn primary" onClick={() => setShowForm(!showForm)}>
          {showForm ? 'Cancel' : '+ New Booking'}
        </button>
      </header>

      {error && <div className="alert error">{error}</div>}
      {success && <div className="alert success">{success}</div>}

      {lastSale && (
        <div className="slip-card">
          <div className="slip-header">
            <h3>Invoice from Delivery</h3>
            <button type="button" className="btn" onClick={() => printSaleSlip(lastSale)}>Print</button>
          </div>
          <SaleSlip sale={lastSale} />
        </div>
      )}

      {showForm && (
        <section className="card">
          <h3>New Advance Booking</h3>
          <form onSubmit={submitBooking} className="form-grid">
            <label>Customer Name *<input value={form.customerName} onChange={(e) => setForm({ ...form, customerName: e.target.value })} required /></label>
            <label>Mobile *<input value={form.customerMobile} onChange={(e) => setForm({ ...form, customerMobile: e.target.value })} required /></label>
            <label>Product
              <select value={form.productId} onChange={(e) => {
                const p = products.find((x) => x.id === e.target.value);
                setForm({ ...form, productId: e.target.value, unitPrice: p?.salePrice ?? 0 });
              }} required>
                <option value="">Select...</option>
                {products.map((p) => <option key={p.id} value={p.id}>{p.name} (Stock: {p.stockQuantity})</option>)}
              </select>
            </label>
            <label>Quantity<input type="number" min={1} value={form.quantity} onChange={(e) => setForm({ ...form, quantity: Number(e.target.value) })} /></label>
            <label>Rate<input type="number" min={0} value={form.unitPrice} onChange={(e) => setForm({ ...form, unitPrice: Number(e.target.value) })} /></label>
            <label>Advance Paid<input type="number" min={0} max={total} value={form.advancePaid} onChange={(e) => setForm({ ...form, advancePaid: Number(e.target.value) })} /></label>
            <label>Delivery Date<input type="date" value={form.deliveryDate} onChange={(e) => setForm({ ...form, deliveryDate: e.target.value })} /></label>
            <p className="full-width">Total: {formatCurrency(total)} — Remaining: {formatCurrency(total - form.advancePaid)}</p>
            <div className="form-action"><button type="submit" className="btn primary">Save Booking</button></div>
          </form>
        </section>
      )}

      <section className="card">
        <h3>Pending Deliveries ({pending.length})</h3>
        <table className="data-table">
          <thead>
            <tr><th>Customer</th><th>Product</th><th>Qty</th><th>Advance</th><th>Remaining</th><th>Delivery</th><th></th></tr>
          </thead>
          <tbody>
            {pending.map((b) => (
              <tr key={b.id}>
                <td>{b.customerName}<br /><small>{b.customerMobile}</small></td>
                <td>{b.productName}</td>
                <td>{b.quantity}</td>
                <td>{formatCurrency(b.advancePaid)}</td>
                <td>{formatCurrency(b.remainingAmount)}</td>
                <td>{formatDate(b.deliveryDate)}</td>
                <td><button type="button" className="btn small primary" onClick={() => deliver(b)}>Deliver</button></td>
              </tr>
            ))}
            {pending.length === 0 && <tr><td colSpan={7} className="empty-state">No pending bookings</td></tr>}
          </tbody>
        </table>
      </section>

      <section className="card">
        <h3>Delivered ({delivered.length})</h3>
        <table className="data-table compact">
          <thead><tr><th>Customer</th><th>Product</th><th>Booked</th><th>Delivered</th><th>Invoice</th></tr></thead>
          <tbody>
            {delivered.map((b) => (
              <tr key={b.id}>
                <td>{b.customerName}</td>
                <td>{b.productName} x {b.quantity}</td>
                <td>{formatDate(b.bookedDate)}</td>
                <td>{formatDate(b.deliveryDate)}</td>
                <td>{b.invoiceId ? 'Created' : '—'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </div>
  );
}
