using Eclipse.Patches;
using Eclipse.Resources;
using Eclipse.Services.HUD.Shared;
using Eclipse.Utilities;
using ProjectM;
using ProjectM.UI;
using Stunlock.Core;
using StunShared.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Eclipse.Patches.InitializationPatches;
using static Eclipse.Services.CanvasService.ConfigureHUD;
using static Eclipse.Services.CanvasService.DataHUD;
using static Eclipse.Services.CanvasService.InitializeHUD;
using static Eclipse.Services.DataService;

namespace Eclipse.Services.HUD;

/// <summary>
/// Manages real-time HUD updates including progress bars, stats, abilities, and tab panels.
/// Extracted from CanvasService.UpdateHUD.
/// </summary>
internal static class HudUpdateManager
{
    #region Constants

    public static readonly PrefabGUID StatBuff = PrefabGUIDs.SetBonus_AllLeech_T09;
    public static readonly bool StatBuffActive = _legacyBar || _expertiseBar;
    const int StatBuffSlotCount = 6;

    #endregion

    #region Fields

    public static readonly HashSet<GameObject> AttributeObjects = [];
    public static GameObject _attributeObjectPrefab;

    public static HashSet<LocalizedText> CombinedAttributeTexts
        => [.. BloodAttributeTexts.Values.Concat(WeaponAttributeTexts.Values)];

    public static readonly Dictionary<UnitStatType, LocalizedText> BloodAttributeTexts = [];
    public static readonly Dictionary<UnitStatType, LocalizedText> WeaponAttributeTexts = [];

    static readonly HashSet<UnitStatType> SupportedAttributeBuffStats =
    [
        UnitStatType.MaxHealth,
        UnitStatType.PhysicalPower,
        UnitStatType.SpellPower,
        UnitStatType.MovementSpeed,
        UnitStatType.PrimaryAttackSpeed,
        UnitStatType.PhysicalCriticalStrikeChance,
        UnitStatType.PhysicalCriticalStrikeDamage,
        UnitStatType.SpellCriticalStrikeChance,
        UnitStatType.SpellCriticalStrikeDamage,
        UnitStatType.PrimaryLifeLeech,
        UnitStatType.PhysicalLifeLeech,
        UnitStatType.SpellLifeLeech,
        UnitStatType.MinionDamage,
        UnitStatType.DamageReduction,
        UnitStatType.HealingReceived,
        UnitStatType.ReducedBloodDrain,
        UnitStatType.ResourceYield,
        UnitStatType.WeaponCooldownRecoveryRate,
        UnitStatType.SpellCooldownRecoveryRate,
        UnitStatType.UltimateCooldownRecoveryRate
    ];

    public static readonly List<ModifyUnitStatBuff_DOTS> BloodStatBuffs = [default, default, default];
    public static readonly List<ModifyUnitStatBuff_DOTS> WeaponStatBuffs = [default, default, default];

    static EntityManager EntityManager => Core.EntityManager;
    static Entity LocalCharacter => Core.LocalCharacter;
    static BufferLookup<ModifyUnitStatBuff_DOTS> ModifyUnitStatBuffLookup
        => ClientChatSystemPatch.ModifyUnitStatBuffLookup;

    #endregion

    #region Attribute Updates

    public static void UpdateAttributeType(UnitStatType unitStatType, Sprite sprite)
    {
        if (BloodAttributeTexts.TryGetValue(unitStatType, out LocalizedText localizedText))
        {
            ConfigureAttributeType(localizedText.gameObject, sprite);
        }
        else if (WeaponAttributeTexts.TryGetValue(unitStatType, out localizedText))
        {
            ConfigureAttributeType(localizedText.gameObject, sprite);
        }
    }

    public static DynamicBuffer<ModifyUnitStatBuff_DOTS> TryGetSourceBuffer()
    {
        if (!ModifyUnitStatBuffLookup.TryGetBuffer(LocalCharacter, out var buffer))
            return default;

        return buffer;
    }

    public static void UpdateTargetBuffer(ref DynamicBuffer<ModifyUnitStatBuff_DOTS> sourceBuffer)
    {
        if (!sourceBuffer.IsCreated)
            return;

        if (!LocalCharacter.TryGetBuff(StatBuff, out Entity buff))
            return;

        if (!ModifyUnitStatBuffLookup.TryGetBuffer(buff, out var targetBuffer))
            return;

        targetBuffer.CopyFrom(sourceBuffer);
    }

