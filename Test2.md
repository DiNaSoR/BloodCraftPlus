# User Feedback Analysis (from Test.md)

Based on the analysis of the chat logs, here are the identified bugs and feature requests from the community.

## 🐛 Reported Bugs & Issues

1.  **Familiar Stats & Scaling**
    *   **Glitch:** HP and Power (PP) sometimes display incorrectly or become extremely high/wrong (Lines 2-4, 56-58).
    *   **Zero Stats:** In some cases, familiar stats show as zero after selection (Line 22).
    *   **Skeleton Scaling:** The skeleton familiar scaling seems broken or stays at 1 damage (Line 150).

2.  **Rift System (T3/Primal)**
    *   **Spawning Logic:** "Elite Primal Rifts" (T3) sometimes override all other rifts or fail to stop when toggled (Lines 689-768).
    *   **Level Discrepancy:** T3 Rifts sometimes spawn at lower levels (e.g., 90-94) despite settings for level 100/110+ (Lines 865-920).
    *   **Config Confusion:** Users are confused by `RiftFrequency` settings (0 vs even numbers) (Line 694).

3.  **Stats & Calculations**
    *   **Bloodmoon Spike:** Players experience massively inflated stats (e.g., Spell Power) during Bloodmoon, potentially linked to "Blood Efficiency" or items like Dracula's Pendant (Lines 547, 657-671).
    *   **Prestige Sync:** UI stats in Eclipse do not always reflect Prestige bonuses immediately or clearly, causing confusion about whether bonuses are applied (Lines 790-844).
    *   **Leveling Glitch:** Players reported incorrect levels (e.g., lvl 178) or base levels sticking (e.g., lvl 90 unarmed) even after disabling leveling (Lines 209-285).

4.  **Other Technical Issues**
    *   **Controller Support:** Players using controllers cannot invoke abilities properly (Lines 94, 122).
    *   **Mod Conflicts:** Issues with `ServerLaunchFix` (SLF) needing wait times (Lines 104, 614), and `Kindred` commands blocking 100% Immortal Blood spawns (Lines 672-677).
    *   **Repairing:** Inability to repair gear after updates (Lines 69-70).

## 💡 Feature Requests & Improvements

1.  **Quality of Life (QoL)**
    *   **UI Overhaul:** Strong desire for a proper GUI to replace chat commands for accessibility (Line 452).
    *   **Familiar Management:** Request for a separate "box" or storage system to organize large collections of familiars (Line 194).
    *   **Documentation:** Repeated requests for a Wiki, clearer guides on Prestige, Classes, and Configs (Lines 160, 179, 197).

2.  **Gameplay & Balance**
    *   **Catch-up Mechanics:** Request for EXP bonuses for new players joining established servers to catch up to high-level friends (Lines 73, 467).
    *   **Leveling Control:** Option to "freeze" EXP or toggle leveling off for individual players to avoid out-leveling friends (Lines 464-465).
    *   **Config Presets:** Users asked for "recommended" or balanced config files, finding it hard to tune the difficulty themselves (Line 454).
    *   **Class Balance:** specific interest in balancing Physical vs Spell builds and feedback on Death Mage viability (Lines 320-355, 481).
