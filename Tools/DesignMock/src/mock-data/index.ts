import type {
  StatRow,
  Profession,
  ClassInfo,
  ClassSpell,
  FamiliarInfo,
  BattleGroupSlot,
  PrestigeEntry,
  TalentNode,
} from '../shared/types';

// Weapon Expertise stats
export const weaponStats: StatRow[] = [
  { name: 'Max Health', value: '+168', selected: true },
  { name: 'Movement Speed', value: '+0%' },
  { name: 'Primary Attack Speed', value: '+0%' },
  { name: 'Physical Life Leech', value: '+0%' },
  { name: 'Spell Life Leech', value: '+0%' },
  { name: 'Primary Life Leech', value: '+10%', selected: true },
  { name: 'Physical Power', value: '+13', selected: true },
  { name: 'Spell Power', value: '+0%' },
  { name: 'Physical Crit Chance', value: '+0%' },
  { name: 'Physical Crit Damage', value: '+0%' },
  { name: 'Spell Crit Chance', value: '+0%' },
  { name: 'Spell Crit Damage', value: '+0%' },
];

// Blood Legacies stats
export const bloodStats: StatRow[] = [
  { name: 'Healing Received', value: '+8%', selected: true },
  { name: 'Damage Reduction', value: '+5%', selected: true },
  { name: 'Physical Resistance', value: '+0%' },
  { name: 'Spell Resistance', value: '+0%' },
  { name: 'Resource Yield', value: '+0%' },
  { name: 'Reduced Blood Drain', value: '+0%' },
  { name: 'Spell Cooldown Recovery', value: '+0%' },
  { name: 'Weapon Cooldown Recovery', value: '+0%' },
  { name: 'Ultimate Cooldown Recovery', value: '+0%' },
  { name: 'Minion Damage', value: '+0%' },
  { name: 'Ability Attack Speed', value: '+0%' },
  { name: 'Corruption Damage Reduction', value: '+0%' },
];

// Professions
export const professions: Profession[] = [
  { name: 'Enchanting', color: '#a46bd4', level: 0, progress: 0 },
  { name: 'Alchemy', color: '#37c4a1', level: 0, progress: 0 },
  { name: 'Harvesting', color: '#4fd154', level: 4, progress: 97 },
  { name: 'Blacksmithing', color: '#7c8791', level: 0, progress: 0 },
  { name: 'Tailoring', color: '#e07841', level: 0, progress: 0 },
  { name: 'Woodcutting', color: '#c08a3a', level: 6, progress: 15 },
  { name: 'Mining', color: '#9aa0a8', level: 8, progress: 58 },
  { name: 'Fishing', color: '#2aa3d6', level: 0, progress: 0 },
];

// Classes
export const classes: ClassInfo[] = [
  { name: 'Blood Knight', color: '#ff0000', active: true },
  { name: 'Demon Hunter', color: '#ffcc00' },
  { name: 'Vampire Lord', color: '#00ffff' },
  { name: 'Shadow Blade', color: '#9933ff' },
  { name: 'Arcane Sorcerer', color: '#008080' },
  { name: 'Death Mage', color: '#00ff00' },
];

// Class Spells
export const classSpells: ClassSpell[] = [
  { index: 0, name: 'Crimson Aegis', active: true },
  { index: 1, name: 'Blood Rite', requirement: '(P1)' },
  { index: 2, name: 'Sanguine Coil', requirement: '(P2)' },
  { index: 3, name: 'Bloodstorm', requirement: '(P5)', locked: true },
];

// Familiar Boxes
export const familiarBoxes: FamiliarInfo[] = [
  { name: 'Skeleton Crossbow', level: 1, active: true },
  { name: 'Wolf', level: 1 },
  { name: 'Thug', level: 1 },
  { name: 'Poacher', level: 1 },
  { name: 'Skeleton', level: 1 },
  { name: 'Scout', level: 3 },
  { name: '', level: 0, empty: true },
  { name: '', level: 0, empty: true },
  { name: '', level: 0, empty: true },
  { name: '', level: 0, empty: true },
];

// Overflow familiars
export const overflowFamiliars: FamiliarInfo[] = [
  { name: 'Overflow #1 — Bandit Trapper *', level: 24, active: true },
  { name: 'Overflow #2 — Skeleton', level: 12 },
];

// Battle Group Slots
export const battleGroupSlots: BattleGroupSlot[] = [
  { slot: 1, name: 'Scout', level: 7 },
  { slot: 2, name: '(empty)', empty: true },
  { slot: 3, name: 'Skeleton', level: 12 },
];

// Prestige Leaderboard
export const prestigeLeaderboard: PrestigeEntry[] = [
  { rank: 1, name: 'Vlad the Enjoyer', value: 98, top: true },
  { rank: 2, name: 'Nightstalker', value: 67 },
  { rank: 3, name: 'Crimson Baron', value: 55 },
  { rank: 4, name: 'Mournful Shade', value: 42 },
  { rank: 5, name: 'Ravenous', value: 31 },
];

// Talent Nodes
export const speedTalents: TalentNode[] = [
  { id: 1, tier: 'minor', available: true },
  { id: 2, tier: 'minor' },
  { id: 3, tier: 'notable' },
  { id: 4, tier: 'keystone' },
];

export const powerTalents: TalentNode[] = [
  { id: 10, tier: 'minor', available: true },
  { id: 11, tier: 'minor' },
  { id: 12, tier: 'notable' },
  { id: 13, tier: 'keystone', enrage: true },
];

export const vitalityTalents: TalentNode[] = [
  { id: 20, tier: 'minor', available: true },
  { id: 21, tier: 'minor' },
  { id: 22, tier: 'notable' },
  { id: 23, tier: 'keystone' },
];

// Shiny buff types
export const shinyBuffs = ['Blood', 'Storm', 'Chaos', 'Frost', 'Illusion', 'Unholy'];

// Box options
export const boxOptions = ['Box 1 - Forest', 'Box 2 - Highlands', 'Box 3 - Crypt', 'Box 4 - Swamp'];

// Battle group options
export const battleGroupOptions = ['arena', 'duels', 'pve'];

// Prestige type options
export const prestigeTypes = ['Experience', 'Weapon', 'Blood', 'Familiar', 'Professions'];

// Players for challenge
export const players = ['PlayerOne', 'PlayerTwo', 'PlayerThree'];
