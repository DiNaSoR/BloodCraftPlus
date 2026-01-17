import type { ReactNode } from 'react';
import './Callout.css';

type CalloutType = 'info' | 'warning' | 'danger' | 'tip' | 'note';

interface CalloutProps {
  type?: CalloutType;
  title?: string;
  children: ReactNode;
}

const icons: Record<CalloutType, string> = {
  info: 'ℹ️',
  warning: '⚠️',
  danger: '🚨',
  tip: '💡',
  note: '📝',
};

const defaultTitles: Record<CalloutType, string> = {
  info: 'Info',
  warning: 'Warning',
  danger: 'Danger',
  tip: 'Tip',
  note: 'Note',
};

export function Callout({ type = 'info', title, children }: CalloutProps) {
  return (
    <div className={`callout callout-${type}`}>
      <div className="callout-header">
        <span className="callout-icon">{icons[type]}</span>
        <span className="callout-title">{title || defaultTitles[type]}</span>
      </div>
      <div className="callout-content">{children}</div>
    </div>
  );
}
