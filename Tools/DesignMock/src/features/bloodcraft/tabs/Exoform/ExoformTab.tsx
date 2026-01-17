import { Card, CardHeader } from '../../../../shared/components';

export function ExoformTab() {
  return (
    <div className="familiar-scrollable">
      <div className="familiar-layout familiar-animate">
        {/* Left Column */}
        <div className="familiar-column is-left">
          <Card className="familiar-animate delay-1">
            <CardHeader icon="" title="Exoform Overview" variant="exo" />

            <div className="familiar-active-name">Current Form: Evolved Vampire</div>
            <div className="familiar-active-meta">Exo Prestiges: 12 | Minimum to enter: 15.0s</div>

            <div className="familiar-bond-meter exo-charge-meter" style={{ '--pct': '72%' } as React.CSSProperties}>
              <span />
            </div>
            <div className="familiar-bond-label">Stored Energy: 129.6 / 180.0s</div>

            <div className="familiar-action-list" style={{ marginTop: 10 }}>
              <button className="familiar-action-row is-primary" type="button">
                <span className="familiar-action-icon is-toggle" />
                <span className="familiar-action-text">Taunt to Exoform</span>
                <span className="exo-status is-on">On</span>
              </button>
            </div>

            <div className="exo-hint">
              Charge refills over real time (about 1 day to fully recharge). When enabled, use the Taunt emote to enter
              Exoform.
            </div>
          </Card>

          <Card className="familiar-animate delay-2">
            <CardHeader icon="" title="Forms" variant="exo" />

            <div className="familiar-action-list">
              <button className="familiar-action-row is-primary exo-form-row" type="button">
                <span className="familiar-action-icon is-list" />
                <span className="familiar-action-text">Evolved Vampire</span>
                <span className="exo-status">Selected</span>
              </button>
              <div className="exo-form-note">Unlocked by consuming Dracula essence.</div>

              <button className="familiar-action-row exo-form-row" type="button" disabled>
                <span className="familiar-action-icon is-list" />
                <span className="familiar-action-text">Corrupted Serpent</span>
                <span className="exo-status is-locked">Locked</span>
              </button>
              <div className="exo-form-note">Requires Megara essence.</div>
            </div>
          </Card>
        </div>

        <div className="familiar-column-divider" aria-hidden="true" />

        {/* Right Column */}
        <div className="familiar-column is-right">
          <Card className="familiar-animate delay-1">
            <CardHeader icon="" title="Active Form Abilities" variant="exo" />

            <div className="exo-ability-list">
              <div className="exo-ability-row">
                <span className="exo-ability-icon" />
                <span className="exo-ability-name">Ability 1</span>
                <span className="exo-ability-cooldown">8.0s</span>
              </div>
              <div className="exo-ability-row">
                <span className="exo-ability-icon" />
                <span className="exo-ability-name">Ability 2</span>
                <span className="exo-ability-cooldown">12.0s</span>
              </div>
              <div className="exo-ability-row">
                <span className="exo-ability-icon" />
                <span className="exo-ability-name">Ability 3</span>
                <span className="exo-ability-cooldown">30.0s</span>
              </div>
            </div>

            <div className="exo-hint">Cooldowns are form-specific and update when you select a different form.</div>
          </Card>

          <Card className="familiar-animate delay-2">
            <CardHeader icon="" title="Notes" variant="exo" />

            <div className="exo-hint">
              - You need at least <b>15s</b> stored energy to enter Exoform.
              <br />
              - Energy drains while in Exoform; if it ends naturally, energy is consumed.
              <br />- Max duration scales with Exo Prestiges (up to <b>180s</b>).
            </div>
          </Card>
        </div>
      </div>
    </div>
  );
}
