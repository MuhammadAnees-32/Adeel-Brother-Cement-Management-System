import { useEffect, useState, type FormEvent } from 'react';
import { api } from '@/api/client';
import type { CreateUserRequest, ScreenInfo, UserAccount } from '@/types/api';
import type { AppScreen } from './AuthContext';

const salesmanDefaults: AppScreen[] = ['NewSale', 'CustomerBalance', 'Inventory', 'Expenses'];

const emptyForm = {
  username: '',
  password: '',
  role: 'Salesman' as 'Admin' | 'Salesman',
  allowedScreens: [...salesmanDefaults],
  isActive: true,
};

export function UserManagementPage() {
  const [users, setUsers] = useState<UserAccount[]>([]);
  const [screens, setScreens] = useState<ScreenInfo[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [form, setForm] = useState(emptyForm);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  async function loadData() {
    setLoading(true);
    setError('');
    try {
      const [userList, screenList] = await Promise.all([
        api.getUsers(),
        api.getScreens(),
      ]);
      setUsers(userList);
      setScreens(screenList);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load users');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadData();
  }, []);

  function resetForm() {
    setForm(emptyForm);
    setEditingId(null);
  }

  function startEdit(user: UserAccount) {
    setEditingId(user.id);
    setForm({
      username: user.username,
      password: '',
      role: user.role,
      allowedScreens: user.allowedScreens as AppScreen[],
      isActive: user.isActive,
    });
  }

  function toggleScreen(screen: string) {
    setForm((prev) => {
      const current = new Set(prev.allowedScreens);
      if (current.has(screen as AppScreen)) current.delete(screen as AppScreen);
      else current.add(screen as AppScreen);
      return { ...prev, allowedScreens: Array.from(current) as AppScreen[] };
    });
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setSaving(true);
    setError('');
    try {
      if (editingId) {
        await api.updateUser(editingId, {
          password: form.password || undefined,
          role: form.role,
          allowedScreens: form.role === 'Admin' ? [] : form.allowedScreens,
          isActive: form.isActive,
        });
      } else {
        const payload: CreateUserRequest = {
          username: form.username,
          password: form.password,
          role: form.role,
          allowedScreens: form.role === 'Admin' ? [] : form.allowedScreens,
        };
        await api.createUser(payload);
      }
      resetForm();
      await loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Save failed');
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(id: string, username: string) {
    if (!confirm(`Delete user "${username}"?`)) return;
    try {
      await api.deleteUser(id);
      if (editingId === id) resetForm();
      await loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Delete failed');
    }
  }

  const assignableScreens = screens.filter((s) => s.key !== 'UserManagement');

  if (loading) return <div className="page-loading">Loading users...</div>;

  return (
    <div className="page">
      <header className="page-header">
        <h2>User Access</h2>
        <p>Create users and assign screen permissions</p>
      </header>

      {error && <div className="page-error">{error}</div>}

      <div className="two-col">
        <section className="card">
          <h3>{editingId ? 'Edit User' : 'Add User'}</h3>
          <form className="form-grid" onSubmit={handleSubmit}>
            <label>
              Username
              <input
                value={form.username}
                onChange={(e) => setForm({ ...form, username: e.target.value })}
                disabled={!!editingId}
                required
              />
            </label>
            <label>
              Password {editingId && '(leave blank to keep)'}
              <input
                type="password"
                value={form.password}
                onChange={(e) => setForm({ ...form, password: e.target.value })}
                required={!editingId}
              />
            </label>
            <label>
              Role
              <select
                value={form.role}
                onChange={(e) => {
                  const role = e.target.value as 'Admin' | 'Salesman';
                  setForm({
                    ...form,
                    role,
                    allowedScreens: role === 'Admin' ? [] : [...salesmanDefaults],
                  });
                }}
              >
                <option value="Admin">Admin (full access)</option>
                <option value="Salesman">Salesman</option>
              </select>
            </label>

            {form.role === 'Salesman' && (
              <div className="screen-permissions">
                <span className="field-label">Allowed Screens</span>
                <div className="checkbox-grid">
                  {assignableScreens.map((screen) => (
                    <label key={screen.key} className="checkbox-item">
                      <input
                        type="checkbox"
                        checked={form.allowedScreens.includes(screen.key as AppScreen)}
                        onChange={() => toggleScreen(screen.key)}
                      />
                      {screen.label}
                    </label>
                  ))}
                </div>
              </div>
            )}

            {editingId && (
              <label className="checkbox-item">
                <input
                  type="checkbox"
                  checked={form.isActive}
                  onChange={(e) => setForm({ ...form, isActive: e.target.checked })}
                />
                Active
              </label>
            )}

            <div className="form-actions">
              <button type="submit" className="btn btn-primary" disabled={saving}>
                {saving ? 'Saving...' : editingId ? 'Update User' : 'Create User'}
              </button>
              {editingId && (
                <button type="button" className="btn btn-secondary" onClick={resetForm}>
                  Cancel
                </button>
              )}
            </div>
          </form>
        </section>

        <section className="card">
          <h3>Users</h3>
          <table className="data-table">
            <thead>
              <tr>
                <th>Username</th>
                <th>Role</th>
                <th>Screens</th>
                <th>Status</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {users.map((user) => (
                <tr key={user.id}>
                  <td>{user.username}</td>
                  <td>{user.role}</td>
                  <td className="screen-list-cell">
                    {user.role === 'Admin' ? 'All screens' : user.allowedScreens.join(', ')}
                  </td>
                  <td>{user.isActive ? 'Active' : 'Inactive'}</td>
                  <td className="table-actions">
                    <button type="button" className="btn btn-sm" onClick={() => startEdit(user)}>
                      Edit
                    </button>
                    <button
                      type="button"
                      className="btn btn-sm btn-danger"
                      onClick={() => handleDelete(user.id, user.username)}
                    >
                      Delete
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </section>
      </div>
    </div>
  );
}
