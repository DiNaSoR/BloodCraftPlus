import { useState, type ReactNode } from 'react';
import './ChangelogSection.css';

interface ChangelogSectionProps {
  version: string;
  date: string;
  title?: string;
  defaultOpen?: boolean;
  children: ReactNode;
}

export function ChangelogSection({
  version,
  date,
  title,
  defaultOpen = false,
  children,
}: ChangelogSectionProps) {
  const [isOpen, setIsOpen] = useState(defaultOpen);

  return (
    <div className={`changelog-section ${isOpen ? 'is-open' : ''}`}>
      <button
        type="button"
        className="changelog-header"
        onClick={() => setIsOpen(!isOpen)}
        aria-expanded={isOpen}
      >
        <div className="changelog-header-left">
          <span className="changelog-version">{version}</span>
          {title && <span className="changelog-title">{title}</span>}
        </div>
        <div className="changelog-header-right">
          <span className="changelog-date">{date}</span>
          <span className="changelog-chevron">
            <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
              <path
                d="M6 8L10 12L14 8"
                stroke="currentColor"
                strokeWidth="2"
                strokeLinecap="round"
                strokeLinejoin="round"
              />
            </svg>
          </span>
        </div>
      </button>
      <div className="changelog-body-wrapper">
        <div className="changelog-body">{children}</div>
      </div>
    </div>
  );
}
