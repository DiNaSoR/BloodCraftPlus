import { useState } from 'react';
import type { BloodcraftTab } from '../../shared/types';
import { PrestigeTab } from './tabs/Prestige/PrestigeTab';
import { ExoformTab } from './tabs/Exoform/ExoformTab';
import { StatBonusesTab } from './tabs/StatBonuses/StatBonusesTab';
import { ProgressionTab } from './tabs/Progression/ProgressionTab';
import { FamiliarsTab } from './tabs/Familiars/FamiliarsTab';

const TAB_LABELS: Record<BloodcraftTab, string> = {
  prestige: 'Prestige',
  exoform: 'Exoform',
  bonuses: 'Stat Bonuses',
  progression: 'Progression',
  familiars: 'Familiars',
};

export function BloodcraftPanel() {
  const [activeTab, setActiveTab] = useState<BloodcraftTab>('familiars');

  return (
    <>
      {/* Sub-tabs */}
      <div className="subtabs">
        {(Object.keys(TAB_LABELS) as BloodcraftTab[]).map((tab) => (
          <button
            key={tab}
            className={`subtab ${activeTab === tab ? 'is-active' : ''}`}
            onClick={() => setActiveTab(tab)}
          >
            {TAB_LABELS[tab]}
          </button>
        ))}
      </div>

      {/* Tab Content */}
      <div className="content">
        <div className={`content-section ${activeTab === 'prestige' ? 'is-active' : ''}`} id="tab-prestige">
          <PrestigeTab />
        </div>

        <div className={`content-section ${activeTab === 'exoform' ? 'is-active' : ''}`} id="tab-exoform">
          <ExoformTab />
        </div>

        <div className={`content-section ${activeTab === 'bonuses' ? 'is-active' : ''}`} id="tab-bonuses">
          <StatBonusesTab />
        </div>

        <div className={`content-section ${activeTab === 'progression' ? 'is-active' : ''}`} id="tab-progression">
          <ProgressionTab />
        </div>

        <div className={`content-section ${activeTab === 'familiars' ? 'is-active' : ''}`} id="tab-familiars">
          <FamiliarsTab />
        </div>
      </div>
    </>
  );
}
