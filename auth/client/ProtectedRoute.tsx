import { Navigate, Outlet } from 'react-router-dom';
import { useAuth, type AppScreen } from './AuthContext';

interface ProtectedRouteProps {
  screen?: AppScreen;
  adminOnly?: boolean;
}

export function ProtectedRoute({ screen, adminOnly }: ProtectedRouteProps) {
  const { user, isLoading, hasScreen } = useAuth();

  if (isLoading) return <div className="page-loading">Checking session...</div>;

  if (!user) return <Navigate to="/login" replace />;

  if (adminOnly && user.role !== 'Admin') {
    return <Navigate to="/unauthorized" replace />;
  }

  if (screen && !hasScreen(screen)) {
    return <Navigate to="/unauthorized" replace />;
  }

  return <Outlet />;
}
