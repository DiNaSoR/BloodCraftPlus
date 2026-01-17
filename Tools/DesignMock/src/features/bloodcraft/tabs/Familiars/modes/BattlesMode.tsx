import { Card, CardHeader, Select, ActionRow, ChipGrid } from '../../../../../shared/components';
import { battleGroupSlots, battleGroupOptions, players } from '../../../../../mock-data';

export function BattlesMode() {
  return (
    <div className="familiar-scrollable">
      <div className="familiar-layout familiar-animate">
        {/* Left Column */}
        <div className="familiar-column">
          <Card className="familiar-battle-groups familiar-animate delay-1">
            <CardHeader icon="" title="Battle Groups" variant="battle" />

            <div className="familiar-action-list">
              <ActionRow icon="list">List Battle Groups</ActionRow>
              <ActionRow icon="list">Show Active Group</ActionRow>
            </div>

            <div className="familiar-subheader-row">
              <span className="familiar-subheader">Active Group</span>
            </div>
            <Select options={battleGroupOptions} />

            <div className="familiar-tools">
              <div className="familiar-tool-row">
                <button className="familiar-mini-btn is-wide" type="button">
                  Create Group (Auto Name)
                </button>
              </div>
              <div className="familiar-tool-row">
                <button className="familiar-danger-btn" type="button">
                  Delete Active Group
                </button>
              </div>
            </div>
          </Card>
        </div>

        <div className="familiar-column-divider" aria-hidden="true" />

        {/* Right Column */}
        <div className="familiar-column">
          <Card className="familiar-battle-slots familiar-animate delay-1">
            <CardHeader icon="" title="Active Group Slots (1-3)" variant="battle" />

            <div className="familiar-box-list">
              {battleGroupSlots.map((slot) => (
                <button key={slot.slot} type="button" className="familiar-box-row">
                  <span className="familiar-box-icon" />
                  <span className="familiar-box-name">
                    Slot {slot.slot} — {slot.name}
                  </span>
                  <span className="familiar-box-level">{slot.level ? `Lv.${slot.level}` : ''}</span>
                </button>
              ))}
            </div>

            <div className="familiar-subheader-row">
              <span className="familiar-subheader">Assign Active Familiar</span>
            </div>
            <ChipGrid items={['Slot 1', 'Slot 2', 'Slot 3']} />
          </Card>

          <Card className="familiar-battle-queue familiar-animate delay-2">
            <CardHeader icon="" title="Battle Queue" variant="battle" />

            <div className="familiar-subheader-row">
              <span className="familiar-subheader">Challenge Player</span>
            </div>
            <div className="familiar-tool-row">
              <Select options={players} />
              <button className="familiar-mini-btn" type="button">
                Challenge
              </button>
            </div>

            <div className="familiar-action-list" style={{ marginTop: 10 }}>
              <ActionRow icon="queue">Check Queue Status</ActionRow>
              <ActionRow icon="reset">Set Arena Here (Admin)</ActionRow>
            </div>
          </Card>
        </div>
      </div>
    </div>
  );
}
