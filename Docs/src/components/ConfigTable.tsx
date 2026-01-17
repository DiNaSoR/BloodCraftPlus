import { useState, useMemo } from 'react';
import './ConfigTable.css';

export interface ConfigEntry {
  key: string;
  type: string;
  default: string;
  description: string;
}

export interface ConfigSection {
  name: string;
  entries: ConfigEntry[];
}

interface ConfigTableProps {
  sections: ConfigSection[];
}

export function ConfigTable({ sections }: ConfigTableProps) {
  const [search, setSearch] = useState('');
  const [expandedSections, setExpandedSections] = useState<Set<string>>(
    new Set(sections.map((s) => s.name))
  );

  const filteredSections = useMemo(() => {
    if (!search.trim()) return sections;

    const lower = search.toLowerCase();
    return sections
      .map((section) => ({
        ...section,
        entries: section.entries.filter(
          (entry) =>
            entry.key.toLowerCase().includes(lower) ||
            entry.description.toLowerCase().includes(lower)
        ),
      }))
      .filter((section) => section.entries.length > 0);
  }, [sections, search]);

  const toggleSection = (name: string) => {
    const newExpanded = new Set(expandedSections);
    if (newExpanded.has(name)) {
      newExpanded.delete(name);
    } else {
      newExpanded.add(name);
    }
    setExpandedSections(newExpanded);
  };

  return (
    <div className="config-table">
      <div className="config-search">
        <input
          type="text"
          placeholder="Search configuration..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="config-search-input"
        />
        {search && (
          <button className="config-search-clear" onClick={() => setSearch('')}>
            ×
          </button>
        )}
      </div>

      {filteredSections.map((section) => (
        <div key={section.name} className="config-section">
          <button
            className="config-section-header"
            onClick={() => toggleSection(section.name)}
          >
            <span className="config-section-toggle">
              {expandedSections.has(section.name) ? '▼' : '▶'}
            </span>
            <span className="config-section-title">{section.name}</span>
            <span className="config-section-count">
              {section.entries.length} options
            </span>
          </button>

          {expandedSections.has(section.name) && (
            <div className="config-entries">
              {section.entries.map((entry) => (
                <div key={entry.key} className="config-entry" id={entry.key}>
                  <div className="config-entry-header">
                    <code className="config-key">{entry.key}</code>
                    <span className="config-type">{entry.type}</span>
                  </div>
                  <div className="config-default">
                    <span className="config-label">Default:</span>
                    <code>{entry.default}</code>
                  </div>
                  <p className="config-desc">{entry.description}</p>
                </div>
              ))}
            </div>
          )}
        </div>
      ))}

      {filteredSections.length === 0 && (
        <div className="config-empty">
          No configuration options found matching "{search}"
        </div>
      )}
    </div>
  );
}
