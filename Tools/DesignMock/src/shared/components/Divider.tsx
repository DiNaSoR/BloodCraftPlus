interface DividerProps {
  className?: string;
}

export function Divider({ className = '' }: DividerProps) {
  return <div className={`divider familiar-divider ${className}`} />;
}
