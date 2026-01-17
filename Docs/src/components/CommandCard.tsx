import { useState } from 'react';
import './CommandCard.css';

interface CommandCardProps {
  command: string;
  shortcut?: string;
  description: string;
  usage?: string;
  adminOnly?: boolean;
}

export function CommandCard({
  command,
  shortcut,
  description,
  usage,
  adminOnly = false,
}: CommandCardProps) {
  const [copied, setCopied] = useState(false);

  const handleCopy = async () => {
    await navigator.clipboard.writeText(command);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <div className={`command-card ${adminOnly ? 'command-admin' : ''}`}>
      <div className="command-header">
        <code className="command-name">{command}</code>
        {adminOnly && <span className="command-badge">Admin</span>}
        <button className="command-copy" onClick={handleCopy}>
          {copied ? '✓' : 'Copy'}
        </button>
      </div>
      <p className="command-desc">{description}</p>
      {usage && (
        <div className="command-usage">
          <span className="command-label">Usage:</span>
          <code>{usage}</code>
        </div>
      )}
      {shortcut && (
        <div className="command-shortcut">
          <span className="command-label">Shortcut:</span>
          <code>{shortcut}</code>
        </div>
      )}
    </div>
  );
}

interface CommandGroupProps {
  title: string;
  children: React.ReactNode;
}

export function CommandGroup({ title, children }: CommandGroupProps) {
  return (
    <div className="command-group">
      <h3 className="command-group-title">{title}</h3>
      <div className="command-group-list">{children}</div>
    </div>
  );
}
