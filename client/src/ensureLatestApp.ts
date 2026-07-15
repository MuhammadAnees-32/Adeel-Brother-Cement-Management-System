const VERSION_KEY = 'abc_app_version';

/** Reload once when deploy version changes — no manual hard refresh needed. */
export async function ensureLatestApp(): Promise<void> {
  try {
    const response = await fetch(`/app-version.json?_=${Date.now()}`, { cache: 'no-store' });
    if (!response.ok) return;

    const data = (await response.json()) as { version?: string };
    const version = data.version?.trim();
    if (!version) return;

    const stored = sessionStorage.getItem(VERSION_KEY);
    if (stored && stored !== version) {
      sessionStorage.removeItem('abc_auth');
      sessionStorage.setItem(VERSION_KEY, version);
      const { pathname, search, hash } = window.location;
      window.location.replace(`${pathname}${search}${hash}`);
      return;
    }

    sessionStorage.setItem(VERSION_KEY, version);
  } catch {
    // Offline or first run — continue without blocking login
  }
}