    static void EnsureStatBufferSize(ref DynamicBuffer<ModifyUnitStatBuff_DOTS> buffer)
    {
        if (!buffer.IsCreated)
            return;

        if (buffer.Length != StatBuffSlotCount)
        {
            buffer.ResizeUninitialized(StatBuffSlotCount);

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = default;
            }
        }
    }

    public static void UpdateAttributes(ref DynamicBuffer<ModifyUnitStatBuff_DOTS> sourceBuffer)
    {
        if (!AttributesInitialized)
        {
            TryInitializeAttributeValues();

            if (AttributesInitialized && !ModifyUnitStatBuffLookup.TryGetBuffer(LocalCharacter, out var buffer))
            {
                buffer = EntityManager.AddBuffer<ModifyUnitStatBuff_DOTS>(LocalCharacter);

                buffer.EnsureCapacity(StatBuffSlotCount);
                buffer.ResizeUninitialized(StatBuffSlotCount);
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = default;
                }
            }

            return;
        }

        if (!sourceBuffer.IsCreated)
            return;

        foreach (var attributePair in BloodAttributeTexts)
        {
            attributePair.Value.ForceSet("");
        }

        foreach (var attributePair in WeaponAttributeTexts)
        {
            attributePair.Value.ForceSet("");
        }

        for (int i = 0; i < sourceBuffer.Length; i++)
        {
            ModifyUnitStatBuff_DOTS unitStatBuff = sourceBuffer[i];
            UnitStatType unitStatType = unitStatBuff.StatType;

            int identifier = unitStatBuff.Id.Id;
            float value = unitStatBuff.Value;

            if (identifier == 0 || value == 0 || float.IsNaN(value))
            {
                continue;
            }

            if (BloodAttributeTexts.TryGetValue(unitStatType, out LocalizedText localizedText))
            {
                string text = HudUtilities.FormatAttributeValue(unitStatType, value);
                if (text != localizedText.GetText())
                {
                    localizedText.ForceSet(text);
                }
            }
            else if (WeaponAttributeTexts.TryGetValue(unitStatType, out localizedText))
            {
                string text = HudUtilities.FormatAttributeValue(unitStatType, value);
                if (text != localizedText.GetText())
                {
                    localizedText.ForceSet(text);
                }
            }
        }
    }

    static bool IsSupportedAttributeBuffStat(UnitStatType unitStatType)
    {
        return SupportedAttributeBuffStats.Contains(unitStatType);
    }

    #endregion

    #region Stat Info Methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetWeaponStatInfo(int i, string statType)
    {
        WeaponStatBuffs[i] = default;
        if (Enum.TryParse(statType, out WeaponStatType weaponStatType))
        {
            if (_weaponStatValues.TryGetValue(weaponStatType, out float statValue))
            {
                float classMultiplier = HudUtilities.ClassSynergy(weaponStatType, _classType, _classStatSynergies);
                statValue *= (1 + (_prestigeStatMultiplier * _expertisePrestige)) * classMultiplier * ((float)_expertiseLevel / _expertiseMaxLevel);

                UnitStatType unitStatType = (UnitStatType)Enum.Parse(typeof(UnitStatType), weaponStatType.ToString());

                if (!IsSupportedAttributeBuffStat(unitStatType))
                {
                    WeaponStatBuffs[i] = default;
                    return HudUtilities.FormatWeaponStatBar(weaponStatType, statValue);
                }

                int statModificationId = ModificationIds.GenerateId(0, (int)weaponStatType, statValue);
                WeaponStatBuffs[i] = new()
                {
                    StatType = unitStatType,
                    ModificationType = ModificationType.Add,
                    Value = statValue,
                    Modifier = 1,
                    IncreaseByStacks = false,
                    ValueByStacks = 0,
                    Priority = 0,
                    Id = new(statModificationId)
                };

                return HudUtilities.FormatWeaponStatBar(weaponStatType, statValue);
            }
        }

        return string.Empty;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetBloodStatInfo(int i, string statType)
    {
        BloodStatBuffs[i] = default;
        if (Enum.TryParse(statType, out BloodStatType bloodStat))
        {
            if (_bloodStatValues.TryGetValue(bloodStat, out float statValue))
            {
                float classMultiplier = HudUtilities.ClassSynergy(bloodStat, _classType, _classStatSynergies);
                statValue *= (1 + (_prestigeStatMultiplier * _legacyPrestige)) * classMultiplier * ((float)_legacyLevel / _legacyMaxLevel);

                string displayString = $"<color=#00FFFF>{HudUtilities.BloodStatTypeAbbreviations[bloodStat]}</color>: <color=#90EE90>{(statValue * 100).ToString("F0") + "%"}</color>";
                UnitStatType unitStatType = (UnitStatType)Enum.Parse(typeof(UnitStatType), bloodStat.ToString());

                if (!IsSupportedAttributeBuffStat(unitStatType))
                {
                    BloodStatBuffs[i] = default;
                    return displayString;
                }

                int statModificationId = ModificationIds.GenerateId(1, (int)bloodStat, statValue);
                BloodStatBuffs[i] = new()
                {
                    StatType = unitStatType,
                    ModificationType = ModificationType.Add,
                    Value = statValue,
                    Modifier = 1,
                    IncreaseByStacks = false,
                    ValueByStacks = 0,
                    Priority = 0,
                    Id = new(statModificationId)
                };

                return displayString;
            }
        }

        return string.Empty;
    }

    #endregion

    #region Ability Updates

    public static void UpdateAbilityData(AbilityTooltipData abilityTooltipData, Entity abilityGroupEntity,
        Entity abilityCastEntity, PrefabGUID abilityGroupPrefabGUID)
    {
        if (!_abilityDummyObject.active)
        {
            _abilityDummyObject.SetActive(true);
            if (_uiState.CachedInputVersion != 3) _uiState.CachedInputVersion = 3;
        }

        if (!_keybindObject.active) _keybindObject.SetActive(true);

        _cooldownFillImage.fillAmount = 0f;
        _chargeCooldownFillImage.fillAmount = 0f;

        _abilityGroupPrefabGUID = abilityGroupPrefabGUID;

        _abilityBarEntry.AbilityEntity = abilityGroupEntity;
        _abilityBarEntry.AbilityId = abilityGroupPrefabGUID;
        _abilityBarEntry.AbilityIconImage.sprite = abilityTooltipData.Icon;

        _abilityBarEntry._CurrentUIState.AbilityIconImageActive = true;
        _abilityBarEntry._CurrentUIState.AbilityIconImageSprite = abilityTooltipData.Icon;

        if (abilityGroupEntity.TryGetComponent(out AbilityChargesData abilityChargesData))
        {
            _maxCharges = abilityChargesData.MaxCharges;
        }
        else
        {
            _maxCharges = 0;
            _currentCharges = 0;
            _chargesText.SetText("");
        }

        if (abilityCastEntity.TryGetComponent(out AbilityCooldownData abilityCooldownData))
        {
            _cooldownTime = _shiftSpellIndex.Equals(-1) ? abilityCooldownData.Cooldown._Value : _shiftSpellIndex * COOLDOWN_FACTOR + COOLDOWN_FACTOR;
            _cooldownEndTime = Core.ServerTime.TimeOnServer + _cooldownTime;
        }
    }

    public static void UpdateAbilityState(Entity abilityGroupEntity, Entity abilityCastEntity)
    {
        PrefabGUID prefabGuid = abilityGroupEntity.GetPrefabGUID();
        if (prefabGuid.HasValue() && !prefabGuid.Equals(_abilityGroupPrefabGUID)) return;

        if (abilityCastEntity.TryGetComponent(out AbilityCooldownState abilityCooldownState))
        {
            _cooldownEndTime = _shiftSpellIndex.Equals(-1) ? abilityCooldownState.CooldownEndTime : _cooldownEndTime;
        }

        _chargeUpTimeRemaining = (float)(_chargeUpEndTime - Core.ServerTime.TimeOnServer);
        _cooldownRemaining = (float)(_cooldownEndTime - Core.ServerTime.TimeOnServer);

        if (abilityGroupEntity.TryGetComponent(out AbilityChargesState abilityChargesState))
        {
            _currentCharges = abilityChargesState.CurrentCharges;
            _chargeUpTime = abilityChargesState.ChargeTime;
            _chargeUpEndTime = Core.ServerTime.TimeOnServer + _chargeUpTime;

            if (_currentCharges == 0)
            {
                _abilityBarEntry._CurrentUIState.ChargesTextActive = false;
                _chargeCooldownFillImage.fillAmount = 0f;
                _chargeCooldownImageObject.SetActive(false);

                _chargesText.SetText("");
                _cooldownText.SetText($"{(int)_chargeUpTime}");

                _cooldownFillImage.fillAmount = _chargeUpTime / _cooldownTime;
            }
            else
            {
                _abilityBarEntry._CurrentUIState.ChargesTextActive = true;
                _cooldownFillImage.fillAmount = 0f;

                _chargesTextObject.SetActive(true);
                _chargeCooldownImageObject.SetActive(true);

                _cooldownText.SetText("");
                _chargesText.SetText($"{_currentCharges}");

                _chargeCooldownFillImage.fillAmount = 1 - _cooldownRemaining / _cooldownTime;

                if (_currentCharges == _maxCharges) _chargeCooldownFillImage.fillAmount = 0f;
            }
        }
        else if (_maxCharges > 0)
        {
            if (_currentCharges == 0)
            {
                _abilityBarEntry._CurrentUIState.ChargesTextActive = true;
                _chargeCooldownFillImage.fillAmount = 0f;
                _chargeCooldownImageObject.SetActive(false);

                if (_chargeUpTimeRemaining < 0f)
                {
                    _cooldownText.SetText("");
                    _chargesText.SetText("1");
                }
                else
                {
                    _chargesText.SetText("");
                    _cooldownText.SetText($"{(int)_chargeUpTimeRemaining}");
                }

                _cooldownFillImage.fillAmount = _chargeUpTimeRemaining / _cooldownTime;

                if (_chargeUpTimeRemaining < 0f)
                {
                    ++_currentCharges;
                    _cooldownEndTime = Core.ServerTime.TimeOnServer + _cooldownTime;
                }
            }
            else if (_currentCharges < _maxCharges && _currentCharges > 0)
            {
                _cooldownText.SetText("");
                _abilityBarEntry._CurrentUIState.ChargesTextActive = true;
                _cooldownFillImage.fillAmount = 0f;

                _chargesTextObject.SetActive(true);
                _chargeCooldownImageObject.SetActive(true);

                _chargesText.SetText($"{_currentCharges}");

                _chargeCooldownFillImage.fillAmount = 1f - _cooldownRemaining / _cooldownTime;

                if (_cooldownRemaining < 0f)
                {
                    ++_currentCharges;
                    _cooldownEndTime = Core.ServerTime.TimeOnServer + _cooldownTime;
                }
            }
            else if (_currentCharges == _maxCharges)
            {
                _chargeCooldownImageObject.SetActive(false);

                _cooldownText.SetText("");
                _abilityBarEntry._CurrentUIState.ChargesTextActive = true;

                _cooldownFillImage.fillAmount = 0f;
                _chargeCooldownFillImage.fillAmount = 0f;

                _chargesTextObject.SetActive(true);
                _chargesText.SetText($"{_currentCharges}");
            }
        }
        else
        {
            _currentCharges = 0;
            _abilityBarEntry._CurrentUIState.ChargesTextActive = false;

            _chargeCooldownImageObject.SetActive(false);
            _chargeCooldownFillImage.fillAmount = 0f;

            if (_cooldownRemaining < 0f)
            {
                _cooldownText.SetText($"");
            }
            else
            {
                _cooldownText.SetText($"{(int)_cooldownRemaining}");
            }

            _cooldownFillImage.fillAmount = _cooldownRemaining / _cooldownTime;
        }
    }

    public static bool UpdateTooltipData(Entity abilityGroupEntity, PrefabGUID abilityGroupPrefabGUID)
    {
        if (_abilityTooltipData == null || _abilityGroupPrefabGUID != abilityGroupPrefabGUID)
        {
            if (abilityGroupEntity.TryGetComponentObject(EntityManager, out _abilityTooltipData))
            {
                _abilityTooltipData ??= EntityManager.GetComponentObject<AbilityTooltipData>(abilityGroupEntity, AbilityTooltipDataComponent);
            }
        }

        return _abilityTooltipData != null;
    }

    #endregion

    #region Profession Updates

    public static void UpdateProfessions(float progress, int level, LocalizedText levelText,
        Image progressFill, Image fill, Profession profession)
    {
        if (_killSwitch) return;

        if (level == MAX_PROFESSION_LEVEL)
        {
            progressFill.fillAmount = 1f;
            fill.fillAmount = 1f;
        }
        else
        {
            progressFill.fillAmount = progress;
            fill.fillAmount = level / MAX_PROFESSION_LEVEL;
        }
    }

    #endregion

    #region Progress Bar Updates

    public static void UpdateBar(float progress, int level, int maxLevel,
        int prestiges, LocalizedText levelText, LocalizedText barHeader,
        Image fill, UIElement element, string type = "")
    {
        if (_killSwitch) return;

        string levelString = level.ToString();

        if (type == "Frailed" || type == "Familiar")
        {
            levelString = "N/A";
        }

        if (level == maxLevel)
        {
            fill.fillAmount = 1f;
        }
        else
        {
            fill.fillAmount = progress;
        }

        if (levelText.GetText() != levelString)
        {
            levelText.ForceSet(levelString);
        }

        if (element.Equals(UIElement.Expertise))
        {
            type = HudUtilities.SplitPascalCase(type);
        }

        if (element.Equals(UIElement.Familiars))
        {
            type = HudUtilities.TrimToFirstWord(type);
        }

        if (barHeader.Text.fontSize != _horizontalBarHeaderFontSize)
        {
            barHeader.Text.fontSize = _horizontalBarHeaderFontSize;
        }

        if (_showPrestige && prestiges != 0)
        {
            string header = string.Empty;

            if (element.Equals(UIElement.Experience))
            {
                header = $"{element} {HudUtilities.IntegerToRoman(prestiges)}";
            }
            else if (element.Equals(UIElement.Legacy))
            {
                header = $"{type} {HudUtilities.IntegerToRoman(prestiges)}";
            }
            else if (element.Equals(UIElement.Expertise))
            {
                header = $"{type} {HudUtilities.IntegerToRoman(prestiges)}";
            }
            else if (element.Equals(UIElement.Familiars))
            {
                header = $"{type} {HudUtilities.IntegerToRoman(prestiges)}";
            }

            barHeader.ForceSet(header);
        }
        else if (!string.IsNullOrEmpty(type))
        {
            if (barHeader.GetText() != type)
            {
                barHeader.ForceSet(type);
            }
        }
    }

    public static void UpdateClass(PlayerClass classType, LocalizedText classText)
    {
        if (_killSwitch) return;

        if (classType != PlayerClass.None)
        {
            if (!classText.enabled) classText.enabled = true;
            if (!classText.gameObject.active) classText.gameObject.SetActive(true);

            string formattedClassName = HudUtilities.FormatClassName(classType);
            classText.ForceSet(formattedClassName);

            if (HudUtilities.ClassColorHexMap.TryGetValue(classType, out Color classColor))
            {
                classText.Text.color = classColor;
            }
        }
        else
        {
            classText.ForceSet(string.Empty);
            classText.enabled = false;
        }
    }

    #endregion

    #region Stat Updates

    public static void UpdateBloodStats(List<string> bonusStats, List<LocalizedText> statTexts,
        ref DynamicBuffer<ModifyUnitStatBuff_DOTS> buffer, Func<int, string, string> getStatInfo)
    {
        EnsureStatBufferSize(ref buffer);

        for (int i = 0; i < 3; i++)
        {
            string statKey = bonusStats[i];
            bool hasStat = !string.IsNullOrWhiteSpace(statKey)
                && !statKey.Equals("None", StringComparison.OrdinalIgnoreCase);
            if (hasStat)
            {
                if (!statTexts[i].enabled)
                    statTexts[i].enabled = true;

                if (!statTexts[i].gameObject.active)
                    statTexts[i].gameObject.SetActive(true);

                string statInfo = getStatInfo(i, statKey);
                if (string.IsNullOrWhiteSpace(statInfo))
                {
                    statTexts[i].ForceSet(string.Empty);
                    statTexts[i].enabled = false;

                    BloodStatBuffs[i] = default;
                    if (buffer.IsCreated)
                    {
                        buffer[i] = default;
                    }

                    continue;
                }

                statTexts[i].ForceSet(statInfo);

                if (buffer.IsCreated)
                {
                    buffer[i] = BloodStatBuffs[i].Id.Id != 0 ? BloodStatBuffs[i] : default;
                }
            }
            else
            {
                if (statTexts[i].enabled)
                {
                    statTexts[i].ForceSet(string.Empty);
                    statTexts[i].enabled = false;
                }

                BloodStatBuffs[i] = default;
                if (buffer.IsCreated)
                {
                    buffer[i] = default;
                }
            }
        }
    }

    public static void UpdateWeaponStats(List<string> bonusStats, List<LocalizedText> statTexts,
        ref DynamicBuffer<ModifyUnitStatBuff_DOTS> buffer, Func<int, string, string> getStatInfo)
    {
        EnsureStatBufferSize(ref buffer);

        for (int i = 0; i < 3; i++)
        {
            int j = i + 3; // Weapon stats -> second half of buffer

            string statKey = bonusStats[i];
            bool hasStat = !string.IsNullOrWhiteSpace(statKey)
                && !statKey.Equals("None", StringComparison.OrdinalIgnoreCase);
            if (hasStat)
            {
                if (!statTexts[i].enabled)
                    statTexts[i].enabled = true;

                if (!statTexts[i].gameObject.active)
                    statTexts[i].gameObject.SetActive(true);

                string statInfo = getStatInfo(i, statKey);
                if (string.IsNullOrWhiteSpace(statInfo))
                {
                    statTexts[i].ForceSet(string.Empty);
                    statTexts[i].enabled = false;

                    WeaponStatBuffs[i] = default;
                    if (buffer.IsCreated)
                    {
                        buffer[j] = default;
                    }

                    continue;
                }

                statTexts[i].ForceSet(statInfo);

                if (buffer.IsCreated)
                {
                    buffer[j] = WeaponStatBuffs[i].Id.Id != 0 ? WeaponStatBuffs[i] : default;
                }
            }
            else
            {
                if (statTexts[i].enabled)
                {
                    statTexts[i].ForceSet(string.Empty);
                    statTexts[i].enabled = false;
                }

                WeaponStatBuffs[i] = default;
                if (buffer.IsCreated)
                {
                    buffer[j] = default;
                }
            }
        }
    }

    public static void UpdateFamiliarStats(List<string> familiarStats, List<LocalizedText> statTexts)
    {
        if (_killSwitch) return;

        for (int i = 0; i < 3; i++)
        {
            if (!string.IsNullOrEmpty(familiarStats[i]))
            {
                if (!statTexts[i].enabled) statTexts[i].enabled = true;
                if (!statTexts[i].gameObject.active) statTexts[i].gameObject.SetActive(true);

                string statInfo = $"<color=#00FFFF>{HudUtilities.FamiliarStatStringAbbreviations[i]}</color>: <color=#90EE90>{familiarStats[i]}</color>";
                statTexts[i].ForceSet(statInfo);
            }
            else if (statTexts[i].enabled)
            {
                statTexts[i].ForceSet("");
                statTexts[i].enabled = false;
            }
        }
    }

    #endregion

    #region Quest Updates

    public static void UpdateQuests(GameObject questObject, LocalizedText questSubHeader, Image questIcon,
        TargetType targetType, string target, int progress, int goal, bool isVBlood)
    {
        if (_killSwitch) return;

        if (progress != goal && ObjectStates[questObject])
        {
            if (!questObject.gameObject.active) questObject.gameObject.active = true;

            if (targetType.Equals(TargetType.Kill))
            {
                target = HudUtilities.TrimToFirstWord(target);
            }
            else if (targetType.Equals(TargetType.Fish)) target = FISHING;

            questSubHeader.ForceSet($"<color=white>{target}</color>: {progress}/<color=yellow>{goal}</color>");

            switch (targetType)
            {
                case TargetType.Kill:
                    if (!questIcon.gameObject.active) questIcon.gameObject.active = true;
                    if (isVBlood && questIcon.sprite != _questKillVBloodUnit)
                    {
                        questIcon.sprite = _questKillVBloodUnit;
                    }
                    else if (!isVBlood && questIcon.sprite != _questKillStandardUnit)
                    {
                        questIcon.sprite = _questKillStandardUnit;
                    }
                    break;
                case TargetType.Craft:
                    if (!questIcon.gameObject.active) questIcon.gameObject.active = true;
                    PrefabGUID targetPrefabGUID = LocalizationService.GetPrefabGuidFromName(target);
                    ManagedItemData managedItemData = ManagedDataRegistry.GetOrDefault<ManagedItemData>(targetPrefabGUID);
                    if (managedItemData != null && questIcon.sprite != managedItemData.Icon)
                    {
                        questIcon.sprite = managedItemData.Icon;
                    }
                    break;
                case TargetType.Gather:
                    if (!questIcon.gameObject.active) questIcon.gameObject.active = true;
                    targetPrefabGUID = LocalizationService.GetPrefabGuidFromName(target);
                    if (target.Equals("Stone")) targetPrefabGUID = PrefabGUIDs.Item_Ingredient_Stone;
                    managedItemData = ManagedDataRegistry.GetOrDefault<ManagedItemData>(targetPrefabGUID);
                    if (managedItemData != null && questIcon.sprite != managedItemData.Icon)
                    {
                        questIcon.sprite = managedItemData.Icon;
                    }
                    break;
                case TargetType.Fish:
                    if (!questIcon.gameObject.active) questIcon.gameObject.active = true;
                    managedItemData = ManagedDataRegistry.GetOrDefault<ManagedItemData>(PrefabGUIDs.FakeItem_AnyFish);
                    if (managedItemData != null && questIcon.sprite != managedItemData.Icon)
                    {
                        questIcon.sprite = managedItemData.Icon;
                    }
                    break;
                default:
                    break;
            }
        }
        else
        {
            questObject.gameObject.active = false;
            questIcon.gameObject.active = false;
        }
    }

    static SystemService SystemService => Core.SystemService;
    static ManagedDataRegistry ManagedDataRegistry => SystemService.ManagedDataSystem.ManagedDataRegistry;

    #endregion

    #region Class Panel Updates

    public static void UpdateClassPanels()
    {
        if (_killSwitch || _classListObject == null || _classSpellsObject == null)
        {
            return;
        }

        if (!_classDataReady)
        {
            _classListSubHeader?.ForceSet("Awaiting class data...");
            ClearEntries(_classListEntries, _classListEntryButtons);
            _classSpellsSubHeader?.ForceSet(string.Empty);
            ClearEntries(_classSpellsEntries, _classSpellsEntryButtons);
            return;
        }

        if (!_classSystemEnabled)
        {
            _classListSubHeader?.ForceSet("Classes are disabled.");
            ClearEntries(_classListEntries, _classListEntryButtons);
            _classSpellsSubHeader?.ForceSet(string.Empty);
            ClearEntries(_classSpellsEntries, _classSpellsEntryButtons);
            return;
        }

        _classListHeader?.ForceSet("Classes");
        string classStatus = _classType == PlayerClass.None
            ? "Select a class."
            : $"Current: {HudUtilities.FormatClassName(_classType)}";
        _classListSubHeader?.ForceSet(classStatus);

        List<PlayerClass> classTypes = Enum.GetValues(typeof(PlayerClass))
            .Cast<PlayerClass>()
            .Where(playerClass => playerClass != PlayerClass.None)
            .ToList();

        EnsureEntries(_classListEntries, _classListEntryButtons, _classListEntriesRoot, _classListEntryTemplate, classTypes.Count, "ClassEntry");

        if (_classListEntries.Count < classTypes.Count)
        {
            return;
        }

        for (int i = 0; i < classTypes.Count; i++)
        {
            PlayerClass playerClass = classTypes[i];
            int classIndex = i + 1;

            LocalizedText entryText = _classListEntries[i];
            entryText.ForceSet($"{classIndex} | {HudUtilities.FormatClassName(playerClass)}");

            if (HudUtilities.ClassColorHexMap.TryGetValue(playerClass, out Color classColor))
            {
                entryText.Text.color = classColor;
            }

            entryText.Text.fontStyle = _classType == playerClass ? FontStyles.Bold : FontStyles.Normal;

            string command = _classType == PlayerClass.None
                ? $".class s {classIndex}"
                : $".class c {classIndex}";

            ConfigureCommandButton(_classListEntryButtons[i], command, true);
        }

        if (classTypes.Count == 0)
        {
            return;
        }

        PlayerClass spellsClass = _classType == PlayerClass.None ? classTypes[0] : _classType;
        _classSpellsHeader?.ForceSet("Class Spells");
        string shiftStatus = _classShiftSlotEnabled ? "Shift ready" : "Shift disabled";
        _classSpellsSubHeader?.ForceSet($"{HudUtilities.FormatClassName(spellsClass)} spells ({shiftStatus})");

        _classSpells.TryGetValue(spellsClass, out List<int> classSpells);
        classSpells ??= [];

        List<(int Index, int SpellId)> spellEntries = [];

        if (_defaultClassSpell != 0)
        {
            spellEntries.Add((0, _defaultClassSpell));
        }

        for (int i = 0; i < classSpells.Count; i++)
        {
            spellEntries.Add((i + 1, classSpells[i]));
        }

        EnsureEntries(_classSpellsEntries, _classSpellsEntryButtons, _classSpellsEntriesRoot, _classSpellsEntryTemplate, spellEntries.Count, "SpellEntry");

        if (_classSpellsEntries.Count < spellEntries.Count)
        {
            return;
        }

        int activeIndex = _shiftSpellIndex >= 0 ? _shiftSpellIndex + 1 : -1;

        for (int i = 0; i < spellEntries.Count; i++)
        {
            var (index, spellId) = spellEntries[i];
            PrefabGUID spellPrefab = new(spellId);
            string spellName = spellPrefab.GetLocalizedName();
            if (string.IsNullOrEmpty(spellName) || spellName.Equals("LocalizationKey.Empty"))
            {
                spellName = spellPrefab.GetPrefabName();
            }

            int requiredPrestige = index < _classSpellUnlockLevels.Count ? _classSpellUnlockLevels[index] : 0;
            string requirement = requiredPrestige > 0 ? $" (P{requiredPrestige})" : string.Empty;

            LocalizedText entryText = _classSpellsEntries[i];
            entryText.ForceSet($"{index} | {spellName}{requirement}");
            entryText.Text.fontStyle = index == activeIndex ? FontStyles.Bold : FontStyles.Normal;

            bool canUse = _classShiftSlotEnabled;
            ConfigureCommandButton(_classSpellsEntryButtons[i], $".class csp {index}", canUse);
        }
    }

    #endregion

    #region Tab Panel Updates

    public static void UpdateTabPanels()
    {
        if (_killSwitch || _tabsNavObject == null || _tabsContentObject == null)
        {
            return;
        }

        UpdateTabNavigation();
        UpdateTabContent();
    }

    static void UpdateTabNavigation()
    {
        _tabsNavHeader?.ForceSet("Eclipse");
        _tabsNavSubHeader?.ForceSet("Select a tab.");

        EnsureEntries(_tabsNavEntries, _tabsNavEntryButtons, _tabsNavEntriesRoot, _tabsNavEntryTemplate, TabOrder.Count, "Tab");

        if (_tabsNavEntries.Count < TabOrder.Count)
        {
            return;
        }

        for (int i = 0; i < TabOrder.Count; i++)
        {
            TabType tab = TabOrder[i];
            TabType capturedTab = tab;
            string label = TabLabels.TryGetValue(tab, out string tabLabel) ? tabLabel : tab.ToString();

            LocalizedText entryText = _tabsNavEntries[i];
            entryText.Text.color = Color.white;
            entryText.Text.fontStyle = _activeTab == tab ? FontStyles.Bold : FontStyles.Normal;
            entryText.ForceSet($"{i + 1} | {label}");

            ConfigureActionButton(_tabsNavEntryButtons[i], () => _activeTab = capturedTab, true);
        }
    }

    static void UpdateTabContent()
    {
        switch (_activeTab)
        {
            case TabType.Exoform:
                UpdateExoFormTab();
                break;
            case TabType.Battles:
                UpdateFamiliarBattleTab();
                break;
            default:
                UpdatePrestigeTab();
                break;
        }
    }

    static void UpdatePrestigeTab()
    {
        _tabsContentHeader?.ForceSet("Prestige Leaderboard");

        if (!_prestigeDataReady)
        {
            _tabsContentSubHeader?.ForceSet("Awaiting prestige data...");
            ClearEntries(_tabsContentEntries, _tabsContentEntryButtons);
            return;
        }

        if (!_prestigeSystemEnabled)
        {
            _tabsContentSubHeader?.ForceSet("Prestige system disabled.");
            ClearEntries(_tabsContentEntries, _tabsContentEntryButtons);
            return;
        }

        if (!_prestigeLeaderboardEnabled)
        {
            _tabsContentSubHeader?.ForceSet("Prestige leaderboard disabled.");
            ClearEntries(_tabsContentEntries, _tabsContentEntryButtons);
            return;
        }

        if (_prestigeLeaderboardOrder.Count == 0)
        {
            _tabsContentSubHeader?.ForceSet("No prestige data available.");
            ClearEntries(_tabsContentEntries, _tabsContentEntryButtons);
            return;
        }

        if (_prestigeLeaderboardIndex >= _prestigeLeaderboardOrder.Count)
        {
            _prestigeLeaderboardIndex = 0;
        }

        string typeKey = _prestigeLeaderboardOrder[_prestigeLeaderboardIndex];
        string displayType = HudUtilities.SplitPascalCase(typeKey);
        _tabsContentSubHeader?.ForceSet("Click type to cycle.");

        _prestigeLeaderboards.TryGetValue(typeKey, out List<PrestigeLeaderboardEntry> leaderboard);
        leaderboard ??= [];

        List<TabEntry> entries =
        [
            new TabEntry($"Type: {displayType}", FontStyles.Bold, action: CyclePrestigeType, enabled: _prestigeLeaderboardOrder.Count > 1)
        ];

        if (leaderboard.Count == 0)
        {
            entries.Add(new TabEntry("No prestige entries yet.", FontStyles.Normal));
        }
        else
        {
            for (int i = 0; i < leaderboard.Count; i++)
            {
                PrestigeLeaderboardEntry entry = leaderboard[i];
                FontStyles style = i == 0 ? FontStyles.Bold : FontStyles.Normal;
                entries.Add(new TabEntry($"{i + 1} | {entry.Name}: {entry.Value}", style));
            }
        }

        ApplyTabEntries(entries, "PrestigeEntry");
    }

    static void UpdateExoFormTab()
    {
        _tabsContentHeader?.ForceSet("Exoforms");

        if (!_exoFormDataReady)
        {
            _tabsContentSubHeader?.ForceSet("Awaiting exoform data...");
            ClearEntries(_tabsContentEntries, _tabsContentEntryButtons);
            return;
        }

        if (!_exoFormEnabled)
        {
            _tabsContentSubHeader?.ForceSet("Exo prestiging disabled.");
            ClearEntries(_tabsContentEntries, _tabsContentEntryButtons);
            return;
        }

        string currentForm = string.IsNullOrWhiteSpace(_exoFormCurrentForm)
            ? "None"
            : HudUtilities.SplitPascalCase(_exoFormCurrentForm);

        _tabsContentSubHeader?.ForceSet($"Current: {currentForm}");

        bool canToggleTaunt = _exoFormPrestiges > 0;
        string chargeLine = _exoFormMaxDuration > 0f
            ? $"Charge: {_exoFormCharge:0.0}/{_exoFormMaxDuration:0.0}s"
            : "Charge: --";

        string tauntStatus = _exoFormTauntEnabled ? "<color=green>On</color>" : "<color=red>Off</color>";

        List<TabEntry> entries =
        [
            new TabEntry($"Exo Prestiges: {_exoFormPrestiges}", FontStyles.Normal),
            new TabEntry(chargeLine, FontStyles.Normal),
            new TabEntry($"Taunt to Exoform: {tauntStatus}", FontStyles.Normal, command: ".prestige exoform", enabled: canToggleTaunt),
            new TabEntry($"Current Form: {currentForm}", FontStyles.Normal),
            new TabEntry("Forms", FontStyles.Bold)
        ];

        for (int i = 0; i < _exoFormEntries.Count; i++)
        {
            ExoFormEntry form = _exoFormEntries[i];
            string formName = HudUtilities.SplitPascalCase(form.FormName);
            string status = form.Unlocked ? "Unlocked" : "Locked";
            FontStyles style = form.FormName.Equals(_exoFormCurrentForm, StringComparison.OrdinalIgnoreCase)
                ? FontStyles.Bold
                : FontStyles.Normal;

            entries.Add(new TabEntry($"{i + 1} | {formName} ({status})", style,
                command: $".prestige sf {form.FormName}", enabled: form.Unlocked));
        }

        ExoFormEntry activeForm = ResolveActiveExoForm();
        if (activeForm != null && activeForm.Abilities.Count > 0)
        {
            entries.Add(new TabEntry("Abilities", FontStyles.Bold));

            foreach (ExoFormAbilityData ability in activeForm.Abilities)
            {
                string abilityName = ResolveAbilityName(ability.AbilityId);
                entries.Add(new TabEntry($" - {abilityName} ({ability.Cooldown:0.0}s)", FontStyles.Normal));
            }
        }

        ApplyTabEntries(entries, "ExoformEntry");
    }

    static void UpdateFamiliarBattleTab()
    {
        _tabsContentHeader?.ForceSet("Familiar Battles");

        if (!_familiarBattleDataReady)
        {
            _tabsContentSubHeader?.ForceSet("Awaiting familiar battle data...");
            ClearEntries(_tabsContentEntries, _tabsContentEntryButtons);
            return;
        }

        if (!_familiarSystemEnabled)
        {
            _tabsContentSubHeader?.ForceSet("Familiars are disabled.");
            ClearEntries(_tabsContentEntries, _tabsContentEntryButtons);
            return;
        }

        if (!_familiarBattlesEnabled)
        {
            _tabsContentSubHeader?.ForceSet("Familiar battles disabled.");
            ClearEntries(_tabsContentEntries, _tabsContentEntryButtons);
            return;
        }

        string activeGroupName = string.IsNullOrWhiteSpace(_familiarActiveBattleGroup)
            ? "None"
            : _familiarActiveBattleGroup;

        _tabsContentSubHeader?.ForceSet($"Active group: {activeGroupName}");

        List<TabEntry> entries = [];

        if (_familiarBattleGroups.Count == 0)
        {
            entries.Add(new TabEntry("No battle groups available.", FontStyles.Normal));
            ApplyTabEntries(entries, "FamiliarEntry");
            return;
        }

        entries.Add(new TabEntry("Groups", FontStyles.Bold));

        for (int i = 0; i < _familiarBattleGroups.Count; i++)
        {
            FamiliarBattleGroupData group = _familiarBattleGroups[i];
            bool isActive = group.Name.Equals(_familiarActiveBattleGroup, StringComparison.OrdinalIgnoreCase);
            FontStyles style = isActive ? FontStyles.Bold : FontStyles.Normal;
            string suffix = isActive ? " (Active)" : string.Empty;
            entries.Add(new TabEntry($"{i + 1} | {group.Name}{suffix}", style,
                command: $".fam cbg {group.Name}", enabled: true));
        }

        FamiliarBattleGroupData activeGroup = FindBattleGroup(_familiarActiveBattleGroup) ?? _familiarBattleGroups[0];
        entries.Add(new TabEntry("Slots", FontStyles.Bold));

        for (int i = 0; i < activeGroup.Slots.Count; i++)
        {
            FamiliarBattleSlotData slot = activeGroup.Slots[i];
            string slotText = FormatFamiliarSlot(slot, i + 1);
            entries.Add(new TabEntry(slotText, FontStyles.Normal, command: $".fam sbg {i + 1}", enabled: true));
        }

        ApplyTabEntries(entries, "FamiliarEntry");
    }

    static void CyclePrestigeType()
    {
        if (_prestigeLeaderboardOrder.Count == 0)
        {
            return;
        }

        _prestigeLeaderboardIndex = (_prestigeLeaderboardIndex + 1) % _prestigeLeaderboardOrder.Count;
    }

    static ExoFormEntry ResolveActiveExoForm()
    {
        if (_exoFormEntries.Count == 0)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(_exoFormCurrentForm))
        {
            ExoFormEntry match = _exoFormEntries.FirstOrDefault(entry =>
                entry.FormName.Equals(_exoFormCurrentForm, StringComparison.OrdinalIgnoreCase));

            if (match != null)
            {
                return match;
            }
        }

        return _exoFormEntries[0];
    }

    static string ResolveAbilityName(int abilityId)
    {
        PrefabGUID abilityGuid = new(abilityId);
        string abilityName = abilityGuid.GetLocalizedName();
        if (string.IsNullOrEmpty(abilityName) || abilityName.Equals("LocalizationKey.Empty"))
        {
            abilityName = abilityGuid.GetPrefabName();
        }

        if (string.IsNullOrEmpty(abilityName))
        {
            return $"Ability {abilityId}";
        }

        Match match = AbilitySpellRegex.Match(abilityName);
        if (match.Success)
        {
            return match.Value.Replace('_', ' ');
        }

        return abilityName;
    }

    static FamiliarBattleGroupData FindBattleGroup(string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName))
        {
            return null;
        }

        return _familiarBattleGroups.FirstOrDefault(group =>
            group.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase));
    }

    static string FormatFamiliarSlot(FamiliarBattleSlotData slot, int slotIndex)
    {
        if (slot.Id == 0)
        {
            return $"Slot {slotIndex}: <color=grey>Empty</color>";
        }

        string name = string.IsNullOrWhiteSpace(slot.Name) ? $"Familiar {slot.Id}" : slot.Name;
        string prestige = slot.Prestige > 0 ? $" P{slot.Prestige}" : string.Empty;
        return $"Slot {slotIndex}: {name} (Lv {slot.Level}{prestige})";
    }

    static void ApplyTabEntries(List<TabEntry> entries, string namePrefix)
    {
        EnsureEntries(_tabsContentEntries, _tabsContentEntryButtons, _tabsContentEntriesRoot, _tabsContentEntryTemplate, entries.Count, namePrefix);

        if (_tabsContentEntries.Count < entries.Count)
        {
            return;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            TabEntry entry = entries[i];
            LocalizedText entryText = _tabsContentEntries[i];

            entryText.Text.color = Color.white;
            entryText.Text.fontStyle = entry.Style;
            entryText.ForceSet(entry.Text);

            if (entry.Action != null)
            {
                ConfigureActionButton(_tabsContentEntryButtons[i], entry.Action, entry.Enabled);
            }
            else
            {
                ConfigureCommandButton(_tabsContentEntryButtons[i], entry.Command, entry.Enabled);
            }
        }
    }

    readonly struct TabEntry
    {
        public string Text { get; }
        public string Command { get; }
        public Action Action { get; }
        public bool Enabled { get; }
        public FontStyles Style { get; }

        public TabEntry(string text, FontStyles style, string command = "", Action action = null, bool enabled = false)
        {
            Text = text;
            Command = command;
            Action = action;
            Enabled = enabled;
            Style = style;
        }
    }

    #endregion

    #region Entry Helpers

    static void ClearEntries(List<LocalizedText> entries, List<SimpleStunButton> buttons)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            entries[i].ForceSet(string.Empty);
            entries[i].gameObject.SetActive(false);

            if (i < buttons.Count)
            {
                ConfigureCommandButton(buttons[i], string.Empty, false);
            }
        }
    }

    static void EnsureEntries(List<LocalizedText> entries, List<SimpleStunButton> buttons, Transform root,
        GameObject template, int count, string namePrefix)
    {
        if (root == null || template == null)
        {
            return;
        }

        while (entries.Count < count)
        {
            int index = entries.Count;
            GameObject entryObject = UnityEngine.Object.Instantiate(template, root, false);
            entryObject.name = $"{namePrefix}_{index + 1}";
            entryObject.SetActive(true);

            LocalizedText localizedText = entryObject.GetComponent<LocalizedText>();
            if (!TryBindLocalizedText(localizedText, $"{namePrefix}_{index + 1}"))
            {
                UnityEngine.Object.Destroy(entryObject);
                return;
            }
            localizedText.Text.enableAutoSizing = false;
            localizedText.Text.enableWordWrapping = false;
            localizedText.Text.raycastTarget = true;

            ApplyTransparentGraphic(entryObject, $"{namePrefix}_{index + 1}");

            SimpleStunButton button = entryObject.GetComponent<SimpleStunButton>() ?? entryObject.AddComponent<SimpleStunButton>();

            entries.Add(localizedText);
            buttons.Add(button);
        }

        for (int i = 0; i < entries.Count; i++)
        {
            bool isActive = i < count;
            entries[i].gameObject.SetActive(isActive);
        }
    }

    static void ConfigureCommandButton(SimpleStunButton button, string command, bool enabled)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();

        if (!enabled || string.IsNullOrWhiteSpace(command))
        {
            return;
        }

        button.onClick.AddListener((UnityAction)(() => Quips.SendCommand(command)));
    }

    static void ConfigureActionButton(SimpleStunButton button, Action action, bool enabled)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();

        if (!enabled || action == null)
        {
            return;
        }

        button.onClick.AddListener((UnityAction)(() => action()));
    }

    #endregion

    #region Reset

    public static void Reset()
    {
        AttributeObjects.Clear();
        _attributeObjectPrefab = null;
        BloodAttributeTexts.Clear();
        WeaponAttributeTexts.Clear();

        for (int i = 0; i < BloodStatBuffs.Count; i++)
        {
            BloodStatBuffs[i] = default;
        }

        for (int i = 0; i < WeaponStatBuffs.Count; i++)
        {
            WeaponStatBuffs[i] = default;
        }
    }

    #endregion
}
