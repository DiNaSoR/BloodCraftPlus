import { useState } from 'react';
import type { ProgressionMode } from '../../../../shared/types';
import { ProfessionsMode } from './modes/ProfessionsMode';
import { ClassMode } from './modes/ClassMode';

export function ProgressionTab() {
  const [mode, setMode] = useState<ProgressionMode>('professions');

  return (
    <>
      {/* Mode Toggle Tabs */}
      <div className="familiar-mode-tabs">
        <button
          className={`familiar-mode-tab ${mode === 'professions' ? 'is-active' : ''}`}
          onClick={() => setMode('professions')}
        >
          <span className="familiar-mode-tab-icon is-profession" />
          <span>Professions</span>
        </button>
        <button
          className={`familiar-mode-tab ${mode === 'class' ? 'is-active' : ''}`}
          onClick={() => setMode('class')}
        >
          <span className="familiar-mode-tab-icon is-class" />
          <span>Class</span>
        </button>
      </div>

      {/* Professions Mode */}
      <div className={`familiar-mode-content ${mode === 'professions' ? 'is-active' : ''}`}>
        <ProfessionsMode />
      </div>

      {/* Class Mode */}
      <div className={`familiar-mode-content ${mode === 'class' ? 'is-active' : ''}`}>
        <ClassMode />
      </div>
    </>
  );
}
