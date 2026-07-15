import { useEffect, useState } from 'react';
import { api } from '../api/client';
import type { CreateProductRequest, CreateShopPurchaseRequest, Dealer, InventoryItem, ShopPurchase } from '../types/api';
import { formatCurrency, formatDate, toInputDate } from '../utils/format';

const CATEGORIES = ['Cement', 'Sirya', 'Taar', 'Keel'] as const;
const UNIT_OPTIONS = ['Bag', 'Kg', 'Piece'] as const;

const DEFAULT_UNITS: Record<string, string> = {
  Cement: 'Bag',
  Sirya: 'Kg',
  Taar: 'Kg',
  Keel: 'Piece',
};

const emptyForm = (): CreateProductRequest => ({
  category: 'Cement',
  name: '',
  unit: 'Bag',
  purchasePrice: 0,
  salePrice: 0,
  stockQuantity: 0,
});

const emptyPurchaseForm = (): CreateShopPurchaseRequest & { purchaseDate: string } => ({
  shopName: '',
  itemName: '',
  quantity: 1,
  unit: 'Bag',
  unitPrice: 0,
  amountPaid: 0,
  purchaseDate: toInputDate(),
  notes: '',
});

const PAYMENT_STATUS_LABEL: Record<ShopPurchase['paymentStatus'], string> = {
  Paid: 'Paid / ادا',
  Partial: 'Partial / جزوی',
  Unpaid: 'Unpaid / باقی',
};

