import type { ReactNode } from 'react';

interface ActionRowProps {
  icon?: string;
  children: ReactNode;
  primary?: boolean;
  onClick?: () => void;
}

export function ActionRow({ icon, children, primary, onClick }: ActionRowProps) {
  const iconClass = icon ? `familiar-action-icon is-${icon}` : 'familiar-action-icon';
  return (
    <div
      className={`familiar-action-row ${primary ? 'is-primary' : ''}`}
      onClick={onClick}
      role="button"
      tabIndex={0}
    >
      <span className={iconClass} />
      <span className="familiar-action-text">{children}</span>
    </div>
  );
}
