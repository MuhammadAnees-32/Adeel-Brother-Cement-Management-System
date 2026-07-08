import { NavLink, Outlet } from 'react-router-dom';

const navItems = [
  { to: '/', label: 'Dashboard', icon: '📊' },
  { to: '/sale', label: 'New Sale', icon: '🧾' },
  { to: '/sales', label: 'Sales History', icon: '📋' },
  { to: '/customers', label: 'Customer Balance', icon: '👥' },
  { to: '/inventory', label: 'Inventory', icon: '📦' },
  { to: '/expenses', label: 'Expenses', icon: '💰' },
];

export function Layout() {
  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand">
          <h1>Adeel & Brother</h1>
          <p>Cement & Sirya Agency</p>
        </div>
        <nav>
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.to === '/'}
              className={({ isActive }) => `nav-link${isActive ? ' active' : ''}`}
            >
              <span>{item.icon}</span>
              {item.label}
            </NavLink>
          ))}
        </nav>
      </aside>
      <main className="main-content">
        <Outlet />
      </main>
    </div>
  );
}
