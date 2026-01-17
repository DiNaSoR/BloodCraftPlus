import { useState } from 'react';
import type { FamiliarsMode } from '../../../../shared/types';
import { ManageMode } from './modes/ManageMode';
import { BattlesMode } from './modes/BattlesMode';
import { TalentsMode } from './modes/TalentsMode';

export function FamiliarsTab() {
  const [mode, setMode] = useState<FamiliarsMode>('manage');

  return (
    <>
      {/* Mode Toggle Tabs */}
      <div className="familiar-mode-tabs">
        <button
          className={`familiar-mode-tab ${mode === 'manage' ? 'is-active' : ''}`}
          onClick={() => setMode('manage')}
        >
          <span className="familiar-mode-tab-icon is-familiar" />
          <span>Familiars</span>
        </button>
        <button
          className={`familiar-mode-tab ${mode === 'battles' ? 'is-active' : ''}`}
          onClick={() => setMode('battles')}
        >
          <span className="familiar-mode-tab-icon is-battle" />
          <span>Battles</span>
        </button>
        <button
          className={`familiar-mode-tab ${mode === 'talents' ? 'is-active' : ''}`}
          onClick={() => setMode('talents')}
        >
          <span className="familiar-mode-tab-icon is-talent" />
          <span>Talents</span>
        </button>
      </div>

      {/* Manage Mode */}
      <div className={`familiar-mode-content ${mode === 'manage' ? 'is-active' : ''}`} id="familiar-manage-content">
        <ManageMode />
      </div>

      {/* Battles Mode */}
      <div className={`familiar-mode-content ${mode === 'battles' ? 'is-active' : ''}`} id="familiar-battles-content">
        <BattlesMode />
      </div>

      {/* Talents Mode */}
      <div className={`familiar-mode-content ${mode === 'talents' ? 'is-active' : ''}`} id="familiar-talents-content">
        <TalentsMode />
      </div>
    </>
  );
}
