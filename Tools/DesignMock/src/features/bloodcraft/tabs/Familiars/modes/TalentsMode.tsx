import { Card, CardHeader } from '../../../../../shared/components';
import { speedTalents, powerTalents, vitalityTalents } from '../../../../../mock-data';
import type { TalentNode } from '../../../../../shared/types';

interface TalentPathProps {
  label: string;
  pathClass: string;
  talents: TalentNode[];
}

function TalentPath({ label, pathClass, talents }: TalentPathProps) {
  return (
    <div className={`talent-path ${pathClass}`}>
      <div className="talent-path-label">{label}</div>
      {talents.map((node, index) => (
        <div key={node.id}>
          <div
            className={`talent-node ${node.tier} ${node.allocated ? 'is-allocated' : ''} ${node.available ? 'is-available' : ''} ${node.enrage ? 'enrage' : ''}`}
            data-id={node.id}
          >
            <div className="talent-node-inner">
              {node.tier === 'minor' && (index === 0 ? 'I' : 'II')}
              {node.tier === 'notable' && (pathClass === 'speed-path' ? '⚡' : pathClass === 'power-path' ? '⚔' : '🛡')}
              {node.tier === 'keystone' && (node.enrage ? '🔥' : pathClass === 'speed-path' ? '💀' : '🏰')}
            </div>
          </div>
          {index < talents.length - 1 && <div className="talent-connection" />}
        </div>
      ))}
    </div>
  );
}

export function TalentsMode() {
  return (
    <div className="familiar-scrollable">
      <Card className="talent-card" style={{ height: 'auto', minHeight: 600 }}>
        <CardHeader icon="" title="Familiar Talent Tree" variant="talent" />

        <div className="talent-points-header">
          <span className="talent-points-label">Talent Points:</span>
          <span className="talent-points-value">0</span>
          <span className="talent-points-spent">/ 3 available</span>
          <span className="talent-familiar-name">Wolf Lv.10</span>
        </div>

        {/* Talent Tree Visualization */}
        <div className="talent-tree">
          <TalentPath label="Speed" pathClass="speed-path" talents={speedTalents} />
          <TalentPath label="Power" pathClass="power-path" talents={powerTalents} />
          <TalentPath label="Vitality" pathClass="vitality-path" talents={vitalityTalents} />
        </div>

        {/* Talent Info Panel */}
        <Card className="talent-info-card">
          <div className="talent-info-title">Hover a Talent</div>
          <div className="talent-info-desc">See details here.</div>
          <div className="talent-info-effect" />
        </Card>

        {/* Reset Button */}
        <div className="familiar-tools" style={{ marginTop: 12 }}>
          <button className="familiar-danger-btn" type="button">
            Reset All Talents
          </button>
        </div>
      </Card>
    </div>
  );
}
