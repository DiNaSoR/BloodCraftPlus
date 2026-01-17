import { useState } from 'react';
import type { MainTab } from './shared/types';
import { EquipmentPanel } from './features/equipment/EquipmentPanel';
import { BloodcraftPanel } from './features/bloodcraft/BloodcraftPanel';

const MAIN_TAB_LABELS: Record<MainTab, string> = {
  equipment: 'Equipment',
  crafting: 'Crafting',
  bloodpool: 'Blood Pool',
  attributes: 'Attributes',
  bloodcraft: 'Bloodcraft',
};

function App() {
  const [activeMainTab, setActiveMainTab] = useState<MainTab>('equipment');

  return (
    <div className="stage">
      <div className="mock">
        <div className="top-header">Character</div>

        {/* Main Tabs */}
        <div className="main-tabs">
          {(Object.keys(MAIN_TAB_LABELS) as MainTab[]).map((tab) => (
            <button
              key={tab}
              className={`main-tab ${activeMainTab === tab ? 'is-active' : ''}`}
              onClick={() => setActiveMainTab(tab)}
            >
              {MAIN_TAB_LABELS[tab]}
            </button>
          ))}
        </div>

        {/* Equipment Panel */}
        <div
          className={`equipment-panel main-panel ${activeMainTab === 'equipment' ? 'is-active' : ''}`}
          id="panel-equipment"
        >
          <EquipmentPanel />
        </div>

        {/* Placeholder panels for non-implemented tabs */}
        <div
          className={`panel main-panel ${activeMainTab === 'crafting' ? 'is-active' : ''}`}
          id="panel-crafting"
        >
          <div className="note">Crafting panel - not implemented in mock</div>
        </div>

        <div
          className={`panel main-panel ${activeMainTab === 'bloodpool' ? 'is-active' : ''}`}
          id="panel-bloodpool"
        >
          <div className="note">Blood Pool panel - not implemented in mock</div>
        </div>

        <div
          className={`panel main-panel ${activeMainTab === 'attributes' ? 'is-active' : ''}`}
          id="panel-attributes"
        >
          <div className="note">Attributes panel - not implemented in mock</div>
        </div>

        {/* Bloodcraft Panel */}
        <div
          className={`panel main-panel ${activeMainTab === 'bloodcraft' ? 'is-active' : ''}`}
          id="panel-bloodcraft"
        >
          <BloodcraftPanel />
        </div>

        <div className="note">Edit CSS variables in design-mock.tokens.css to tweak sizing.</div>
      </div>
    </div>
  );
}

export default App;
