// Main character menu tabs
export type MainTab = 'equipment' | 'crafting' | 'bloodpool' | 'attributes' | 'bloodcraft';

// Bloodcraft sub-tabs
export type BloodcraftTab = 'prestige' | 'exoform' | 'bonuses' | 'progression' | 'familiars';

// Stat Bonuses system type
export type StatBonusType = 'weapon' | 'blood';

// Familiars mode tabs
export type FamiliarsMode = 'manage' | 'battles' | 'talents';

// Progression mode tabs
export type ProgressionMode = 'professions' | 'class';

// View models for mock data
export interface StatRow {
  name: string;
  value: string;
  selected?: boolean;
}

export interface Profession {
  name: string;
  color: string;
  level: number;
  progress: number;
}

export interface ClassInfo {
  name: string;
  color: string;
  active?: boolean;
}

export interface ClassSpell {
  index: number;
  name: string;
  requirement?: string;
  locked?: boolean;
  active?: boolean;
}

export interface FamiliarInfo {
  name: string;
  level: number;
  active?: boolean;
  empty?: boolean;
}

export interface BattleGroupSlot {
  slot: number;
  name: string;
  level?: number;
  empty?: boolean;
}

export interface PrestigeEntry {
  rank: number;
  name: string;
  value: number;
  top?: boolean;
}

export interface TalentNode {
  id: number;
  tier: 'minor' | 'notable' | 'keystone';
  allocated?: boolean;
  available?: boolean;
  enrage?: boolean;
}
