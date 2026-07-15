import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import { api } from '@/api/client';

export type AppScreen =
  | 'Dashboard'
  | 'NewSale'
  | 'SalesHistory'
  | 'CustomerBalance'
  | 'KhataBook'
  | 'Inventory'
  | 'Dealers'
  | 'AdvanceBookings'
  | 'Reports'
  | 'Expenses'
  | 'UserManagement';

export interface AuthUser {
  username: string;
  role: 'Admin' | 'Salesman';
  allowedScreens: AppScreen[];
  token: string;
}

interface AuthContextValue {
  user: AuthUser | null;
  isLoading: boolean;
  login: (user: AuthUser) => void;
  logout: () => void;
  hasScreen: (screen: AppScreen) => boolean;
  isAdmin: boolean;
}

const STORAGE_KEY = 'abc_auth';
const storage = sessionStorage;

const AuthContext = createContext<AuthContextValue | null>(null);

function isTokenExpired(token: string): boolean {
  if (!token || token === 'temporary-token' || !token.includes('.')) return true;

  try {
    const payloadPart = token.split('.')[1];
    const payload = JSON.parse(atob(payloadPart.replace(/-/g, '+').replace(/_/g, '/'))) as { exp?: number };
    return typeof payload.exp !== 'number' || payload.exp * 1000 <= Date.now();
  } catch {
    return true;
  }
}

function readStoredUser(): AuthUser | null {
  const raw = storage.getItem(STORAGE_KEY);
  if (!raw) return null;

  try {
    const user = JSON.parse(raw) as AuthUser;
    if (!user?.token || !user?.username || isTokenExpired(user.token)) {
      storage.removeItem(STORAGE_KEY);
      return null;
    }
    return user;
  } catch {
    storage.removeItem(STORAGE_KEY);
    return null;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    async function verifySession() {
      const stored = readStoredUser();
      if (!stored) {
        if (!cancelled) {
          setUser(null);
          setIsLoading(false);
        }
        return;
      }

      try {
        const me = await api.getMe();
        if (cancelled) return;

        setUser({
          username: me.username,
          role: me.role as 'Admin' | 'Salesman',
          allowedScreens: me.allowedScreens as AppScreen[],
          token: stored.token,
        });
      } catch {
        if (!cancelled) {
          storage.removeItem(STORAGE_KEY);
          setUser(null);
        }
      } finally {
        if (!cancelled) setIsLoading(false);
      }
    }

    verifySession();
    return () => {
      cancelled = true;
    };
  }, []);

  const login = useCallback((authUser: AuthUser) => {
    storage.setItem(STORAGE_KEY, JSON.stringify(authUser));
    setUser(authUser);
    setIsLoading(false);
  }, []);

  const logout = useCallback(() => {
    storage.removeItem(STORAGE_KEY);
    setUser(null);
    setIsLoading(false);
  }, []);

  const hasScreen = useCallback(
    (screen: AppScreen) => {
      if (!user) return false;
      if (user.role === 'Admin') return true;
      return user.allowedScreens.includes(screen);
    },
    [user]
  );

  const value = useMemo(
    () => ({
      user,
      isLoading,
      login,
      logout,
      hasScreen,
      isAdmin: user?.role === 'Admin',
    }),
    [user, isLoading, login, logout, hasScreen]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used within AuthProvider');
  return context;
}
