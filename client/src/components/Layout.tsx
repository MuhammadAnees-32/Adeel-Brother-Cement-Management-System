import { NavLink, Outlet } from 'react-router-dom';
import { useAuth, type AppScreen } from '@auth';

const navItems: { to: string; label: string; icon: string; screen: AppScreen }[] = [
  { to: '/dashboard', label: 'Dashboard', icon: '📊', screen: 'Dashboard' },
  { to: '/sale', label: 'New Sale', icon: '🧾', screen: 'NewSale' },
  { to: '/sales', label: 'Sales History', icon: '📋', screen: 'SalesHistory' },
  { to: '/customers', label: 'Customer Balance', icon: '👥', screen: 'CustomerBalance' },
  { to: '/khata', label: 'Khata Book', icon: '📒', screen: 'KhataBook' },
  { to: '/inventory', label: 'Inventory', icon: '📦', screen: 'Inventory' },
  { to: '/dealers', label: 'Dealers', icon: '🏭', screen: 'Dealers' },
  { to: '/bookings', label: 'Bookings', icon: '📅', screen: 'AdvanceBookings' },
  { to: '/reports', label: 'Reports', icon: '📈', screen: 'Reports' },
  { to: '/expenses', label: 'Expenses', icon: '💰', screen: 'Expenses' },
  { to: '/users', label: 'User Access', icon: '🔐', screen: 'UserManagement' },
];

export function Layout() {
  const { user, logout, hasScreen } = useAuth();
  const visibleItems = navItems.filter((item) => hasScreen(item.screen));

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand">
          <h1>Adeel & Brother</h1>
          <p>Cement & Sirya Agency</p>
          <p className="app-version">App v2 — Khata, Dealers, Bookings, Reports</p>
        </div>
        <nav>
          {visibleItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) => `nav-link${isActive ? ' active' : ''}`}
            >
              <span>{item.icon}</span>
              {item.label}
            </NavLink>
          ))}
        </nav>
        <div className="sidebar-footer">
          <div className="user-info">
            <strong>{user?.username}</strong>
            <span>{user?.role}</span>
          </div>
          <button type="button" className="btn btn-logout" onClick={logout}>
            Sign Out
          </button>
        </div>
      </aside>
      <main className="main-content">
        <Outlet />
      </main>
    </div>
  );
}