export function InventoryPage() {
  const [items, setItems] = useState<InventoryItem[]>([]);
  const [dealers, setDealers] = useState<Dealer[]>([]);
  const [shopPurchases, setShopPurchases] = useState<ShopPurchase[]>([]);
  const [activeTab, setActiveTab] = useState<'stock' | 'khata'>('stock');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [editingId, setEditingId] = useState<string | null>(null);
  const [showAddForm, setShowAddForm] = useState(false);
  const [showPurchaseForm, setShowPurchaseForm] = useState(false);
  const [editName, setEditName] = useState('');
  const [editUnit, setEditUnit] = useState('');
  const [editDealerId, setEditDealerId] = useState('');
  const [stockQty, setStockQty] = useState(0);
  const [purchasePrice, setPurchasePrice] = useState(0);
  const [salePrice, setSalePrice] = useState(0);
  const [reason, setReason] = useState('Stock update');
  const [newItem, setNewItem] = useState<CreateProductRequest>(emptyForm());
  const [purchaseForm, setPurchaseForm] = useState(emptyPurchaseForm());

  const load = () => {
    setLoading(true);
    Promise.all([
      api.getInventory(),
      api.getDealers().catch(() => [] as Dealer[]),
      api.getShopPurchases().catch(() => [] as ShopPurchase[]),
    ])
      .then(([inventory, dealerList, purchases]) => {
        setItems(inventory);
        setDealers(dealerList);
        setShopPurchases(purchases);
      })
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, []);

  const clearMessages = () => {
    setError('');
    setSuccess('');
  };

  const startEdit = (item: InventoryItem) => {
    setEditingId(item.id);
    setEditName(item.name);
    setEditUnit(item.unit);
    setEditDealerId(item.dealerId ?? '');
    setStockQty(item.stockQuantity);
    setPurchasePrice(item.purchasePrice);
    setSalePrice(item.salePrice);
    setReason('Stock update');
    clearMessages();
  };

  const saveStock = async (id: string) => {
    if (!editName.trim()) {
      setError('Product name is required');
      return;
    }
    try {
      clearMessages();
      await api.setStock(id, stockQty, reason);
      await api.updateProduct(id, {
        name: editName.trim(),
        unit: editUnit.trim() || 'Kg',
        dealerId: editDealerId || null,
        purchasePrice,
        salePrice,
        stockQuantity: stockQty,
      });
      setSuccess('Product updated successfully');
      setEditingId(null);
      load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Update failed');
    }
  };

  const handleCategoryChange = (category: string) => {
    setNewItem({
      ...newItem,
      category,
      unit: DEFAULT_UNITS[category] ?? 'Kg',
    });
  };

  const addItem = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newItem.name.trim()) {
      setError('Product name is required');
      return;
    }
    try {
      clearMessages();
      await api.createProduct({
        ...newItem,
        name: newItem.name.trim(),
        unit: newItem.unit.trim() || DEFAULT_UNITS[newItem.category] || 'Kg',
      });
      setSuccess(`Added "${newItem.name}" successfully`);
      setNewItem(emptyForm());
      setShowAddForm(false);
      load();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to add item');
    }
  };

  const removeItem = async (item: InventoryItem) => {
    if (!confirm(`Remove "${item.name}" from inventory?`)) return;
    try {
      clearMessages();
      await api.deleteProduct(item.id);
      setSuccess(`Removed "${item.name}"`);
      if (editingId === item.id) setEditingId(null);
      load();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to remove item');
    }
  };

  const purchaseTotal = purchaseForm.quantity * purchaseForm.unitPrice;

  const addShopPurchase = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!purchaseForm.shopName.trim() || !purchaseForm.itemName.trim()) {
      setError('Shop name and item name are required');
      return;
    }
    if (purchaseForm.quantity <= 0 || purchaseForm.unitPrice < 0) {
      setError('Valid quantity and rate are required');
      return;
    }
    try {
      clearMessages();
      await api.createShopPurchase({
        shopName: purchaseForm.shopName.trim(),
        itemName: purchaseForm.itemName.trim(),
        quantity: purchaseForm.quantity,
        unit: purchaseForm.unit,
        unitPrice: purchaseForm.unitPrice,
        amountPaid: purchaseForm.amountPaid ?? 0,
        purchaseDate: new Date(purchaseForm.purchaseDate).toISOString(),
        notes: purchaseForm.notes?.trim() || undefined,
      });
      setSuccess('Purchase recorded in khata');
      setPurchaseForm(emptyPurchaseForm());
      setShowPurchaseForm(false);
      load();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to record purchase');
    }
  };

  const removeShopPurchase = async (purchase: ShopPurchase) => {
    if (!confirm(`Remove purchase from "${purchase.shopName}"?`)) return;
    try {
      clearMessages();
      await api.deleteShopPurchase(purchase.id);
      setSuccess('Purchase entry removed');
      load();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to remove purchase');
    }
  };

  const totalOutstanding = shopPurchases.reduce((sum, p) => sum + p.balanceDue, 0);

  const categories = [...new Set([...CATEGORIES, ...items.map((i) => i.category)])];

  if (loading) return <div className="page-loading">Loading inventory...</div>;

  return (
    <div className="page">
      <header className="page-header">
        <div className="page-header-row">
          <div>
            <h2>Inventory Management</h2>
            <p>Stock levels & purchase khata / اسٹاک اور خریداری کھاتہ</p>
          </div>
          {activeTab === 'stock' && (
            <button
              className="btn primary"
              onClick={() => { setShowAddForm(!showAddForm); clearMessages(); }}
            >
              {showAddForm ? 'Cancel' : '+ Add Item'}
            </button>
          )}
          {activeTab === 'khata' && (
            <button
              className="btn primary"
              onClick={() => { setShowPurchaseForm(!showPurchaseForm); clearMessages(); }}
            >
              {showPurchaseForm ? 'Cancel' : '+ Record Purchase'}
            </button>
          )}
        </div>
      </header>

      <div className="tab-bar">
        <button
          className={`tab-btn ${activeTab === 'stock' ? 'active' : ''}`}
          type="button"
          onClick={() => { setActiveTab('stock'); clearMessages(); }}
        >
          Stock / اسٹاک
        </button>
        <button
          className={`tab-btn ${activeTab === 'khata' ? 'active' : ''}`}
          type="button"
          onClick={() => { setActiveTab('khata'); clearMessages(); }}
        >
          Purchase Khata / خریداری کھاتہ
        </button>
      </div>

      {error && <div className="alert error">{error}</div>}
      {success && <div className="alert success">{success}</div>}

      {activeTab === 'stock' && showAddForm && (
        <section className="card">
          <h3>Add New Item</h3>
          <form onSubmit={addItem} className="form-grid">
            <label>
              Category
              <select value={newItem.category} onChange={(e) => handleCategoryChange(e.target.value)}>
                {CATEGORIES.map((c) => <option key={c} value={c}>{c}</option>)}
              </select>
            </label>
            <label>
              Product Name *
              <input
                value={newItem.name}
                onChange={(e) => setNewItem({ ...newItem, name: e.target.value })}
                placeholder="e.g. Maple Leaf, 7mm Ring"
              />
            </label>
            <label>
              Unit
              <input
                list="unit-options"
                value={newItem.unit}
                onChange={(e) => setNewItem({ ...newItem, unit: e.target.value })}
                placeholder="Bag, Kg, Piece"
              />
            </label>
            <label>
              Dealer
              <select
                value={newItem.dealerId ?? ''}
                onChange={(e) => setNewItem({ ...newItem, dealerId: e.target.value || undefined })}
              >
                <option value="">— No dealer —</option>
                {dealers.map((d) => (
                  <option key={d.id} value={d.id}>{d.name}</option>
                ))}
              </select>
            </label>
            <label>
              Opening Stock
              <input
                type="number"
                min={0}
                value={newItem.stockQuantity || ''}
                onChange={(e) => setNewItem({ ...newItem, stockQuantity: Number(e.target.value) })}
              />
            </label>
            <label>
              Buy Price (PKR)
              <input
                type="number"
                min={0}
                value={newItem.purchasePrice || ''}
                onChange={(e) => setNewItem({ ...newItem, purchasePrice: Number(e.target.value) })}
              />
            </label>
            <label>
              Sale Price (PKR)
              <input
                type="number"
                min={0}
                value={newItem.salePrice || ''}
                onChange={(e) => setNewItem({ ...newItem, salePrice: Number(e.target.value) })}
              />
            </label>
            <div className="form-action">
              <button className="btn primary" type="submit">Add Item</button>
            </div>
          </form>
          <datalist id="unit-options">
            {UNIT_OPTIONS.map((u) => <option key={u} value={u} />)}
          </datalist>
        </section>
      )}

      {activeTab === 'khata' && showPurchaseForm && (
        <section className="card">
          <h3>Record Shop Purchase / دکان سے خریداری</h3>
          <form onSubmit={addShopPurchase} className="form-grid">
            <label>
              Date
              <input
                type="date"
                value={purchaseForm.purchaseDate}
                onChange={(e) => setPurchaseForm({ ...purchaseForm, purchaseDate: e.target.value })}
              />
            </label>
            <label>
              Shop / Supplier Name *
              <input
                value={purchaseForm.shopName}
                onChange={(e) => setPurchaseForm({ ...purchaseForm, shopName: e.target.value })}
                placeholder="e.g. Ali Cement Store"
              />
            </label>
            <label>
              Item Name *
              <input
                value={purchaseForm.itemName}
                onChange={(e) => setPurchaseForm({ ...purchaseForm, itemName: e.target.value })}
                placeholder="e.g. Maple Leaf Cement"
              />
            </label>
            <label>
              Quantity
              <input
                type="number"
                min={0.01}
                step="any"
                value={purchaseForm.quantity || ''}
                onChange={(e) => setPurchaseForm({ ...purchaseForm, quantity: Number(e.target.value) })}
              />
            </label>
            <label>
              Unit
              <input
                list="unit-options"
                value={purchaseForm.unit ?? ''}
                onChange={(e) => setPurchaseForm({ ...purchaseForm, unit: e.target.value })}
                placeholder="Bag, Kg, Piece"
              />
            </label>
            <label>
              Rate (PKR)
              <input
                type="number"
                min={0}
                value={purchaseForm.unitPrice || ''}
                onChange={(e) => setPurchaseForm({ ...purchaseForm, unitPrice: Number(e.target.value) })}
              />
            </label>
            <label>
              Amount Paid (PKR)
              <input
                type="number"
                min={0}
                max={purchaseTotal}
                value={purchaseForm.amountPaid ?? ''}
                onChange={(e) => setPurchaseForm({ ...purchaseForm, amountPaid: Number(e.target.value) })}
              />
            </label>
            <label className="full-width">
              Notes
              <input
                value={purchaseForm.notes ?? ''}
                onChange={(e) => setPurchaseForm({ ...purchaseForm, notes: e.target.value })}
                placeholder="Optional remarks"
              />
            </label>
            <p className="full-width purchase-total-hint">
              Total: <strong>{formatCurrency(purchaseTotal)}</strong>
              {purchaseTotal > (purchaseForm.amountPaid ?? 0) && (
                <> — Balance: <strong>{formatCurrency(purchaseTotal - (purchaseForm.amountPaid ?? 0))}</strong></>
              )}
            </p>
            <div className="form-action">
              <button className="btn primary" type="submit">Save to Khata</button>
            </div>
          </form>
        </section>
      )}

      {activeTab === 'stock' && categories.map((category) => {
        const categoryItems = items.filter((i) => i.category === category);
        if (categoryItems.length === 0) return null;

        return (
          <section className="card" key={category}>
            <h3>{category}</h3>
            <table className="data-table">
              <thead>
                <tr>
                  <th>Product</th>
                  <th>Dealer</th>
                  <th>Stock</th>
                  <th>Unit</th>
                  <th>Buy Price</th>
                  <th>Sale Price</th>
                  <th>Purchased</th>
                  <th>Sold</th>
                  <th>Stock Value</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {categoryItems.map((item) => (
                  <tr key={item.id} className={item.stockQuantity <= 10 ? 'low-stock' : ''}>
                    {editingId === item.id ? (
                      <>
                        <td>
                          <input
                            className="input-sm input-name"
                            value={editName}
                            onChange={(e) => setEditName(e.target.value)}
                          />
                        </td>
                        <td>
                          <select
                            className="input-sm input-dealer"
                            value={editDealerId}
                            onChange={(e) => setEditDealerId(e.target.value)}
                          >
                            <option value="">— No dealer —</option>
                            {dealers.map((d) => (
                              <option key={d.id} value={d.id}>{d.name}</option>
                            ))}
                          </select>
                        </td>
                        <td>
                          <input
                            type="number"
                            className="input-sm"
                            value={stockQty}
                            onChange={(e) => setStockQty(Number(e.target.value))}
                          />
                        </td>
                        <td>
                          <input
                            list="edit-unit-options"
                            className="input-sm"
                            value={editUnit}
                            onChange={(e) => setEditUnit(e.target.value)}
                          />
                        </td>
                        <td>
                          <input
                            type="number"
                            className="input-sm"
                            value={purchasePrice}
                            onChange={(e) => setPurchasePrice(Number(e.target.value))}
                          />
                        </td>
                        <td>
                          <input
                            type="number"
                            className="input-sm"
                            value={salePrice}
                            onChange={(e) => setSalePrice(Number(e.target.value))}
                          />
                        </td>
                        <td>{item.totalPurchased}</td>
                        <td>{item.totalSold}</td>
                        <td>{formatCurrency(stockQty * purchasePrice)}</td>
                        <td className="actions">
                          <button className="btn small" type="button" onClick={() => saveStock(item.id)}>Save</button>
                          <button className="btn small secondary" type="button" onClick={() => setEditingId(null)}>Cancel</button>
                        </td>
                      </>
                    ) : (
                      <>
                        <td>{item.name}</td>
                        <td>{item.dealerName || '—'}</td>
                        <td><strong>{item.stockQuantity}</strong></td>
                        <td>{item.unit}</td>
                        <td>{formatCurrency(item.purchasePrice)}</td>
                        <td>{formatCurrency(item.salePrice)}</td>
                        <td>{item.totalPurchased}</td>
                        <td>{item.totalSold}</td>
                        <td>{formatCurrency(item.stockValue)}</td>
                        <td className="actions">
                          <button className="btn small" type="button" onClick={() => startEdit(item)}>Edit</button>
                          <button className="btn small danger" type="button" onClick={() => removeItem(item)}>Remove</button>
                        </td>
                      </>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          </section>
        );
      })}

      {activeTab === 'khata' && (
        <section className="card">
          <div className="page-header-row">
            <h3>Purchase Khata / خریداری کھاتہ</h3>
            {totalOutstanding > 0 && (
              <span className="balance-due">
                Total outstanding: <strong>{formatCurrency(totalOutstanding)}</strong>
              </span>
            )}
          </div>
          <p className="section-desc">Track items purchased from other shops — separate from customer khata.</p>
          {shopPurchases.length === 0 ? (
            <p className="empty-state">No purchases recorded yet</p>
          ) : (
            <table className="data-table">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Shop</th>
                  <th>Item</th>
                  <th>Qty</th>
                  <th>Rate</th>
                  <th>Total</th>
                  <th>Paid</th>
                  <th>Balance</th>
                  <th>Status</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {shopPurchases.map((p) => (
                  <tr key={p.id}>
                    <td>{formatDate(p.purchaseDate)}</td>
                    <td>{p.shopName}</td>
                    <td>{p.itemName}</td>
                    <td>{p.quantity} {p.unit}</td>
                    <td>{formatCurrency(p.unitPrice)}</td>
                    <td>{formatCurrency(p.totalAmount)}</td>
                    <td>{formatCurrency(p.amountPaid)}</td>
                    <td>{p.balanceDue > 0 ? formatCurrency(p.balanceDue) : '—'}</td>
                    <td>
                      <span className={`status-badge status-${p.paymentStatus.toLowerCase()}`}>
                        {PAYMENT_STATUS_LABEL[p.paymentStatus]}
                      </span>
                    </td>
                    <td>
                      <button className="btn small danger" type="button" onClick={() => removeShopPurchase(p)}>Remove</button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </section>
      )}

      <datalist id="edit-unit-options">
        {UNIT_OPTIONS.map((u) => <option key={u} value={u} />)}
      </datalist>
    </div>
  );
}
