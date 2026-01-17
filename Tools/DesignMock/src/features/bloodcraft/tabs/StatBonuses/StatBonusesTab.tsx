import { useState } from 'react';
import type { StatBonusType, StatRow } from '../../../../shared/types';
import { weaponStats, bloodStats } from '../../../../mock-data';

interface StatListProps {
  stats: StatRow[];
  onToggle: (index: number) => void;
}

function StatList({ stats, onToggle }: StatListProps) {
  return (
    <div className="stat-scrollable">
      <div className="stat-list">
        {stats.map((stat, index) => (
          <div
            key={stat.name}
            className={`stat-row ${stat.selected ? 'stat-row-selected' : ''}`}
            onClick={() => onToggle(index)}
          >
            <div className="stat-check">{stat.selected ? '✓' : ''}</div>
            <div className="stat-name">{stat.name}</div>
            <div className="stat-value">{stat.value}</div>
          </div>
        ))}
      </div>
    </div>
  );
}

export function StatBonusesTab() {
  const [activeType, setActiveType] = useState<StatBonusType>('weapon');
  const [weaponStatsState, setWeaponStatsState] = useState(weaponStats);
  const [bloodStatsState, setBloodStatsState] = useState(bloodStats);

  const toggleWeaponStat = (index: number) => {
    setWeaponStatsState((prev) => prev.map((s, i) => (i === index ? { ...s, selected: !s.selected } : s)));
  };

  const toggleBloodStat = (index: number) => {
    setBloodStatsState((prev) => prev.map((s, i) => (i === index ? { ...s, selected: !s.selected } : s)));
  };

  return (
    <>
      {/* System Toggle Tabs */}
      <div className="stat-bonus-tabs">
        <button
          className={`stat-bonus-tab ${activeType === 'weapon' ? 'is-active' : ''}`}
          onClick={() => setActiveType('weapon')}
        >
          <span className="stat-bonus-tab-icon is-weapon" />
          <span>Weapon Expertise</span>
        </button>
        <button
          className={`stat-bonus-tab ${activeType === 'blood' ? 'is-active' : ''}`}
          onClick={() => setActiveType('blood')}
        >
          <span className="stat-bonus-tab-icon is-blood" />
          <span>Blood Legacies</span>
        </button>
      </div>

      {/* Weapon Expertise Content */}
      <div className={`stat-bonus-content ${activeType === 'weapon' ? 'is-active' : ''}`}>
        <div className="stat-header">
          <div className="stat-header-icon is-sword" />
          <div className="stat-header-info">
            <div className="stat-header-name">Sword</div>
            <div className="stat-header-meta">
              <span className="stat-header-slots">3 / 3 Bonuses</span>
              <span className="stat-header-level">
                Lv.67
                <span className="stat-header-level-bar" style={{ '--pct': '45%' } as React.CSSProperties}>
                  <span />
                </span>
                45%
              </span>
            </div>
          </div>
        </div>
        <StatList stats={weaponStatsState} onToggle={toggleWeaponStat} />
      </div>

      {/* Blood Legacies Content */}
      <div className={`stat-bonus-content ${activeType === 'blood' ? 'is-active' : ''}`}>
        <div className="stat-header">
          <div
            className="stat-header-icon"
            style={{ backgroundImage: "url('/assets/sprites/BloodType_Rogue_Big.png')" }}
          />
          <div className="stat-header-info">
            <div className="stat-header-name">Warrior</div>
            <div className="stat-header-meta">
              <span className="stat-header-slots">2 / 3 Bonuses</span>
              <span className="stat-header-level">
                Lv.42
                <span className="stat-header-level-bar" style={{ '--pct': '78%' } as React.CSSProperties}>
                  <span />
                </span>
                78%
              </span>
            </div>
          </div>
        </div>
        <StatList stats={bloodStatsState} onToggle={toggleBloodStat} />
      </div>
    </>
  );
}
