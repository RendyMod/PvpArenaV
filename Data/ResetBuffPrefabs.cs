using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectM;

namespace PvpArena.Data;
public static class ResetBuffPrefabs
{
	public static HashSet<PrefabGUID> BuffsToKeep = new HashSet<PrefabGUID>
	{
		Prefabs.SetBonus_AttackSpeed_Minor_Buff_02,
		Prefabs.SetBonus_AttackSpeed_Minor_Buff_01,
		Prefabs.AllowJumpFromCliffsBuff,
		Prefabs.Item_EquipBuff_MagicSource_General,
		Prefabs.Item_EquipBuff_Shared_General,
		Prefabs.EquipBuff_Cloak_Base,
		Prefabs.EquipBuff_Boots_Base,
		Prefabs.EquipBuff_Chest_Base,
		Prefabs.EquipBuff_Gloves_Base,
		Prefabs.EquipBuff_Headgear_Base,
		Prefabs.EquipBuff_Legs_Base,
		Prefabs.SetBonus_Speed_Minor_Buff_02,
		Prefabs.SetBonus_Speed_Minor_Buff_01,
		Prefabs.SetBonus_MaxHealth_Minor_Buff_02,
		Prefabs.SetBonus_MaxHealth_Minor_Buff_01,
		Prefabs.SetBonus_Damage_Minor_Buff_01,
		Prefabs.SetBonus_GearLevel_Minor_Buff_02,
		Prefabs.SetBonus_GearLevel_Minor_Buff_01,
		Prefabs.JumpFromCliffs_AvoidDoubleJump,
		Prefabs.JumpFromCliffs_TriggerLeapUpAbility,
		Prefabs.EquipBuff_ShroudOfTheForest,
		Prefabs.SetBonus_Silk_Twilight,
		Prefabs.JumpFromCliffs_Landing,
		Prefabs.EquipBuff_Weapon_GreatSword_Ability03,
		Prefabs.EquipBuff_Weapon_GreatSword_Ability02,
		Prefabs.EquipBuff_Weapon_GreatSword_Ability01,
		Prefabs.EquipBuff_Weapon_FishingPole_Debug,
		Prefabs.EquipBuff_Weapon_FishingPole_Base,
		Prefabs.EquipBuff_Weapon_Rapier_Base,
		Prefabs.EquipBuff_Weapon_Rapier_Ability03,
		Prefabs.EquipBuff_Weapon_Rapier_Ability02,
		Prefabs.EquipBuff_Weapon_Rapier_Ability01,
		Prefabs.EquipBuff_Weapon_Pistols_Base,
		Prefabs.EquipBuff_Weapon_Pistols_Ability03,
		Prefabs.EquipBuff_Weapon_Pistols_Ability02,
		Prefabs.EquipBuff_Weapon_Pistols_Ability01,
		Prefabs.EquipBuff_Weapon_NecromancyDagger_Base,
		Prefabs.EquipBuff_Weapon_Mace_Base,
		Prefabs.EquipBuff_Weapon_Mace_Ability03,
		Prefabs.EquipBuff_Weapon_Mace_Ability02,
		Prefabs.EquipBuff_Weapon_Mace_Ability01,
		Prefabs.EquipBuff_Weapon_Crossbow_Base,
		Prefabs.EquipBuff_Weapon_Crossbow_Ability03,
		Prefabs.EquipBuff_Weapon_Crossbow_Ability02,
		Prefabs.EquipBuff_Weapon_Crossbow_Ability01,
		Prefabs.EquipBuff_Weapon_Axe_Base,
		Prefabs.EquipBuff_Weapon_Axe_Ability03,
		Prefabs.EquipBuff_Weapon_Axe_Ability02,
		Prefabs.EquipBuff_Weapon_Axe_Ability01,
		Prefabs.EquipBuff_Weapon_Unarmed_Start01,
		Prefabs.EquipBuff_Weapon_Sword_Base,
		Prefabs.EquipBuff_Weapon_Sword_Ability03,
		Prefabs.EquipBuff_Weapon_Sword_Ability02,
		Prefabs.EquipBuff_Weapon_Sword_Ability01,
		Prefabs.EquipBuff_Weapon_Spear_Base,
		Prefabs.EquipBuff_Weapon_Spear_Ability03,
		Prefabs.EquipBuff_Weapon_Spear_Ability02,
		Prefabs.EquipBuff_Weapon_Spear_Ability01,
		Prefabs.EquipBuff_Weapon_Slashers_Base,
		Prefabs.EquipBuff_Weapon_Slashers_Ability03,
		Prefabs.EquipBuff_Weapon_Slashers_Ability02,
		Prefabs.EquipBuff_Weapon_Slashers_Ability01,
		Prefabs.EquipBuff_Weapon_Reaper_Base,
		Prefabs.EquipBuff_Weapon_Reaper_Ability03,
		Prefabs.EquipBuff_Weapon_Reaper_Ability02,
		Prefabs.EquipBuff_Weapon_Reaper_Ability01,
		Prefabs.EquipBuff_Weapon_Longbow_Base,
		Prefabs.EquipBuff_Weapon_GreatSword_MoveCast,
		Prefabs.EquipBuff_Weapon_GreatSword_Fast,
		Prefabs.EquipBuff_Weapon_GreatSword_Base,
		Prefabs.Item_EquipBuff_MagicSource_T01_Bone,
		Prefabs.Item_EquipBuff_MagicSource_BloodKey_T01,
		Prefabs.JumpFromCliffs_Travel_Upwards,
		Prefabs.JumpFromCliffs_Travel,
		Prefabs.AB_BloodBuff_Brute_NulifyAndEmpower,
		Prefabs.Buff_General_DisconnectedTemporaryImmunity,
		Prefabs.AB_BloodBuff_Brute_HealthRegenBonus,
		Prefabs.AB_BloodBuff_General_100,
		Prefabs.AB_BloodBuff_Rogue_MountDamageBonus,
		Prefabs.Buff_Gloomrot_SentryOfficer_TurretCooldown,
		Prefabs.Buff_VBlood_Ability_Replace,
		Prefabs.Buff_General_Disconnected,
		Prefabs.AB_BloodBuff_Scholar_ManaRegenBonus,
		Prefabs.AB_BloodBuff_VBlood_0,
		Prefabs.AB_BloodBuff_Brute_100,
		Prefabs.AB_BloodBuff_Brute_HealReceivedProc,
		Prefabs.AB_BloodBuff_Mutant_HealReceivedProc,
		Prefabs.AB_BloodBuff_Brute_RecoverOnKill,
		Prefabs.AB_BloodBuff_Creature_RecoverOnKill,
		Prefabs.AB_BloodBuff_Mutant_BiteToMutant,
		Prefabs.AB_BloodBuff_PrimaryProc_FreeCast,
		Prefabs.AB_BloodBuff_Rogue_100,
		Prefabs.AB_BloodBuff_Rogue_CritProcAmplify,
		Prefabs.AB_BloodBuff_Warrior_100,
		Prefabs.AB_BloodBuff_Warrior_PhysDamageBonus,
		Prefabs.AB_BloodBuff_Worker_100,
		Prefabs.AB_BloodBuff_Worker_Pulverize,
		Prefabs.AB_BloodBuff_Brute_GearLevelBonus,
		Prefabs.AB_BloodBuff_Brute_PhysLifeLeech,
		Prefabs.AB_BloodBuff_Creature_SpeedBonus,
		Prefabs.AB_BloodBuff_Creature_SunResistance,
		Prefabs.AB_BloodBuff_DamageReduction_Creature,
		Prefabs.AB_BloodBuff_DamageReduction_Warrior,
		Prefabs.AB_BloodBuff_DoubleCharges_Travel,
		Prefabs.AB_BloodBuff_Mutant_AllResistance,
		Prefabs.AB_BloodBuff_Mutant_BloodConsumption,
		Prefabs.AB_BloodBuff_Mutant_HealthRegeneration,
		Prefabs.AB_BloodBuff_PrimaryAttackLifeLeech,
		Prefabs.AB_BloodBuff_Rogue_AttackSpeedBonus,
		Prefabs.AB_BloodBuff_Rogue_PhysCritChanceBonus,
		Prefabs.AB_BloodBuff_Rogue_SpeedBonus,
		Prefabs.AB_BloodBuff_Rogue_TravelCooldown,
		Prefabs.AB_BloodBuff_Scholar_MaxManaBonus,
		Prefabs.AB_BloodBuff_Scholar_SpellCooldown,
		Prefabs.AB_BloodBuff_Scholar_SpellCritChanceBonus,
		Prefabs.AB_BloodBuff_Scholar_SpellLevelBonus,
		Prefabs.AB_BloodBuff_Scholar_SpellPowerBonus,
		Prefabs.AB_BloodBuff_SpellLifeLeech,
		Prefabs.AB_BloodBuff_Warrior_FirstStrike,
		Prefabs.AB_BloodBuff_Warrior_PhysCritDamageBonus,
		Prefabs.AB_BloodBuff_Warrior_PhysPowerBonus,
		Prefabs.AB_BloodBuff_Warrior_WeaponCooldown,
		Prefabs.AB_BloodBuff_Warrior_WeaponLevelBonus,
		Prefabs.AB_BloodBuff_Worker_GallopBonus,
		Prefabs.AB_BloodBuff_Worker_IncreaseYield,
		Prefabs.AB_BloodBuff_Worker_ReducedDurability,
		Prefabs.AB_BloodBuff_Worker_ResourceDamageBonus,
		Prefabs.AB_BloodBuff_Mutant_ShapeshiftMovementSpeedBurst,
		Prefabs.AB_BloodBuff_ResetSpellCooldownOnCast,
		Prefabs.AB_BloodBuff_Scholar_MovementSpeedOnCast,
		Prefabs.Buff_OutOfCombat,
		Prefabs.AB_Shapeshift_NormalForm_Buff
	};

