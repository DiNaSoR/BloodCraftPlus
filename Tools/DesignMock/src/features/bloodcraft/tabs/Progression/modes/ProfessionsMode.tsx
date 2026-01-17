import { professions } from '../../../../../mock-data';

export function ProfessionsMode() {
  const totalLevel = professions.reduce((sum, p) => sum + p.level, 0);
  const avgProgress = Math.round(professions.reduce((sum, p) => sum + p.progress, 0) / professions.length);

  return (
    <>
      <div className="prof-table">
        <div className="prof-header">
          <div />
          <div>Profession</div>
          <div>Level</div>
          <div>Progress</div>
          <div />
        </div>

        {professions.map((prof) => (
          <div key={prof.name} className="prof-row" style={{ '--pct': `${prof.progress}%` } as React.CSSProperties}>
            <div className="prof-icon" />
            <div className="prof-name" style={{ color: prof.color }}>
              {prof.name}
            </div>
            <div>{prof.level}</div>
            <div className="progress">
              <span />
            </div>
            <div>{prof.progress}%</div>
          </div>
        ))}
      </div>

      <div className="prof-footer">
        <div>Total Level: {totalLevel}</div>
        <div>Avg Progress: {avgProgress}%</div>
      </div>
    </>
  );
}
