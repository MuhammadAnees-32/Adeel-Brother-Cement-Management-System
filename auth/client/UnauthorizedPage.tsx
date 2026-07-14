import { Link } from 'react-router-dom';
import { useAuth } from './AuthContext';

export function UnauthorizedPage() {
  const { user, hasScreen } = useAuth();

  const fallback = user
    ? (['NewSale', 'Inventory', 'CustomerBalance', 'Expenses', 'Dashboard', 'SalesHistory'] as const)
        .find((screen) => hasScreen(screen))
    : undefined;

  const fallbackPath =
    fallback === 'NewSale' ? '/sale' :
    fallback === 'Inventory' ? '/inventory' :
    fallback === 'CustomerBalance' ? '/customers' :
    fallback === 'Expenses' ? '/expenses' :
    fallback === 'SalesHistory' ? '/sales' :
    '/';

  return (
    <div className="page">
      <header className="page-header">
        <h2>Access Denied</h2>
        <p>You do not have permission to view this page.</p>
      </header>
      {fallback && (
        <Link to={fallbackPath} className="btn btn-primary">
          Go to allowed page
        </Link>
      )}
    </div>
  );
}
