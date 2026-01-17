import type { ReactNode, CSSProperties } from 'react';

interface CardProps {
  children: ReactNode;
  className?: string;
  style?: CSSProperties;
}

export function Card({ children, className = '', style }: CardProps) {
  return <div className={`familiar-card ${className}`} style={style}>{children}</div>;
}

interface CardHeaderProps {
  icon?: string;
  title: string;
  variant?: 'active' | 'settings' | 'box' | 'battle' | 'exo' | 'prestige' | 'class' | 'talent';
}

export function CardHeader({ icon, title, variant }: CardHeaderProps) {
  const variantClass = variant ? `is-${variant}` : '';
  return (
    <div className={`familiar-card-header ${variantClass}`}>
      {icon !== undefined && <span className="familiar-card-icon" />}
      <span className="familiar-card-title">{title}</span>
    </div>
  );
}
