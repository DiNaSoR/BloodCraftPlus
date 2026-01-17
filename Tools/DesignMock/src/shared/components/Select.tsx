interface SelectProps {
  options: string[];
  value?: string;
  onChange?: (value: string) => void;
  className?: string;
}

export function Select({ options, value, onChange, className = '' }: SelectProps) {
  return (
    <div className={`familiar-select ${className}`}>
      <select value={value} onChange={(e) => onChange?.(e.target.value)}>
        {options.map((opt) => (
          <option key={opt} value={opt}>
            {opt}
          </option>
        ))}
      </select>
      <span className="familiar-select-arrow" />
    </div>
  );
}
