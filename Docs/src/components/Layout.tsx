import { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { docsConfig, type NavItem } from '../docs.config';
import './Layout.css';

interface LayoutProps {
  children: React.ReactNode;
}

export function Layout({ children }: LayoutProps) {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const location = useLocation();

  return (
    <div className="layout">
      {/* Header */}
      <header className="header">
        <div className="header-left">
          <button
            className="menu-toggle"
            onClick={() => setSidebarOpen(!sidebarOpen)}
            aria-label="Toggle navigation"
          >
            <span className="menu-icon"></span>
          </button>
          <Link to="/" className="logo">
            <span className="logo-text">{docsConfig.title}</span>
            <span className="logo-badge">Docs</span>
          </Link>
        </div>
        <nav className="header-nav">
          <a
            href={docsConfig.links.github}
            target="_blank"
            rel="noopener noreferrer"
            className="header-link"
          >
            GitHub
          </a>
          <a
            href={docsConfig.links.thunderstoreServer}
            target="_blank"
            rel="noopener noreferrer"
            className="header-link"
          >
            Thunderstore
          </a>
        </nav>
      </header>

      <div className="layout-body">
        {/* Sidebar */}
        <aside className={`sidebar ${sidebarOpen ? 'sidebar-open' : ''}`}>
          <nav className="sidebar-nav">
            {docsConfig.nav.map((item) => (
              <NavSection
                key={item.title}
                item={item}
                currentPath={location.pathname}
                onNavigate={() => setSidebarOpen(false)}
              />
            ))}
          </nav>
        </aside>

        {/* Backdrop for mobile */}
        {sidebarOpen && (
          <div
            className="sidebar-backdrop"
            onClick={() => setSidebarOpen(false)}
          />
        )}

        {/* Main content */}
        <main className="main">
          <article className="content">{children}</article>
        </main>
      </div>
    </div>
  );
}

interface NavSectionProps {
  item: NavItem;
  currentPath: string;
  onNavigate: () => void;
  depth?: number;
}

function NavSection({ item, currentPath, onNavigate, depth = 0 }: NavSectionProps) {
  const isActive = item.path === currentPath;
  const hasChildren = item.children && item.children.length > 0;
  const isExpanded = hasChildren && item.children!.some(
    (child) => child.path === currentPath || 
    (child.children?.some(c => c.path === currentPath))
  );

  // Check if this section or any children are active
  const isSectionActive = isActive || 
    (item.path && currentPath.startsWith(item.path) && item.path !== '/');

  return (
    <div className={`nav-section ${depth > 0 ? 'nav-section-nested' : ''}`}>
      {item.path ? (
        <Link
          to={item.path}
          className={`nav-link ${isActive ? 'nav-link-active' : ''} ${isSectionActive ? 'nav-link-section-active' : ''}`}
          onClick={onNavigate}
        >
          {item.title}
          {item.badge && <span className="nav-badge">{item.badge}</span>}
        </Link>
      ) : (
        <span className="nav-label">{item.title}</span>
      )}

      {hasChildren && (isExpanded || !item.path) && (
        <div className="nav-children">
          {item.children!.map((child) => (
            <NavSection
              key={child.title}
              item={child}
              currentPath={currentPath}
              onNavigate={onNavigate}
              depth={depth + 1}
            />
          ))}
        </div>
      )}
    </div>
  );
}
