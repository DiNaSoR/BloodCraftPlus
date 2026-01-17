export function EquipmentPanel() {
  return (
    <>
      {/* Gear Level Section */}
      <div className="gear-level-section">
        <div className="gear-level-number">3</div>
        <div className="gear-level-bar" />
        <div className="gear-level-label">Gear Level</div>
        <div className="bloodcraft-stats-summary">WP:6 | Cl:4 | Exp:15</div>
      </div>

      {/* Stats Row */}
      <div className="equipment-stats">
        <div className="equipment-stat">
          <div className="equipment-stat-icon" />
          <div className="equipment-stat-value">10.0</div>
          <div className="equipment-stat-label">Physical Power</div>
        </div>
        <div className="equipment-stat">
          <div className="equipment-stat-icon" />
          <div className="equipment-stat-value">220</div>
          <div className="equipment-stat-label">Max Health</div>
        </div>
        <div className="equipment-stat">
          <div className="equipment-stat-icon" />
          <div className="equipment-stat-value">15.3</div>
          <div className="equipment-stat-label">Spell Power</div>
        </div>
      </div>

      {/* Equipment Slots */}
      <div className="equipment-slots">
        <div className="equipment-slot">
          <div className="equipment-slot-icon" />
        </div>
        <div className="equipment-slot has-item">
          <div className="equipment-slot-icon" />
          <div className="equipment-slot-upgrade" />
        </div>
        <div className="equipment-slot has-item">
          <div className="equipment-slot-icon" />
        </div>
        <div className="equipment-slot has-item">
          <div className="equipment-slot-icon" />
          <div className="equipment-slot-upgrade" />
        </div>
        <div className="equipment-slot has-item">
          <div className="equipment-slot-icon" />
        </div>
        <div className="equipment-slot">
          <div className="equipment-slot-icon" />
        </div>
        <div className="equipment-slot has-item">
          <div className="equipment-slot-icon" />
        </div>
      </div>

      {/* Hotbar */}
      <div className="hotbar-slots">
        <div className="hotbar-group">
          <div className="hotbar-slot has-item">
            <span className="hotbar-slot-number">1</span>
          </div>
          <div className="hotbar-slot has-item">
            <span className="hotbar-slot-number">2</span>
            <div className="equipment-slot-upgrade" />
          </div>
          <div className="hotbar-slot has-item">
            <span className="hotbar-slot-number">3</span>
            <div className="equipment-slot-upgrade" />
          </div>
          <div className="hotbar-slot">
            <span className="hotbar-slot-number">4</span>
          </div>
        </div>
        <div className="hotbar-divider" />
        <div className="hotbar-group">
          <div className="hotbar-slot">
            <span className="hotbar-slot-number">5</span>
          </div>
          <div className="hotbar-slot has-item">
            <span className="hotbar-slot-number">6</span>
            <div className="equipment-slot-upgrade" />
          </div>
          <div className="hotbar-slot">
            <span className="hotbar-slot-number">7</span>
          </div>
          <div className="hotbar-slot">
            <span className="hotbar-slot-number">8</span>
          </div>
        </div>
      </div>

      {/* Inventory Grid */}
      <div className="inventory-grid">
        {/* Row 1 */}
        <div className="inventory-slot has-item" />
        <div className="inventory-slot has-item" />
        <div className="inventory-slot has-item" />
        <div className="inventory-slot" />
        <div className="inventory-slot" />
        <div className="inventory-slot has-item" />
        <div className="inventory-slot has-item" />
        <div className="inventory-slot has-item" />
        {/* Row 2 */}
        <div className="inventory-slot has-item" />
        <div className="inventory-slot has-item" />
        <div className="inventory-slot has-item" />
        <div className="inventory-slot has-item" />
        <div className="inventory-slot" />
        <div className="inventory-slot" />
        <div className="inventory-slot" />
        <div className="inventory-slot" />
        {/* Row 3 */}
        <div className="inventory-slot" />
        <div className="inventory-slot" />
        <div className="inventory-slot" />
        <div className="inventory-slot" />
        <div className="inventory-slot" />
        <div className="inventory-slot" />
        <div className="inventory-slot" />
        <div className="inventory-slot" />
        {/* Row 4 - Locked */}
        <div className="inventory-slot locked" />
        <div className="inventory-slot locked" />
        <div className="inventory-slot locked" />
        <div className="inventory-slot locked" />
        <div className="inventory-slot locked" />
        <div className="inventory-slot locked" />
        <div className="inventory-slot locked" />
        <div className="inventory-slot locked" />
      </div>

      <div className="note">Equipment tab with Bloodcraft stats summary</div>
    </>
  );
}
