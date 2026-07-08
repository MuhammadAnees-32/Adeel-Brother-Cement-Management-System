import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { Layout } from './components/Layout';
import { CustomerBalancesPage } from './pages/CustomerBalances';
import { DashboardPage } from './pages/Dashboard';
import { ExpensesPage } from './pages/Expenses';
import { InventoryPage } from './pages/Inventory';
import { NewSalePage } from './pages/NewSale';
import { SalesHistoryPage } from './pages/SalesHistory';
import './index.css';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<Layout />}>
          <Route path="/" element={<DashboardPage />} />
          <Route path="/sale" element={<NewSalePage />} />
          <Route path="/sales" element={<SalesHistoryPage />} />
          <Route path="/customers" element={<CustomerBalancesPage />} />
          <Route path="/inventory" element={<InventoryPage />} />
          <Route path="/expenses" element={<ExpensesPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
