import { useEffect, useMemo, useState } from 'react';
import { useLocation } from 'react-router-dom';
import { api } from '../api/client';
import { printSaleSlip, SaleSlip } from '../components/SaleSlip';
import { SearchableSelect } from '../components/SearchableSelect';
import type { Product, Sale } from '../types/api';
import { formatCurrency, toInputDate } from '../utils/format';

interface CartItem {
  productId: string;
  productName: string;
  unit: string;
  quantity: number;
  unitPrice: number;
  stock: number;
}

export function NewSalePage() {
  const location = useLocation();
  const prefill = (location.state as { customerName?: string; customerMobile?: string; customerId?: string }) ?? {};

  const [products, setProducts] = useState<Product[]>([]);
  const [customerName, setCustomerName] = useState(prefill.customerName ?? '');
  const [customerMobile, setCustomerMobile] = useState(prefill.customerMobile ?? '');
  const [existingCustomerMsg, setExistingCustomerMsg] = useState('');
  const [previousBalance, setPreviousBalance] = useState(0);
  const [amountPaid, setAmountPaid] = useState<number | ''>('');
  const [notes, setNotes] = useState('');
  const [saleDate, setSaleDate] = useState(toInputDate());
  const [cart, setCart] = useState<CartItem[]>([]);
  const [selectedProduct, setSelectedProduct] = useState('');
  const [quantity, setQuantity] = useState(1);
  const [unitPrice, setUnitPrice] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [lastSale, setLastSale] = useState<Sale | null>(null);

  useEffect(() => {
    api.getProducts().then(setProducts).catch((e) => setError(e.message));
  }, []);

  useEffect(() => {
    if (prefill.customerName && prefill.customerMobile) {
      lookupCustomer(prefill.customerName, prefill.customerMobile);
    }
  }, []);

  const lookupCustomer = async (name: string, mobile: string) => {
    if (!name.trim() || !mobile.trim()) {
      setExistingCustomerMsg('');
      setPreviousBalance(0);
      return;
    }
    try {
      const result = await api.lookupCustomer(name.trim(), mobile.trim());
      setExistingCustomerMsg(result.message);
      setPreviousBalance(result.customer?.balance ?? 0);
    } catch {
      setExistingCustomerMsg('');
    }
  };

  const onCustomerBlur = () => {
    lookupCustomer(customerName, customerMobile);
  };

  const selectedProductData = products.find((p) => p.id === selectedProduct);

  useEffect(() => {
    if (selectedProductData) setUnitPrice(selectedProductData.salePrice);
  }, [selectedProduct, selectedProductData?.salePrice]);

  const addToCart = () => {
    const product = selectedProductData;
    if (!product) return;
    if (quantity <= 0) {
      setError('Quantity must be greater than 0');
      return;
    }
    if (unitPrice < 0) {
      setError('Sale rate cannot be negative');
      return;
    }

    const existing = cart.find((c) => c.productId === product.id);
    const totalQty = (existing?.quantity ?? 0) + quantity;
    if (totalQty > product.stockQuantity) {
      setError(`Only ${product.stockQuantity} ${product.unit} available for ${product.name}`);
      return;
    }

    setError('');
    if (existing) {
      setCart(cart.map((c) =>
        c.productId === product.id
          ? { ...c, quantity: totalQty, unitPrice }
          : c
      ));
    } else {
      setCart([...cart, {
        productId: product.id,
        productName: product.name,
        unit: product.unit,
        quantity,
        unitPrice,
        stock: product.stockQuantity,
      }]);
    }
    setQuantity(1);
    setUnitPrice(product.salePrice);
  };

  const updateCartPrice = (productId: string, price: number) => {
    setCart(cart.map((c) =>
      c.productId === productId ? { ...c, unitPrice: Math.max(0, price) } : c
    ));
  };

  const removeFromCart = (productId: string) => {
    setCart(cart.filter((c) => c.productId !== productId));
  };

  const total = cart.reduce((sum, item) => sum + item.quantity * item.unitPrice, 0);
  const paid = amountPaid === '' ? total : amountPaid;
  const balance = Math.max(0, total - paid);

  useEffect(() => {
    if (cart.length === 0) {
      setAmountPaid('');
    } else if (amountPaid === '') {
      setAmountPaid(total);
    } else if (typeof amountPaid === 'number' && amountPaid > total) {
      setAmountPaid(total);
    }
  }, [total, cart.length]);

  const submitSale = async () => {
    if (!customerName.trim()) {
      setError('Customer name is required');
      return;
    }
    if (!customerMobile.trim()) {
      setError('Mobile number is required');
      return;
    }
    if (cart.length === 0) {
      setError('Add at least one item');
      return;
    }
    if (cart.some((c) => c.unitPrice < 0)) {
      setError('Sale rate cannot be negative');
      return;
    }
    if (paid < 0 || paid > total) {
      setError('Amount paid must be between 0 and bill total');
      return;
    }

    setLoading(true);
    setError('');
    try {
      const sale = await api.createSale({
        customerName: customerName.trim(),
        customerMobile: customerMobile.trim(),
        amountPaid: paid,
        transactionDate: new Date(saleDate).toISOString(),
        notes: notes || undefined,
        items: cart.map((c) => ({
          productId: c.productId,
          quantity: c.quantity,
          unitPrice: c.unitPrice,
        })),
      });
      setLastSale(sale);
      setCart([]);
      setCustomerName('');
      setCustomerMobile('');
      setAmountPaid('');
      setNotes('');
      const updated = await api.getProducts();
      setProducts(updated);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to create sale');
    } finally {
      setLoading(false);
    }
  };

  const productOptions = useMemo(
    () =>
      products.map((p) => ({
        value: p.id,
        group: p.category,
        label: `${p.name} — Stock: ${p.stockQuantity} ${p.unit} @ ${formatCurrency(p.salePrice)}`,
        searchText: `${p.name} ${p.category} ${p.unit}`,
      })),
    [products],
  );

  return (
    <div className="page">
      <header className="page-header">
        <h2>New Sale</h2>
        <p>Create customer slip & update inventory</p>
      </header>

      {error && <div className="alert error">{error}</div>}

      {lastSale && (
        <div className="slip-card">
          <div className="slip-header">
            <div>
              <h3>Sale Slip Generated</h3>
            </div>
            <div className="slip-actions">
              <button className="btn" onClick={() => printSaleSlip(lastSale)} type="button">
                Print Slip
              </button>
              <button className="btn secondary" onClick={() => setLastSale(null)} type="button">
                Dismiss
              </button>
            </div>
          </div>
          <SaleSlip sale={lastSale} />
        </div>
      )}

      <div className="grid-2">
        <section className="card">
          <h3>Customer Details</h3>
          <div className="form-grid">
            <label>
              Customer Name *
              <input value={customerName} onChange={(e) => setCustomerName(e.target.value)} onBlur={onCustomerBlur} placeholder="Enter customer name" />
            </label>
            <label>
              Mobile Number *
              <input
                type="tel"
                value={customerMobile}
                onChange={(e) => setCustomerMobile(e.target.value)}
                onBlur={onCustomerBlur}
                placeholder="03XX XXXXXXX"
                required
              />
            </label>
            {existingCustomerMsg && (
              <p className={`full-width customer-lookup-msg ${previousBalance > 0 ? 'warning' : 'info'}`}>
                {existingCustomerMsg}
                {previousBalance > 0 && <> Previous balance: <strong>{formatCurrency(previousBalance)}</strong></>}
              </p>
            )}
            <label>
              Date
              <input type="date" value={saleDate} onChange={(e) => setSaleDate(e.target.value)} />
            </label>
            <label className="full-width">
              Notes
              <input value={notes} onChange={(e) => setNotes(e.target.value)} placeholder="Optional notes" />
            </label>
          </div>

          <h3 className="section-title">Add Items</h3>
          <div className="form-grid">
            <label className="full-width">
              Product
              <SearchableSelect
                options={productOptions}
                value={selectedProduct}
                onChange={setSelectedProduct}
                placeholder="Search or select product..."
              />
            </label>
            <label>
              Quantity
              <input type="number" min={1} value={quantity} onChange={(e) => setQuantity(Number(e.target.value))} />
            </label>
            <label>
              Sale Rate (PKR)
              <input
                type="number"
                min={0}
                value={unitPrice || ''}
                onChange={(e) => setUnitPrice(Number(e.target.value))}
              />
            </label>
            {selectedProductData && unitPrice !== selectedProductData.salePrice && (
              <p className="price-hint">
                List price: {formatCurrency(selectedProductData.salePrice)}
              </p>
            )}
            <div className="form-action">
              <button className="btn" onClick={addToCart} type="button">Add to Cart</button>
            </div>
          </div>
        </section>

        <section className="card">
          <h3>Cart ({cart.length} items)</h3>
          {cart.length === 0 ? (
            <p className="empty-state">No items added yet</p>
          ) : (
            <>
              <table className="data-table">
                <thead>
                  <tr><th>Product</th><th>Qty</th><th>Rate</th><th>Total</th><th></th></tr>
                </thead>
                <tbody>
                  {cart.map((item) => (
                    <tr key={item.productId}>
                      <td>{item.productName}</td>
                      <td>{item.quantity} {item.unit}</td>
                      <td>
                        <input
                          type="number"
                          className="input-sm input-rate"
                          min={0}
                          value={item.unitPrice || ''}
                          onChange={(e) => updateCartPrice(item.productId, Number(e.target.value))}
                        />
                      </td>
                      <td>{formatCurrency(item.quantity * item.unitPrice)}</td>
                      <td>
                        <button className="btn-icon" onClick={() => removeFromCart(item.productId)}>✕</button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
              <div className="cart-footer payment-section">
                <div className="payment-fields">
                  <label>
                    Amount Paid
                    <input
                      type="number"
                      min={0}
                      max={total}
                      value={amountPaid === '' ? '' : amountPaid}
                      onChange={(e) => setAmountPaid(e.target.value === '' ? '' : Number(e.target.value))}
                    />
                  </label>
                  {balance > 0 && (
                    <span className="balance-due">
                      This bill balance: <strong>{formatCurrency(balance)}</strong>
                    </span>
                  )}
                  {previousBalance > 0 && (
                    <span className="balance-due">
                      Previous balance: <strong>{formatCurrency(previousBalance)}</strong>
                    </span>
                  )}
                  {(previousBalance + balance) > 0 && (
                    <span className="balance-due total-balance">
                      Total remaining: <strong>{formatCurrency(previousBalance + balance)}</strong>
                    </span>
                  )}
                </div>
                <div className="cart-footer-actions">
                  <strong>Grand Total: {formatCurrency(total)}</strong>
                  <button className="btn primary" onClick={submitSale} disabled={loading}>
                    {loading ? 'Saving...' : 'Generate Slip & Save'}
                  </button>
                </div>
              </div>
            </>
          )}
        </section>
      </div>
    </div>
  );
}
