interface ChipGridProps {
  items: string[];
  activeItem?: string;
  onSelect?: (item: string) => void;
  columns?: 2 | 3;
}

export function ChipGrid({ items, activeItem, onSelect, columns = 3 }: ChipGridProps) {
  const style = columns === 2 ? { gridTemplateColumns: 'repeat(2, 1fr)' } : undefined;
  return (
    <div className="familiar-chip-grid" style={style}>
      {items.map((item) => (
        <button
          key={item}
          type="button"
          className={`familiar-chip ${activeItem === item ? 'is-active' : ''}`}
          onClick={() => onSelect?.(item)}
        >
          {item}
        </button>
      ))}
    </div>
  );
}
