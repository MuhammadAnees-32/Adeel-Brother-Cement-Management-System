import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../api/client';
import { printKhataStatement } from '../components/KhataPrint';
import type { Customer, KhataBook } from '../types/api';
import { formatCurrency, formatDateTime } from '../utils/format';

export function KhataBookPage() {
  const [search, setSearch] = useState('');
  const [results, setResults] = useState<Customer[]>([]);
  const [khata, setKhata] = useState<KhataBook | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!search.trim()) {
      setResults([]);
      return;
    }
    const timer = setTimeout(() => {
      api.searchCustomers(search)
        .then(setResults)
        .catch((e) => setError(e.message));
    }, 300);
    return () => clearTimeout(timer);
  }, [search]);

  const openKhata = async (customer: Customer) => {
    setLoading(true);
    setError('');
    try {
      const data = await api.getKhataBook(customer.id);
      setKhata(data);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load Khata');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page">
      <header className="page-header">
        <h2>Khata Book</h2>
        <p>Search customer by name or mobile — one permanent ledger per customer</p>
      </header>

      {error && <div className="alert error">{error}</div>}

      <section className="card">
        <label>
          Search Customer
          <input
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Name or mobile number..."
          />
        </label>
        {results.length > 0 && (
          <ul className="search-results">
            {results.map((c) => (
              <li key={c.id}>
                <button type="button" className="search-result-btn" onClick={() => openKhata(c)}>
                  <strong>{c.name}</strong>
                  <span>{c.phone || '—'}</span>
                  <span className={c.balance > 0 ? 'text-red' : ''}>
                    Balance: {formatCurrency(c.balance)}
                  </span>
                </button>
              </li>
            ))}
          </ul>
        )}
      </section>

      {loading && <div className="page-loading">Loading Khata Book...</div>}

      {khata && !loading && (
        <section className="card">
          <div className="page-header-row">
            <div>
              <h3>{khata.customer.name}</h3>
              <p>{khata.customer.phone} — Current Balance: <strong>{formatCurrency(khata.currentBalance)}</strong></p>
            </div>
            <div className="slip-actions">
              <Link
                to="/sale"
                state={{ customerName: khata.customer.name, customerMobile: khata.customer.phone, customerId: khata.customer.id }}
                className="btn primary"
              >
                Add More Item
              </Link>
              <button type="button" className="btn" onClick={() => printKhataStatement(khata)}>
                Print Customer Balance
              </button>
            </div>
          </div>

          {khata.entries.length === 0 ? (
            <p className="empty-state">No transactions yet. Use &quot;Add More Item&quot; to record first purchase.</p>
          ) : (
            <table className="data-table">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Type</th>
                  <th>Description</th>
                  <th>Previous Balance</th>
                  <th>New Purchase</th>
                  <th>Payment Received</th>
                  <th>Remaining Balance</th>
                </tr>
              </thead>
              <tbody>
                {khata.entries.map((e, i) => (
                  <tr key={`${e.reference}-${i}`}>
                    <td>{formatDateTime(e.date)}</td>
                    <td>{e.type}</td>
                    <td>{e.description}</td>
                    <td>{formatCurrency(e.previousBalance)}</td>
                    <td>{e.purchaseAmount > 0 ? formatCurrency(e.purchaseAmount) : '—'}</td>
                    <td>{e.paymentReceived > 0 ? formatCurrency(e.paymentReceived) : '—'}</td>
                    <td><strong>{formatCurrency(e.remainingBalance)}</strong></td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </section>
      )}
    </div>
  );
}
