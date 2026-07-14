import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import {
  AuthProvider,
  LoginPage,
  ProtectedRoute,
  UnauthorizedPage,
  UserManagementPage,
  useAuth,
  type AppScreen,
} from '@auth';
import { Layout } from './components/Layout';
import { CustomerBalancesPage } from './pages/CustomerBalances';
import { DashboardPage } from './pages/Dashboard';
import { ExpensesPage } from './pages/Expenses';
import { InventoryPage } from './pages/Inventory';
import { NewSalePage } from './pages/NewSale';
import { SalesHistoryPage } from './pages/SalesHistory';
import './index.css';

function DefaultRedirect() {
  const { user, isLoading, hasScreen } = useAuth();
  if (isLoading) return <div className="page-loading">Checking session...</div>;
  if (!user) return <Navigate to="/login" replace />;

  const order: { screen: AppScreen; path: string }[] = [
    { screen: 'Dashboard', path: '/dashboard' },
    { screen: 'NewSale', path: '/sale' },
    { screen: 'Inventory', path: '/inventory' },
    { screen: 'CustomerBalance', path: '/customers' },
    { screen: 'Expenses', path: '/expenses' },
    { screen: 'SalesHistory', path: '/sales' },
    { screen: 'UserManagement', path: '/users' },
  ];

  const first = order.find((item) => hasScreen(item.screen));
  return <Navigate to={first?.path ?? '/unauthorized'} replace />;
}

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route element={<ProtectedRoute />}>
            <Route element={<Layout />}>
              <Route index element={<DefaultRedirect />} />
              <Route element={<ProtectedRoute screen="Dashboard" />}>
                <Route path="/dashboard" element={<DashboardPage />} />
              </Route>
              <Route element={<ProtectedRoute screen="NewSale" />}>
                <Route path="/sale" element={<NewSalePage />} />
              </Route>
              <Route element={<ProtectedRoute screen="SalesHistory" />}>
                <Route path="/sales" element={<SalesHistoryPage />} />
              </Route>
              <Route element={<ProtectedRoute screen="CustomerBalance" />}>
                <Route path="/customers" element={<CustomerBalancesPage />} />
              </Route>
              <Route element={<ProtectedRoute screen="Inventory" />}>
                <Route path="/inventory" element={<InventoryPage />} />
              </Route>
              <Route element={<ProtectedRoute screen="Expenses" />}>
                <Route path="/expenses" element={<ExpensesPage />} />
              </Route>
              <Route element={<ProtectedRoute screen="UserManagement" adminOnly />}>
                <Route path="/users" element={<UserManagementPage />} />
              </Route>
              <Route path="/unauthorized" element={<UnauthorizedPage />} />
            </Route>
          </Route>
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App;