	public static HashSet<PrefabGUID> ShapeshiftBuffs = new HashSet<PrefabGUID>
	{
		Prefabs.AB_Shapeshift_DominatingPresence_PsychicForm_Buff,
		Prefabs.AB_Shapeshift_Bear_Dash,
		/*Prefabs.AB_Shapeshift_NormalForm_Buff,*/
		Prefabs.AB_Shapeshift_CommandingPresence_Buff,
		Prefabs.AB_Shapeshift_BloodHunger_BloodSight_Buff,
		Prefabs.AB_Shapeshift_Toad_Buff,
		Prefabs.AB_Shapeshift_Bat_Buff,
		Prefabs.AB_Shapeshift_Bear_Buff,
		Prefabs.AB_Shapeshift_Bear_Skin01_Buff,
		Prefabs.AB_Shapeshift_Human_Buff,
		Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Buff,
		Prefabs.AB_Shapeshift_Wolf_Buff,
		Prefabs.AB_Shapeshift_Wolf_Skin01_Buff,
		Prefabs.AB_Shapeshift_BloodMend_Buff,
		/*Prefabs.AB_Shapeshift_Golem_T01_Buff,
		Prefabs.AB_Shapeshift_Golem_T02_Buff,*/
		Prefabs.AB_Shapeshift_Rat_Buff,
		Prefabs.AB_Shapeshift_ShareBlood_ExposeVein_Buff,
	};

