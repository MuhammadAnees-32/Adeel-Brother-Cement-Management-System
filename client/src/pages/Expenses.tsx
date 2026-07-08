import { useEffect, useState } from 'react';
import { api } from '../api/client';
import type { Expense } from '../types/api';
import { formatCurrency, formatDate, toInputDate } from '../utils/format';

const EXPENSE_CATEGORIES = [
  'Transport',
  'Labour',
  'Rent',
  'Utilities',
  'Loading',
  'Miscellaneous',
];

export function ExpensesPage() {
  const [expenses, setExpenses] = useState<Expense[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [form, setForm] = useState({
    expenseDate: toInputDate(),
    category: 'Transport',
    description: '',
    amount: 0,
  });

  const load = () => {
    api.getExpenses()
      .then(setExpenses)
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, []);

  const todayTotal = expenses
    .filter((e) => e.expenseDate.startsWith(toInputDate()))
    .reduce((sum, e) => sum + e.amount, 0);

  const monthTotal = expenses
    .filter((e) => {
      const d = new Date(e.expenseDate);
      const now = new Date();
      return d.getMonth() === now.getMonth() && d.getFullYear() === now.getFullYear();
    })
    .reduce((sum, e) => sum + e.amount, 0);

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.description.trim() || form.amount <= 0) {
      setError('Description and amount are required');
      return;
    }
    try {
      await api.createExpense({
        expenseDate: new Date(form.expenseDate).toISOString(),
        category: form.category,
        description: form.description.trim(),
        amount: form.amount,
      });
      setForm({ expenseDate: toInputDate(), category: 'Transport', description: '', amount: 0 });
      setError('');
      load();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save expense');
    }
  };

  const remove = async (id: string) => {
    if (!confirm('Delete this expense?')) return;
    await api.deleteExpense(id);
    load();
  };

  if (loading) return <div className="page-loading">Loading expenses...</div>;

  return (
    <div className="page">
      <header className="page-header">
        <h2>Daily Expenses</h2>
        <p>Track business running costs</p>
      </header>

      <div className="stat-grid">
        <div className="stat-card accent-red">
          <span className="stat-label">Today Expenses</span>
          <strong>{formatCurrency(todayTotal)}</strong>
        </div>
        <div className="stat-card">
          <span className="stat-label">This Month</span>
          <strong>{formatCurrency(monthTotal)}</strong>
        </div>
      </div>

      {error && <div className="alert error">{error}</div>}

      <div className="grid-2">
        <section className="card">
          <h3>Add Expense</h3>
          <form onSubmit={submit} className="form-grid">
            <label>
              Date
              <input type="date" value={form.expenseDate} onChange={(e) => setForm({ ...form, expenseDate: e.target.value })} />
            </label>
            <label>
              Category
              <select value={form.category} onChange={(e) => setForm({ ...form, category: e.target.value })}>
                {EXPENSE_CATEGORIES.map((c) => <option key={c} value={c}>{c}</option>)}
              </select>
            </label>
            <label className="full-width">
              Description
              <input value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} placeholder="What was this expense for?" />
            </label>
            <label>
              Amount (PKR)
              <input type="number" min={1} value={form.amount || ''} onChange={(e) => setForm({ ...form, amount: Number(e.target.value) })} />
            </label>
            <div className="form-action">
              <button className="btn primary" type="submit">Save Expense</button>
            </div>
          </form>
        </section>

        <section className="card">
          <h3>Expense History</h3>
          <table className="data-table">
            <thead>
              <tr>
                <th>Date</th>
                <th>Category</th>
                <th>Description</th>
                <th>Amount</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {expenses.length === 0 ? (
                <tr><td colSpan={5} className="empty">No expenses recorded yet</td></tr>
              ) : (
                expenses.map((expense) => (
                  <tr key={expense.id}>
                    <td>{formatDate(expense.expenseDate)}</td>
                    <td><span className="badge">{expense.category}</span></td>
                    <td>{expense.description}</td>
                    <td>{formatCurrency(expense.amount)}</td>
                    <td>
                      <button className="btn-icon" onClick={() => remove(expense.id)}>✕</button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </section>
      </div>
    </div>
  );
}
