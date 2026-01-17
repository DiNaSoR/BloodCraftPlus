import { useState } from 'react';
import { Card, CardHeader } from '../../../../../shared/components';
import { classes, classSpells } from '../../../../../mock-data';

export function ClassMode() {
  const [activeClassIndex, setActiveClassIndex] = useState(0);
  const [activeSpellIndex, setActiveSpellIndex] = useState(0);

  return (
    <div className="familiar-scrollable">
      <div className="familiar-layout familiar-animate">
        {/* Left Column: Class Selection */}
        <div className="familiar-column is-left">
          <Card className="familiar-animate delay-1">
            <CardHeader icon="" title="Class Selection" variant="class" />

            <div className="familiar-active-name">
              Current: <span style={{ color: '#ff4444' }}>Blood Knight</span>
            </div>
            <div className="familiar-active-meta">On-Hit: Leech → Blood Curse</div>

            <div className="class-list" style={{ marginTop: 12 }}>
              {classes.map((cls, index) => (
                <button
                  key={cls.name}
                  type="button"
                  className={`class-row ${activeClassIndex === index ? 'is-active' : ''}`}
                  style={{ '--class-color': cls.color } as React.CSSProperties}
                  onClick={() => setActiveClassIndex(index)}
                >
                  <span className="class-row-icon" />
                  <span className="class-row-name">{cls.name}</span>
                  {activeClassIndex === index && <span className="class-row-status">Active</span>}
                </button>
              ))}
            </div>

            <div className="exo-hint" style={{ marginTop: 10 }}>
              Click a class to select or change. Changing class may require a special item.
            </div>
          </Card>
        </div>

        <div className="familiar-column-divider" aria-hidden="true" />

        {/* Right Column: Class Spells */}
        <div className="familiar-column is-right">
          <Card className="familiar-animate delay-1">
            <CardHeader icon="" title="Class Spells" variant="class" />

            <div className="familiar-active-meta">
              Shift Slot: <span style={{ color: '#9ef2b5' }}>Ready</span>
            </div>

            <div className="class-spell-list" style={{ marginTop: 12 }}>
              {classSpells.map((spell, index) => (
                <button
                  key={spell.index}
                  type="button"
                  className={`class-spell-row ${activeSpellIndex === index ? 'is-active' : ''}`}
                  disabled={spell.locked}
                  onClick={() => !spell.locked && setActiveSpellIndex(index)}
                >
                  <span className="class-spell-index">{spell.index}</span>
                  <span className="class-spell-name">{spell.name}</span>
                  {spell.requirement && (
                    <span className={`class-spell-req ${spell.locked ? 'is-locked' : ''}`}>{spell.requirement}</span>
                  )}
                </button>
              ))}
            </div>

            <div className="exo-hint" style={{ marginTop: 10 }}>
              Click a spell to set it as your Shift ability. (P#) indicates prestige requirement.
            </div>
          </Card>

          <Card className="familiar-animate delay-2" style={{ marginTop: 12 }}>
            <CardHeader icon="" title="Stat Synergies" variant="class" />

            <div className="class-synergy-section">
              <div className="class-synergy-label">Weapon Stats (1.5x)</div>
              <div className="class-synergy-chips">
                <span className="class-synergy-chip">Physical Power</span>
                <span className="class-synergy-chip">Primary Leech</span>
                <span className="class-synergy-chip">Phys Crit Dmg</span>
              </div>
            </div>

            <div className="class-synergy-section" style={{ marginTop: 8 }}>
              <div className="class-synergy-label">Blood Stats (1.5x)</div>
              <div className="class-synergy-chips">
                <span className="class-synergy-chip">Healing Received</span>
                <span className="class-synergy-chip">Dmg Reduction</span>
                <span className="class-synergy-chip">Phys Resistance</span>
              </div>
            </div>
          </Card>
        </div>
      </div>
    </div>
  );
}
