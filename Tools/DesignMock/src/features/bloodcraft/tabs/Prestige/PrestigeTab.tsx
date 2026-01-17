import { Card, CardHeader, Select } from '../../../../shared/components';
import { prestigeLeaderboard, prestigeTypes } from '../../../../mock-data';

export function PrestigeTab() {
  return (
    <div className="familiar-scrollable">
      <Card>
        <CardHeader icon="" title="Prestige Leaderboard" variant="prestige" />

        <div className="prestige-toolbar">
          <div className="prestige-hint">Click type to cycle.</div>
          <Select options={prestigeTypes} className="max-w-60" />
        </div>

        <div className="stat-scrollable prestige-scrollable">
          <div className="stat-list">
            {prestigeLeaderboard.map((entry) => (
              <div key={entry.rank} className={`stat-row ${entry.top ? 'prestige-row-top' : ''}`}>
                <div className="stat-check">{entry.rank}</div>
                <div className="stat-name">{entry.name}</div>
                <div className="stat-value">{entry.value}</div>
              </div>
            ))}
          </div>
        </div>

        <div className="exo-hint" style={{ marginTop: 10 }}>
          Leaderboards show the top players for the selected prestige type. Some types may be disabled by the server.
        </div>
      </Card>
    </div>
  );
}
