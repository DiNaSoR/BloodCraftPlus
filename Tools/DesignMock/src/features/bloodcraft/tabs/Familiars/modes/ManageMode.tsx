import { useState } from 'react';
import { Card, CardHeader, Select, Divider, ActionRow, ChipGrid } from '../../../../../shared/components';
import { familiarBoxes, overflowFamiliars, boxOptions, shinyBuffs } from '../../../../../mock-data';

export function ManageMode() {
  const [activeShinyBuff, setActiveShinyBuff] = useState('Blood');
  const [boxClickAction, setBoxClickAction] = useState<'Bind' | 'Remove'>('Bind');

  return (
    <div className="familiar-scrollable">
      <div className="familiar-layout familiar-animate">
        {/* Left Column */}
        <div className="familiar-column is-left">
          {/* Active Familiar Card */}
          <Card className="familiar-active-section familiar-animate delay-1">
            <CardHeader icon="" title="Active Familiar" variant="active" />
            <div className="familiar-portrait" />
            <div className="familiar-active-name">Scout Lv.1</div>
            <div className="familiar-active-stats">HP:500 | PP:1 | SP:1</div>
            <div className="familiar-active-meta">Progress: 0% | Max: 90</div>
            <div className="familiar-bond-meter" style={{ '--pct': '0%' } as React.CSSProperties}>
              <span />
            </div>
            <div className="familiar-bond-label">Bond Strength</div>
          </Card>

          {/* Settings Card */}
          <Card className="familiar-settings-section familiar-animate delay-2">
            <CardHeader icon="" title="Shiny & Options" variant="settings" />

            <div className="familiar-action-list">
              <ActionRow icon="shiny">Toggle Shiny Effects</ActionRow>
              <ActionRow icon="vblood">Toggle VBlood Emotes</ActionRow>
            </div>

            <div className="familiar-subheader-row">
              <span className="familiar-subheader">Apply / Change Shiny Buff</span>
            </div>
            <ChipGrid items={shinyBuffs} activeItem={activeShinyBuff} onSelect={setActiveShinyBuff} />

            <button className="familiar-danger-btn" type="button">
              Reset Familiar State
            </button>
          </Card>

          <Divider />

          {/* Quick Actions */}
          <div className="familiar-advanced-section familiar-animate delay-3">
            <div className="familiar-section-label">Quick Actions</div>
            <div className="familiar-action-list">
              <ActionRow icon="call" primary>
                Call / Dismiss Familiar
              </ActionRow>
              <ActionRow icon="toggle">Toggle Combat Mode</ActionRow>
              <ActionRow icon="unbind">Unbind Familiar</ActionRow>
              <ActionRow icon="search">Search Familiars</ActionRow>
              <ActionRow icon="overflow">View Overflow</ActionRow>
              <ActionRow icon="emote">Toggle Emote Actions</ActionRow>
              <ActionRow icon="show">Show Emote Actions</ActionRow>
              <ActionRow icon="level">Get Familiar Level</ActionRow>
              <ActionRow icon="prestige">Prestige Familiar</ActionRow>
            </div>
          </div>
        </div>

        <div className="familiar-column-divider" aria-hidden="true" />

        {/* Right Column */}
        <div className="familiar-column is-right">
          <Card className="familiar-box-section familiar-animate delay-2">
            <CardHeader icon="" title="Boxes & Storage" variant="box" />

            <div className="familiar-subheader-row">
              <span className="familiar-subheader">Destination Box</span>
            </div>
            <Select options={boxOptions} />

            <div className="familiar-subheader-row">
              <span className="familiar-subheader">Select Box</span>
            </div>
            <Select options={boxOptions} />

            <div className="familiar-subheader-row">
              <span className="familiar-subheader">Current Box</span>
            </div>
            <div className="familiar-box-list">
              {familiarBoxes.map((fam, index) => (
                <button
                  key={index}
                  type="button"
                  className={`familiar-box-row ${fam.active ? 'is-active' : ''} ${fam.empty ? 'is-empty' : ''}`}
                  disabled={fam.empty}
                >
                  <span className="familiar-box-icon" />
                  <span className="familiar-box-name">{fam.name}</span>
                  <span className="familiar-box-level">{fam.level > 0 ? `Lv.${fam.level}` : ''}</span>
                </button>
              ))}
            </div>

            <Divider />

            <div className="familiar-subheader-row">
              <span className="familiar-subheader">Box Tools</span>
            </div>
            <div className="familiar-tools">
              <div className="familiar-tool-row">
                <button className="familiar-mini-btn is-wide" type="button">
                  Add Box (Auto Name)
                </button>
              </div>
              <div className="familiar-tool-row">
                <button className="familiar-mini-btn is-wide" type="button">
                  Rename Active Box (Auto Name)
                </button>
              </div>
              <div className="familiar-tool-row">
                <button className="familiar-danger-btn" type="button">
                  Delete Active Box
                </button>
              </div>
              <div className="familiar-tool-row">
                <button className="familiar-mini-btn is-wide" type="button">
                  Move Active Familiar → Destination Box
                </button>
              </div>
              <div className="familiar-subheader-row">
                <span className="familiar-subheader">Current Box Click Action</span>
              </div>
              <ChipGrid
                items={['Bind', 'Remove']}
                activeItem={boxClickAction}
                onSelect={(item) => setBoxClickAction(item as 'Bind' | 'Remove')}
                columns={2}
              />
            </div>

            <Divider />

            <div className="familiar-subheader-row">
              <span className="familiar-subheader">Overflow</span>
            </div>
            <div className="familiar-box-list">
              {overflowFamiliars.map((fam, index) => (
                <button
                  key={index}
                  type="button"
                  className={`familiar-box-row ${fam.active ? 'is-active' : ''}`}
                >
                  <span className="familiar-box-icon" />
                  <span className="familiar-box-name">{fam.name}</span>
                  <span className="familiar-box-level">Lv.{fam.level}</span>
                </button>
              ))}
              <div className="familiar-tools">
                <div className="familiar-tool-row">
                  <button className="familiar-mini-btn is-wide" type="button">
                    Move Selected Overflow → Destination Box
                  </button>
                </div>
                <div className="familiar-tool-row">
                  <button className="familiar-mini-btn is-wide" type="button">
                    Refresh Overflow
                  </button>
                </div>
              </div>
            </div>
          </Card>
        </div>
      </div>
    </div>
  );
}
