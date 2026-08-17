import { TopNavigation } from './NavMenu';

export function AppShell({ children }) {
  return (
    <div className="app-shell">
      <TopNavigation />
      <main className="enterprise-main">
        <div className="app-content">{children}</div>
      </main>
    </div>
  );
}

export const Layout = AppShell;