	public static HashSet<PrefabGUID> ConsumableBuffs = new HashSet<PrefabGUID>
	{
		Prefabs.AB_Consumable_Antidote_Curse_Buff,
		Prefabs.AB_Consumable_HealingPotion_Debuff,
		/*Prefabs.AB_Consumable_Eat_TrippyShroom_Buff,*/
		Prefabs.AB_Consumable_SpellBrew_T02_Buff,
		Prefabs.AB_Consumable_SpellBrew_T01_Buff,
		Prefabs.AB_Consumable_SilverResistancePotion_T03_Buff,
		Prefabs.AB_Consumable_SilverResistanceBrew_T01_Buff,
		Prefabs.AB_Consumable_PhysicalBrew_T02_Buff,
		Prefabs.AB_Consumable_PhysicalBrew_T01_Buff,
		Prefabs.AB_Consumable_MinorSunResistanceBrew_T01_Buff,
		Prefabs.AB_Consumable_MinorGarlicResistanceBrew_T01_Buff,
		Prefabs.AB_Consumable_MinorFireResistanceBrew_T01_Buff,
		Prefabs.AB_Consumable_HolyResistancePotion_T03_Buff,
		Prefabs.AB_Consumable_HolyResistancePotion_T02_Buff,
		Prefabs.AB_Consumable_GarlicResistancePotion_T02_Buff,
		Prefabs.AB_Interact_UseRelic_Paladin_Buff,
		Prefabs.AB_Interact_UseRelic_Monster_Buff,
		Prefabs.AB_Interact_UseRelic_Manticore_Buff,
		Prefabs.AB_Interact_UseRelic_Behemoth_Buff,
		Prefabs.AB_Consumable_Salve_Vermin_Heal,
		Prefabs.AB_Consumable_RoseTea_T02_Heal,
		Prefabs.AB_Consumable_RoseTea_T01_Heal,
		/*Prefabs.AB_Consumable_Eat_Rat_Heal,
		Prefabs.AB_Consumable_Eat_Heart_Unsullied_Heal,
		Prefabs.AB_Consumable_Eat_Heart_Tainted_Heal,
		Prefabs.AB_Consumable_Eat_Heart_Pristine_Heal,
		Prefabs.AB_Consumable_Eat_Heart_Exquisite_Heal,
		Prefabs.AB_Consumable_Eat_Heart_Defiled_Heal,*/
		Prefabs.AB_Consumable_Bottle_CrimsonDraught_Heal,
		Prefabs.AB_Consumable_BloodPotion_T04_Heal,
		Prefabs.AB_Consumable_BloodPotion_T03_Heal,
		Prefabs.AB_Consumable_BloodPotion_T02_Heal,
		Prefabs.AB_Consumable_BloodPotion_T01_Heal,
		Prefabs.AB_Consumable_BloodCanteen_T01_Heal,
		Prefabs.AB_Consumable_WranglersTea_T01_Buff,
		Prefabs.AB_Consumable_Bottle_CrimsonDraught_Activate,
		Prefabs.AB_Consumable_Bottle_EmptyCanister_Activate,
		Prefabs.AB_Consumable_Bottle_EmptyCanteen_Activate,
		Prefabs.AB_Consumable_Bottle_EmptyCrystalFlask_Activate,
		Prefabs.AB_Consumable_Bottle_EmptyGlassBottle_Activate,
		Prefabs.AB_Consumable_BarrelDisguise_Buff,
		Prefabs.Buff_BloodMoon, //this is here because we use it as the "visual display" that they have rage/witch on
	};

	public static HashSet<PrefabGUID> DelayedBuffs = new HashSet<PrefabGUID>
	{
		Prefabs.Buff_BloodBuff_Empower,
		Prefabs.Blood_Vampire_Buff_Leech
	};
}
