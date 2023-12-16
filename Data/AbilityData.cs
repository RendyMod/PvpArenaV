using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectM;
using UnityEngine;

namespace PvpArena.Data;
public static class AbilityData
{
	public static Dictionary<PrefabGUID, string> AbilityPrefabToName = new() 
	{
		{ Prefabs.Chaos_Vampire_Buff_Ignite, "Ignite" },
		{ Prefabs.Chaos_Vampire_Combust_AreaImpact, "Combustion" },
		{ Prefabs.Storm_Vampire_Buff_Static, "Static" },
		{ Prefabs.AB_Spear_AThousandSpears_Stab_Hit, "Spear Q" },
		{ Prefabs.AB_Spectral_SpellSlinger_Projectile_Projectile, "Veil of Illusion"},
		{ Prefabs.AB_GreatSword_GreatCleaver_Hit_01, "Greatsword Q"},
		{ Prefabs.AB_GreatSword_LeapAttack_Hit, "Greatsword E"},
		{ Prefabs.AB_Pistols_Primary_Attack_Projectile_01, "Gun Auto"},
		{ Prefabs.AB_Pistols_Primary_Attack_Projectile_02, "Gun Auto"},
		{ Prefabs.AB_Pistols_FanTheHammer_Projectile, "Gun Q"},
		{ Prefabs.AB_Pistols_ExplosiveShot_Shot_Projectile, "Gun E"},
		{ Prefabs.AB_Pistols_ExplosiveShot_Shot_ExplosiveImpact, "Gun E"},
		{ Prefabs.AB_Blood_BloodFountain_Ground_Impact, "Blood Fountain"},
		{ Prefabs.AB_Blood_BloodFountain_Ground_Init, "Blood Fountain"},
		{ Prefabs.AB_Blood_BloodFountain_Spellmod_Recast_Ground_Impact, "Blood Fountain"},
		{ Prefabs.AB_Blood_BloodFountain_Spellmod_Recast_Ground_Init, "Blood Fountain"},
		{ Prefabs.AB_Blood_BloodFountain_Spellmod_Recast_Throw, "Blood Fountain"},
		{ Prefabs.AB_Blood_BloodFountain_Throw, "Blood Fountain"},
		{ Prefabs.AB_Blood_BloodRage_Area, "Blood Rage"},
		{ Prefabs.AB_Blood_BloodRite_Hit, "Bloodrite"},
		{ Prefabs.AB_Blood_HeartStrike_Debuff, "Heart Strike"},
		{ Prefabs.AB_Blood_HeartStrike_Phase, "Heart Strike"},
		{ Prefabs.AB_Blood_SanguineCoil_Projectile, "Sanguine Coil"},
		{ Prefabs.AB_Blood_SanguineCoil_SpellMod_Projectile_AllowOwner, "Sanguine Coil"},
		{ Prefabs.AB_Blood_Shadowbolt_Projectile, "Shadowbolt"},
		{ Prefabs.AB_Blood_Shadowbolt_SpellMod_Area, "Shadowbolt"},
		{ Prefabs.AB_Blood_Shadowbolt_SpellMod_ForkProjectile, "Shadowbolt"},
		{ Prefabs.AB_Chaos_Aftershock_AreaThrow, "Aftershock"},
		{ Prefabs.AB_Chaos_Aftershock_Projectile, "Aftershock"},
		{ Prefabs.AB_Chaos_Aftershock_SpellMod_KnockbackArea, "Aftershock"},
		{ Prefabs.AB_Chaos_Barrier_Projectile, "Chaos Barrier"},
		{ Prefabs.AB_Chaos_Barrier_Recast_LesserProjectile, "Chaos Barrier"},
		{ Prefabs.AB_Chaos_Barrier_Recast_Projectile, "Chaos Barrier"},
		{ Prefabs.AB_Chaos_Barrier_SpellMod_ForkProjectile, "Chaos Barrier"},
		{ Prefabs.AB_Chaos_ChaosBarrage_Area, "Chaos Barrage"},
		{ Prefabs.AB_Chaos_ChaosBarrage_Projectile, "Chaos Barrage"},
		{ Prefabs.AB_Chaos_MercilessCharge_EndImpact, "Merciless Charge"},
		{ Prefabs.AB_Chaos_MercilessCharge_Phase, "Merciless Charge"},
		{ Prefabs.AB_Chaos_PowerSurge_Spellmod_Recast_AreaImpact, "Power Surge"},
		{ Prefabs.AB_Chaos_PowerSurge_Spellmod_Recast_Trigger, "Power Surge"},
		{ Prefabs.AB_Chaos_Void_SpellMod_BurnArea, "Void"},
		{ Prefabs.AB_Chaos_Void_SpellMod_ClusterBomb, "Void"},
		{ Prefabs.AB_Chaos_Void_Throw, "Void"},
		{ Prefabs.AB_Chaos_Volley_Projectile_First, "Chaos Volley"},
		{ Prefabs.AB_Chaos_Volley_Projectile_Second, "Chaos Volley"},
		{ Prefabs.AB_Frost_ArcticLeap_End, "Arctic Leap"},
		{ Prefabs.AB_Frost_ArcticLeap_FrostSpikes_01, "Arctic Leap"},
		{ Prefabs.AB_Frost_ArcticLeap_FrostSpikes_02, "Arctic Leap"},
		{ Prefabs.AB_Frost_ArcticLeap_FrostSpikes_03, "Arctic Leap"},
		{ Prefabs.AB_Frost_ArcticLeap_FrostSpikes_04, "Arctic Leap"},
		{ Prefabs.AB_Frost_ArcticLeap_FrostSpikes_Base, "Arctic Leap"},
		{ Prefabs.AB_Frost_CrystalLance_Projectile, "Crystal Lance"},
		{ Prefabs.AB_Frost_CrystalLance_Projectile_SpellMod_Pierce, "Crystal Lance"},
		{ Prefabs.AB_Frost_FrostBat_AoE, "Frost Bat"},
		{ Prefabs.AB_Frost_FrostBat_Projectile, "Frost Bat"},
		{ Prefabs.AB_Frost_FrostVortex_Delay, "Frost Vortex"},
		{ Prefabs.AB_Frost_FrostVortex_Delayed, "Frost Vortex"},
		{ Prefabs.AB_Frost_FrostVortex_Throw, "Frost Vortex"},
		{ Prefabs.AB_Frost_IceNova_SpellMod_Recast_Throw, "Ice Nova"},
		{ Prefabs.AB_Frost_IceNova_Throw, "Ice Nova"},
		{ Prefabs.AB_FrostBarrier_Pulse, "Frost Barrier"},
		{ Prefabs.AB_FrostBarrier_Recast_Cone, "Frost Barrier"},
		{ Prefabs.AB_Illusion_MistTrance_PreTravel, "Mist Trance"},
		{ Prefabs.AB_Illusion_MistTrance_TravelEnd, "Mist Trance"},
		{ Prefabs.AB_Illusion_Mosquito_Area_Explosion, "Mosquito"},
		{ Prefabs.AB_Illusion_Mosquito_Summon, "Mosquito"},
		{ Prefabs.AB_Illusion_PhantomAegis_Buff, "Phantom Aegis"},
		{ Prefabs.AB_Illusion_PhantomAegis_Recast_TriggerBuff, "Phantom Aegis"},
		{ Prefabs.AB_Illusion_PhantomAegis_SpellMod_Explode, "Phantom Aegis"},
		{ Prefabs.AB_Illusion_SpectralGuardian_ApplyShieldAoE, "Spectral Guardian"},
		{ Prefabs.AB_Illusion_SpectralGuardian_Summon_Throw, "Spectral Guardian"},
		{ Prefabs.AB_Illusion_SpectralWolf_Projectile_Bouncing, "Spectral Wolf"},
		{ Prefabs.AB_Illusion_SpectralWolf_Projectile_First, "Spectral Wolf"},
		{ Prefabs.AB_Illusion_SpectralWolf_Projectile_Owner_Homing, "Spectral Wolf"},
		{ Prefabs.AB_Illusion_WispDance_Buff01, "Wisp Dance"},
		{ Prefabs.AB_Illusion_WispDance_Buff02, "Wisp Dance"},
		{ Prefabs.AB_Illusion_WispDance_Buff03, "Wisp Dance"},
		{ Prefabs.AB_Illusion_WispDance_Recast_Projectile, "Wisp Dance"},
		{ Prefabs.AB_Illusion_WraithSpear_Projectile, "Wraith Spear"},
		{ Prefabs.AB_Storm_BallLightning_AreaImpact, "Ball Lightning"},
		{ Prefabs.AB_Storm_BallLightning_Projectile, "Ball Lightning"},
		{ Prefabs.AB_Storm_BallLightning_Spellmod_Recast_Trigger, "Ball Lightning"},
		{ Prefabs.AB_Storm_Cyclone_Projectile, "Cyclone"},
		{ Prefabs.AB_Storm_Discharge_Travel, "Discharge"},
		{ Prefabs.AB_Storm_Discharge_Travel_Second, "Discharge"},
		{ Prefabs.AB_Storm_EyeOfTheStorm_Buff, "Eye of the Storm"},
		{ Prefabs.AB_Storm_EyeOfTheStorm_Throw, "Eye of the Storm"},
		{ Prefabs.AB_Storm_LightningWall_Object, "Lightning Curtain"},
		{ Prefabs.AB_Storm_LightningWall_Throw, "Lightning Curtain"},
		{ Prefabs.AB_Storm_PolarityShift_Projectile, "Polarity Shift"},
		{ Prefabs.AB_Storm_PolarityShift_SpellMod_AreaImpactDestination, "Polarity Shift"},
		{ Prefabs.AB_Storm_PolarityShift_SpellMod_AreaImpactOrigin, "Polarity Shift"},
		{ Prefabs.AB_Storm_RagingTempest_Area_Hit, "Raging Tempest"},
		{ Prefabs.AB_Storm_RagingTempest_LightningStrike, "Raging Tempest"},
		{ Prefabs.AB_Storm_RagingTempest_Phase, "Raging Tempest"},
		{ Prefabs.AB_Unholy_Baneling_Explode_Poison_Area, "Volatile Arachnid"},
		{ Prefabs.AB_Unholy_Baneling_Explode_Poison_Hit, "Volatile Arachnid"},
		{ Prefabs.AB_Unholy_CorpseExplosion_SpellMod_DoubleImpact, "Corpse Explosion"},
		{ Prefabs.AB_Unholy_CorpseExplosion_SpellMod_SkullNova_Projectile, "Corpse Explosion"},
		{ Prefabs.AB_Unholy_CorpseExplosion_Throw, "Corpse Explosion"},
		{ Prefabs.AB_Unholy_CorruptedSkull_Projectile, "Corrupted Skull"},
		{ Prefabs.AB_Unholy_CorruptedSkull_SpellMod_BoneSpirit, "Corrupted Skull"},
		{ Prefabs.AB_Unholy_CorruptedSkull_SpellMod_DetonateSkeleton, "Corrupted Skull"},
		{ Prefabs.AB_Unholy_CorruptedSkull_SpellMod_LesserProjectile, "Corrupted Skull"},
		{ Prefabs.AB_Unholy_DeathKnight_Summon, "Death Knight"},
		{ Prefabs.AB_Unholy_DeathKnightStrike_Hit, "Death Knight"},
		{ Prefabs.AB_Unholy_FallenAngel_Dash_Phase, "Fallen Angel"},
		{ Prefabs.AB_Unholy_FallenAngel_InitAggroTrigger, "Fallen Angel"},
		{ Prefabs.AB_Unholy_FallenAngel_MeleeAttack_StabHit01, "Fallen Angel"},
		{ Prefabs.AB_Unholy_FallenAngel_MeleeAttack_StabHit02, "Fallen Angel"},
		{ Prefabs.AB_Unholy_FallenAngel_MeleeAttack_StabHit03, "Fallen Angel"},
		{ Prefabs.AB_Unholy_FallenAngel_MeleeAttack_SwingHit, "Fallen Angel"},
		{ Prefabs.AB_Unholy_FallenAngel_UnholyBarrage_Area, "Fallen Angel"},
		{ Prefabs.AB_Unholy_FallenAngel_UnholyBarrage_Projectile, "Fallen Angel"},
		{ Prefabs.AB_Unholy_Shared_SkeletonSpawner, "Skeleton"},
		{ Prefabs.AB_Unholy_Shared_SkeletonSpawner_Mage, "Skeleton Mage"},
		{ Prefabs.AB_Unholy_Shared_SkeletonSpawner_Mage_DeathKnightJewel, "Skeleton Mage"},
		{ Prefabs.AB_Unholy_Shared_SkeletonSpawner_WardOfTheDamned, "Skeleton"},
		{ Prefabs.AB_Unholy_SkeletonApprentice_Projectile, "Skeleton Mage"},
		{ Prefabs.AB_Unholy_SkeletonWarrior_MeleeAttack_Hit, "Skeleton Warrior"},
		{ Prefabs.AB_Unholy_Soulburn_Area, "Soulburn"},
		{ Prefabs.AB_Unholy_SummonBanelings_Throw, "Volatile Arachnid"},
		{ Prefabs.AB_Unholy_SummonFallenAngel_Throw, "Fallen Angel"},
		{ Prefabs.AB_Unholy_WardOfTheDamned_Recast_Cone, "Ward of the Damned"},
		{ Prefabs.AB_Vampire_Axe_Frenzy_Dash_Hit, "Axe Q"},
		{ Prefabs.AB_Vampire_Axe_Primary_MeleeAttack_Hit01, "Axe Auto"},
		{ Prefabs.AB_Vampire_Axe_Primary_MeleeAttack_Hit02, "Axe Auto"},
		{ Prefabs.AB_Vampire_Axe_Primary_MeleeAttack_Hit03, "Axe Auto"},
		{ Prefabs.AB_Vampire_Axe_Primary_Mounted_Hit, "Axe Auto"},
		{ Prefabs.AB_Vampire_Axe_XStrike_Toss_Hit, "Axe E"},
		{ Prefabs.AB_Vampire_Axe_XStrike_Toss_Projectile01, "Axe E"},
		{ Prefabs.AB_Vampire_Axe_XStrike_Toss_Projectile02, "Axe E"},
		{ Prefabs.AB_Vampire_Crossbow_Primary_Mounted_Projectile, "Crossbow Auto"},
		{ Prefabs.AB_Vampire_Crossbow_Primary_Projectile, "Crossbow Auto"},
		{ Prefabs.AB_Vampire_Crossbow_Primary_VeilOfBlood_Projectile, "Crossbow Auto"},
		{ Prefabs.AB_Vampire_Crossbow_Primary_VeilOfBones_Projectile, "Crossbow Auto"},
		{ Prefabs.AB_Vampire_Crossbow_Primary_VeilOfChaos_Projectile, "Crossbow Auto"},
		{ Prefabs.AB_Vampire_Crossbow_Primary_VeilOfFrost_Projectile, "Crossbow Auto"},
		{ Prefabs.AB_Vampire_Crossbow_Primary_VeilOfIllusion_Projectile, "Crossbow Auto"},
		{ Prefabs.AB_Vampire_Crossbow_Primary_VeilOfStorm_Projectile, "Crossbow Auto"},
		{ Prefabs.AB_Vampire_Crossbow_RainOfBolts_Throw, "Crossbow Q"},
		{ Prefabs.AB_Vampire_Crossbow_RainOfBolts_Throw_Center, "Crossbow Q"},
		{ Prefabs.AB_Vampire_Crossbow_RainOfBolts_Trigger, "Crossbow Q"},
		{ Prefabs.AB_Vampire_Crossbow_Snapshot_Projectile, "Crossbow E"},
		{ Prefabs.AB_Vampire_GreatSword_Mounted_Hit, "Greatsword Auto"},
		{ Prefabs.AB_Vampire_GreatSword_Primary_Moving_Hit01, "Greatsword Auto"},
		{ Prefabs.AB_Vampire_GreatSword_Primary_Moving_Hit02, "Greatsword Auto"},
		{ Prefabs.AB_Vampire_GreatSword_Primary_Moving_Hit03, "Greatsword Auto"},
		{ Prefabs.AB_Vampire_Longbow_AcrobaticShot_Throw, "Longbow"},
		{ Prefabs.AB_Vampire_Longbow_Primary_Projectile, "Longbow"},
		{ Prefabs.AB_Vampire_Longbow_Sharpshot_Projectile, "Longbow"},
		{ Prefabs.AB_Vampire_Mace_CrushingBlow_Slam_Hit, "Mace Q"},
		{ Prefabs.AB_Vampire_Mace_Primary_MeleeAttack_Hit01, "Mace Auto"},
		{ Prefabs.AB_Vampire_Mace_Primary_MeleeAttack_Hit02, "Mace Auto"},
		{ Prefabs.AB_Vampire_Mace_Primary_MeleeAttack_Hit03, "Mace Auto"},
		{ Prefabs.AB_Vampire_Mace_Primary_Mounted_Hit, "Mace Auto"},
		{ Prefabs.AB_Vampire_Mace_Smack_Hit, "Mace E"},
		{ Prefabs.AB_Vampire_Rapier_Feint_Hit, "Rapier"},
		{ Prefabs.AB_Vampire_Rapier_Lunge_Hit, "Rapier"},
		{ Prefabs.AB_Vampire_Rapier_Lunge_Phase, "Rapier"},
		{ Prefabs.AB_Vampire_Rapier_Lunge_TravelToTarget, "Rapier"},
		{ Prefabs.AB_Vampire_Rapier_MeleeAttack_Hit01, "Rapier"},
		{ Prefabs.AB_Vampire_Rapier_MeleeAttack_Hit02, "Rapier"},
		{ Prefabs.AB_Vampire_Rapier_MeleeAttack_Hit03, "Rapier"},
		{ Prefabs.AB_Vampire_Reaper_HowlingReaper_Hit, "Reaper E"},
		{ Prefabs.AB_Vampire_Reaper_HowlingReaper_Projectile, "Reaper E"},
		{ Prefabs.AB_Vampire_Reaper_Primary_MeleeAttack_Hit01, "Reaper Auto"},
		{ Prefabs.AB_Vampire_Reaper_Primary_MeleeAttack_Hit02, "Reaper Auto"},
		{ Prefabs.AB_Vampire_Reaper_Primary_MeleeAttack_Hit03, "Reaper Auto"},
		{ Prefabs.AB_Vampire_Reaper_Primary_Mounted_Hit, "Reaper Auto"},
		{ Prefabs.AB_Vampire_Reaper_TendonSwing_Twist_Hit, "Reaper Q"},
		{ Prefabs.AB_Vampire_Slashers_Camouflage_Secondary_Hit, "Slasher E"},
		{ Prefabs.AB_Vampire_Slashers_ElusiveStrike_Dash_PhaseIn, "Slasher Q"},
		{ Prefabs.AB_Vampire_Slashers_ElusiveStrike_Dash_PhaseOut, "Slasher Q"},
		{ Prefabs.AB_Vampire_Slashers_Primary_MeleeAttack_Hit01, "Slasher Auto"},
		{ Prefabs.AB_Vampire_Slashers_Primary_MeleeAttack_Hit02, "Slasher Auto"},
		{ Prefabs.AB_Vampire_Slashers_Primary_MeleeAttack_Hit03, "Slasher Auto"},
		{ Prefabs.AB_Vampire_Slashers_Primary_Mounted_Hit, "Slasher Auto"},
		{ Prefabs.AB_Vampire_Spear_Harpoon_Throw_Projectile, "Spear E"},
		{ Prefabs.AB_Vampire_Spear_Primary_MeleeAttack_Hit01, "Spear Auto"},
		{ Prefabs.AB_Vampire_Spear_Primary_MeleeAttack_Hit02, "Spear Auto"},
		{ Prefabs.AB_Vampire_Spear_Primary_MeleeAttack_Hit03, "Spear Auto"},
		{ Prefabs.AB_Vampire_Spear_Primary_Mounted_Hit, "Spear Auto"},
		{ Prefabs.AB_Spear_AThousandSpears_Recast_Impale_Hit, "Spear Q"},
		{ Prefabs.AB_Vampire_Sword_Primary_MeleeAttack_Hit01, "Sword Auto"},
		{ Prefabs.AB_Vampire_Sword_Primary_MeleeAttack_Hit02, "Sword Auto"},
		{ Prefabs.AB_Vampire_Sword_Primary_MeleeAttack_Hit03, "Sword Auto"},
		{ Prefabs.AB_Vampire_Sword_Primary_Mounted_Hit, "Sword Auto"},
		{ Prefabs.AB_Vampire_Sword_Shockwave_Main_Projectile, "Sword E"},
		{ Prefabs.AB_Vampire_Sword_Whirlwind_Spin_Hit, "Sword Q"},
		{ Prefabs.AB_Vampire_Sword_Whirlwind_Spin_LastHit, "Sword Q"},
		{ Prefabs.AB_Vampire_Unarmed_Primary_MeleeAttack_Hit01, "Unarmed Auto"},
		{ Prefabs.AB_Vampire_Unarmed_Primary_MeleeAttack_Hit02, "Unarmed Auto"},
		{ Prefabs.AB_Vampire_Unarmed_Primary_MeleeAttack_Hit03, "Unarmed Auto"},
		{ Prefabs.AB_Vampire_Unarmed_Primary_Mounted_Hit, "Unarmed Auto"},
		{ Prefabs.AB_Vampire_Unarmed_Secondary_Hit, "Unarmed Auto"},
		{ Prefabs.AB_Vampire_VeilOfBlood_SpellMod_BloodNova, "Veil of Blood"},
		{ Prefabs.AB_Vampire_VeilOfBlood_SpellMod_DashInflictLeechBuff, "Veil of Blood"},
		{ Prefabs.AB_Vampire_VeilOfBlood_TriggerBonusEffects, "Veil of Blood"},
		{ Prefabs.AB_Vampire_VeilOfBones_BounceProjectile, "Veil of Bones"},
		{ Prefabs.AB_Vampire_VeilOfBones_SpellMod_DashHealMinions, "Veil of Bones"},
		{ Prefabs.AB_Vampire_VeilOfBones_SpellMod_DashInflictCondemnBuff, "Veil of Bones"},
		{ Prefabs.AB_Vampire_VeilOfChaos_Bomb, "Veil of Chaos"},
		{ Prefabs.AB_Vampire_VeilOfChaos_SpellMod_BonusDummy_Bomb, "Veil of Chaos"},
		{ Prefabs.AB_Vampire_VeilOfFrost_AoE, "Veil of Frost"},
		{ Prefabs.AB_Vampire_VeilOfFrost_SpellMod_IllusionFrostBlast, "Veil of Frost"},
		{ Prefabs.AB_Vampire_VeilOfIllusion_SpellMod_RecastDetonate, "Veil of Illusion"},
		{ Prefabs.AB_Vampire_VeilOfStorm_LightningTriggerBuff, "Veil of Storm"},
		{ Prefabs.AB_Vampire_VeilOfStorm_SpellMod_SparklingIllusion, "Veil of Storm"},
	};

	public static Dictionary<PrefabGUID, Color32> AbilityPrefabToColor = new()
	{
		{ Prefabs.Chaos_Vampire_Buff_Ignite, ExtendedColor.Chaos },
		{ Prefabs.Chaos_Vampire_Combust_AreaImpact, ExtendedColor.Chaos },
		{ Prefabs.Storm_Vampire_Buff_Static, ExtendedColor.Storm },
		{ Prefabs.AB_Spear_AThousandSpears_Stab_Hit, ExtendedColor.Orange },
		{ Prefabs.AB_Spectral_SpellSlinger_Projectile_Projectile, ExtendedColor.Illusion},
		{ Prefabs.AB_GreatSword_GreatCleaver_Hit_01, ExtendedColor.Orange},
		{ Prefabs.AB_GreatSword_LeapAttack_Hit, ExtendedColor.Orange},
		{ Prefabs.AB_Pistols_Primary_Attack_Projectile_01, ExtendedColor.Orange},
		{ Prefabs.AB_Pistols_Primary_Attack_Projectile_02, ExtendedColor.Orange},
		{ Prefabs.AB_Pistols_FanTheHammer_Projectile, ExtendedColor.Orange},
		{ Prefabs.AB_Pistols_ExplosiveShot_Shot_Projectile, ExtendedColor.Orange},
		{ Prefabs.AB_Pistols_ExplosiveShot_Shot_ExplosiveImpact, ExtendedColor.Orange},
		{ Prefabs.AB_Blood_BloodFountain_Ground_Impact, ExtendedColor.Blood},
		{ Prefabs.AB_Blood_BloodFountain_Ground_Init, ExtendedColor.Blood},
		{ Prefabs.AB_Blood_BloodFountain_Spellmod_Recast_Ground_Impact, ExtendedColor.Blood},
		{ Prefabs.AB_Blood_BloodFountain_Spellmod_Recast_Ground_Init, ExtendedColor.Blood},
		{ Prefabs.AB_Blood_BloodFountain_Spellmod_Recast_Throw, ExtendedColor.Blood},
		{ Prefabs.AB_Blood_BloodFountain_Throw, ExtendedColor.Blood},
		{ Prefabs.AB_Blood_BloodRage_Area, ExtendedColor.Blood},
		{ Prefabs.AB_Blood_BloodRite_Hit, ExtendedColor.Blood},
		{ Prefabs.AB_Blood_HeartStrike_Debuff, ExtendedColor.Blood},
		{ Prefabs.AB_Blood_HeartStrike_Phase, ExtendedColor.Blood},
		{ Prefabs.AB_Blood_SanguineCoil_Projectile, ExtendedColor.Blood},
		{ Prefabs.AB_Blood_SanguineCoil_SpellMod_Projectile_AllowOwner, ExtendedColor.Blood},
		{ Prefabs.AB_Blood_Shadowbolt_Projectile, ExtendedColor.Blood},
		{ Prefabs.AB_Blood_Shadowbolt_SpellMod_Area, ExtendedColor.Blood},
		{ Prefabs.AB_Blood_Shadowbolt_SpellMod_ForkProjectile, ExtendedColor.Blood},
		{ Prefabs.AB_Chaos_Aftershock_AreaThrow, ExtendedColor.Chaos},
		{ Prefabs.AB_Chaos_Aftershock_Projectile, ExtendedColor.Chaos},
		{ Prefabs.AB_Chaos_Aftershock_SpellMod_KnockbackArea, ExtendedColor.Chaos},
		{ Prefabs.AB_Chaos_Barrier_Projectile, ExtendedColor.Chaos},
		{ Prefabs.AB_Chaos_Barrier_Recast_LesserProjectile, ExtendedColor.Chaos},
		{ Prefabs.AB_Chaos_Barrier_Recast_Projectile, ExtendedColor.Chaos},
		{ Prefabs.AB_Chaos_Barrier_SpellMod_ForkProjectile, ExtendedColor.Chaos},
		{ Prefabs.AB_Chaos_ChaosBarrage_Area, ExtendedColor.Chaos},
		{ Prefabs.AB_Chaos_ChaosBarrage_Projectile, ExtendedColor.Chaos},
		{ Prefabs.AB_Chaos_MercilessCharge_EndImpact, ExtendedColor.Chaos},
		{ Prefabs.AB_Chaos_MercilessCharge_Phase, ExtendedColor.Chaos},
		{ Prefabs.AB_Chaos_PowerSurge_Spellmod_Recast_AreaImpact, ExtendedColor.Chaos},
		{ Prefabs.AB_Chaos_PowerSurge_Spellmod_Recast_Trigger, ExtendedColor.Chaos},
		{ Prefabs.AB_Chaos_Void_SpellMod_BurnArea, ExtendedColor.Chaos},
		{ Prefabs.AB_Chaos_Void_SpellMod_ClusterBomb, ExtendedColor.Chaos},
		{ Prefabs.AB_Chaos_Void_Throw, ExtendedColor.Chaos},
		{ Prefabs.AB_Chaos_Volley_Projectile_First, ExtendedColor.Chaos},
		{ Prefabs.AB_Chaos_Volley_Projectile_Second, ExtendedColor.Chaos},
		{ Prefabs.AB_Frost_ArcticLeap_End, ExtendedColor.Frost},
		{ Prefabs.AB_Frost_ArcticLeap_FrostSpikes_01, ExtendedColor.Frost},
		{ Prefabs.AB_Frost_ArcticLeap_FrostSpikes_02, ExtendedColor.Frost},
		{ Prefabs.AB_Frost_ArcticLeap_FrostSpikes_03, ExtendedColor.Frost},
		{ Prefabs.AB_Frost_ArcticLeap_FrostSpikes_04, ExtendedColor.Frost},
		{ Prefabs.AB_Frost_ArcticLeap_FrostSpikes_Base, ExtendedColor.Frost},
		{ Prefabs.AB_Frost_CrystalLance_Projectile, ExtendedColor.Frost},
		{ Prefabs.AB_Frost_CrystalLance_Projectile_SpellMod_Pierce, ExtendedColor.Frost},
		{ Prefabs.AB_Frost_FrostBat_AoE, ExtendedColor.Frost},
		{ Prefabs.AB_Frost_FrostBat_Projectile, ExtendedColor.Frost},
		{ Prefabs.AB_Frost_FrostVortex_Delay, ExtendedColor.Frost},
		{ Prefabs.AB_Frost_FrostVortex_Delayed, ExtendedColor.Frost},
		{ Prefabs.AB_Frost_FrostVortex_Throw, ExtendedColor.Frost},
		{ Prefabs.AB_Frost_IceNova_SpellMod_Recast_Throw, ExtendedColor.Frost},
		{ Prefabs.AB_Frost_IceNova_Throw, ExtendedColor.Frost},
		{ Prefabs.AB_FrostBarrier_Pulse, ExtendedColor.Frost},
		{ Prefabs.AB_FrostBarrier_Recast_Cone, ExtendedColor.Frost},
		{ Prefabs.AB_Illusion_MistTrance_PreTravel, ExtendedColor.Illusion},
		{ Prefabs.AB_Illusion_MistTrance_TravelEnd, ExtendedColor.Illusion},
		{ Prefabs.AB_Illusion_Mosquito_Area_Explosion, ExtendedColor.Illusion},
		{ Prefabs.AB_Illusion_Mosquito_Summon, ExtendedColor.Illusion},
		{ Prefabs.AB_Illusion_PhantomAegis_Buff, ExtendedColor.Illusion},
		{ Prefabs.AB_Illusion_PhantomAegis_Recast_TriggerBuff, ExtendedColor.Illusion},
		{ Prefabs.AB_Illusion_PhantomAegis_SpellMod_Explode, ExtendedColor.Illusion},
		{ Prefabs.AB_Illusion_SpectralGuardian_ApplyShieldAoE, ExtendedColor.Illusion},
		{ Prefabs.AB_Illusion_SpectralGuardian_Summon_Throw, ExtendedColor.Illusion},
		{ Prefabs.AB_Illusion_SpectralWolf_Projectile_Bouncing, ExtendedColor.Illusion},
		{ Prefabs.AB_Illusion_SpectralWolf_Projectile_First, ExtendedColor.Illusion},
		{ Prefabs.AB_Illusion_SpectralWolf_Projectile_Owner_Homing, ExtendedColor.Illusion},
		{ Prefabs.AB_Illusion_WispDance_Buff01, ExtendedColor.Illusion},
		{ Prefabs.AB_Illusion_WispDance_Buff02, ExtendedColor.Illusion},
		{ Prefabs.AB_Illusion_WispDance_Buff03, ExtendedColor.Illusion},
		{ Prefabs.AB_Illusion_WispDance_Recast_Projectile, ExtendedColor.Illusion},
		{ Prefabs.AB_Illusion_WraithSpear_Projectile, ExtendedColor.Illusion},
		{ Prefabs.AB_Storm_BallLightning_AreaImpact, ExtendedColor.Storm},
		{ Prefabs.AB_Storm_BallLightning_Projectile, ExtendedColor.Storm},
		{ Prefabs.AB_Storm_BallLightning_Spellmod_Recast_Trigger, ExtendedColor.Storm},
		{ Prefabs.AB_Storm_Cyclone_Projectile, ExtendedColor.Storm},
		{ Prefabs.AB_Storm_Discharge_Travel, ExtendedColor.Storm},
		{ Prefabs.AB_Storm_Discharge_Travel_Second, ExtendedColor.Storm},
		{ Prefabs.AB_Storm_EyeOfTheStorm_Buff, ExtendedColor.Storm},
		{ Prefabs.AB_Storm_EyeOfTheStorm_Throw, ExtendedColor.Storm},
		{ Prefabs.AB_Storm_LightningWall_Object, ExtendedColor.Storm},
		{ Prefabs.AB_Storm_LightningWall_Throw, ExtendedColor.Storm},
		{ Prefabs.AB_Storm_PolarityShift_Projectile, ExtendedColor.Storm},
		{ Prefabs.AB_Storm_PolarityShift_SpellMod_AreaImpactDestination, ExtendedColor.Storm},
		{ Prefabs.AB_Storm_PolarityShift_SpellMod_AreaImpactOrigin, ExtendedColor.Storm},
		{ Prefabs.AB_Storm_RagingTempest_Area_Hit, ExtendedColor.Storm},
		{ Prefabs.AB_Storm_RagingTempest_LightningStrike, ExtendedColor.Storm},
		{ Prefabs.AB_Storm_RagingTempest_Phase, ExtendedColor.Storm},
		{ Prefabs.AB_Unholy_Baneling_Explode_Poison_Area, ExtendedColor.Unholy},
		{ Prefabs.AB_Unholy_Baneling_Explode_Poison_Hit, ExtendedColor.Unholy},
		{ Prefabs.AB_Unholy_CorpseExplosion_SpellMod_DoubleImpact, ExtendedColor.Unholy},
		{ Prefabs.AB_Unholy_CorpseExplosion_SpellMod_SkullNova_Projectile, ExtendedColor.Unholy},
		{ Prefabs.AB_Unholy_CorpseExplosion_Throw, ExtendedColor.Unholy},
		{ Prefabs.AB_Unholy_CorruptedSkull_Projectile, ExtendedColor.Unholy},
		{ Prefabs.AB_Unholy_CorruptedSkull_SpellMod_BoneSpirit, ExtendedColor.Unholy},
		{ Prefabs.AB_Unholy_CorruptedSkull_SpellMod_DetonateSkeleton, ExtendedColor.Unholy},
		{ Prefabs.AB_Unholy_CorruptedSkull_SpellMod_LesserProjectile, ExtendedColor.Unholy},
		{ Prefabs.AB_Unholy_DeathKnight_Summon, ExtendedColor.Unholy},
		{ Prefabs.AB_Unholy_DeathKnightStrike_Hit, ExtendedColor.Unholy},
		{ Prefabs.AB_Unholy_FallenAngel_Dash_Phase, ExtendedColor.Unholy},
		{ Prefabs.AB_Unholy_FallenAngel_InitAggroTrigger, ExtendedColor.Unholy},
		{ Prefabs.AB_Unholy_FallenAngel_MeleeAttack_StabHit01, ExtendedColor.Unholy},
		{ Prefabs.AB_Unholy_FallenAngel_MeleeAttack_StabHit02, ExtendedColor.Unholy},
		{ Prefabs.AB_Unholy_FallenAngel_MeleeAttack_StabHit03, ExtendedColor.Unholy},
		{ Prefabs.AB_Unholy_FallenAngel_MeleeAttack_SwingHit, ExtendedColor.Unholy},
		{ Prefabs.AB_Unholy_FallenAngel_UnholyBarrage_Area, ExtendedColor.Unholy},
		{ Prefabs.AB_Unholy_FallenAngel_UnholyBarrage_Projectile, ExtendedColor.Unholy},
		{ Prefabs.AB_Unholy_Shared_SkeletonSpawner, ExtendedColor.Unholy},
		{ Prefabs.AB_Unholy_Shared_SkeletonSpawner_Mage, ExtendedColor.Unholy},
		{ Prefabs.AB_Unholy_Shared_SkeletonSpawner_Mage_DeathKnightJewel, ExtendedColor.Unholy},
		{ Prefabs.AB_Unholy_Shared_SkeletonSpawner_WardOfTheDamned, ExtendedColor.Unholy},
		{ Prefabs.AB_Unholy_SkeletonApprentice_Projectile, ExtendedColor.Unholy},
		{ Prefabs.AB_Unholy_SkeletonWarrior_MeleeAttack_Hit, ExtendedColor.Unholy},
		{ Prefabs.AB_Unholy_Soulburn_Area, ExtendedColor.Unholy},
		{ Prefabs.AB_Unholy_SummonBanelings_Throw, ExtendedColor.Unholy},
		{ Prefabs.AB_Unholy_SummonFallenAngel_Throw, ExtendedColor.Unholy},
		{ Prefabs.AB_Unholy_WardOfTheDamned_Recast_Cone, ExtendedColor.Unholy},
		{ Prefabs.AB_Vampire_Axe_Frenzy_Dash_Hit, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Axe_Primary_MeleeAttack_Hit01, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Axe_Primary_MeleeAttack_Hit02, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Axe_Primary_MeleeAttack_Hit03, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Axe_Primary_Mounted_Hit, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Axe_XStrike_Toss_Hit, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Axe_XStrike_Toss_Projectile01, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Axe_XStrike_Toss_Projectile02, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Crossbow_Primary_Mounted_Projectile, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Crossbow_Primary_Projectile, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Crossbow_Primary_VeilOfBlood_Projectile, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Crossbow_Primary_VeilOfBones_Projectile, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Crossbow_Primary_VeilOfChaos_Projectile, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Crossbow_Primary_VeilOfFrost_Projectile, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Crossbow_Primary_VeilOfIllusion_Projectile, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Crossbow_Primary_VeilOfStorm_Projectile, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Crossbow_RainOfBolts_Throw, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Crossbow_RainOfBolts_Throw_Center, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Crossbow_RainOfBolts_Trigger, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Crossbow_Snapshot_Projectile, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_GreatSword_Mounted_Hit, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_GreatSword_Primary_Moving_Hit01, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_GreatSword_Primary_Moving_Hit02, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_GreatSword_Primary_Moving_Hit03, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Longbow_AcrobaticShot_Throw, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Longbow_Primary_Projectile, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Longbow_Sharpshot_Projectile, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Mace_CrushingBlow_Slam_Hit, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Mace_Primary_MeleeAttack_Hit01, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Mace_Primary_MeleeAttack_Hit02, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Mace_Primary_MeleeAttack_Hit03, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Mace_Primary_Mounted_Hit, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Mace_Smack_Hit, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Rapier_Feint_Hit, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Rapier_Lunge_Hit, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Rapier_Lunge_Phase, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Rapier_Lunge_TravelToTarget, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Rapier_MeleeAttack_Hit01, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Rapier_MeleeAttack_Hit02, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Rapier_MeleeAttack_Hit03, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Reaper_HowlingReaper_Hit, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Reaper_HowlingReaper_Projectile, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Reaper_Primary_MeleeAttack_Hit01, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Reaper_Primary_MeleeAttack_Hit02, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Reaper_Primary_MeleeAttack_Hit03, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Reaper_Primary_Mounted_Hit, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Reaper_TendonSwing_Twist_Hit, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Slashers_Camouflage_Secondary_Hit, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Slashers_ElusiveStrike_Dash_PhaseIn, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Slashers_ElusiveStrike_Dash_PhaseOut, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Slashers_Primary_MeleeAttack_Hit01, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Slashers_Primary_MeleeAttack_Hit02, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Slashers_Primary_MeleeAttack_Hit03, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Slashers_Primary_Mounted_Hit, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Spear_Harpoon_Throw_Projectile, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Spear_Primary_MeleeAttack_Hit01, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Spear_Primary_MeleeAttack_Hit02, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Spear_Primary_MeleeAttack_Hit03, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Spear_Primary_Mounted_Hit, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Sword_Primary_MeleeAttack_Hit01, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Sword_Primary_MeleeAttack_Hit02, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Sword_Primary_MeleeAttack_Hit03, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Sword_Primary_Mounted_Hit, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Sword_Shockwave_Main_Projectile, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Sword_Whirlwind_Spin_Hit, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Sword_Whirlwind_Spin_LastHit, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Unarmed_Primary_MeleeAttack_Hit01, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Unarmed_Primary_MeleeAttack_Hit02, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Unarmed_Primary_MeleeAttack_Hit03, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Unarmed_Primary_Mounted_Hit, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_Unarmed_Secondary_Hit, ExtendedColor.Orange},
		{ Prefabs.AB_Vampire_VeilOfBlood_SpellMod_BloodNova, ExtendedColor.Blood},
		{ Prefabs.AB_Vampire_VeilOfBlood_SpellMod_DashInflictLeechBuff, ExtendedColor.Blood},
		{ Prefabs.AB_Vampire_VeilOfBones_BounceProjectile, ExtendedColor.Unholy},
		{ Prefabs.AB_Vampire_VeilOfBones_SpellMod_DashHealMinions, ExtendedColor.Unholy},
		{ Prefabs.AB_Vampire_VeilOfBones_SpellMod_DashInflictCondemnBuff, ExtendedColor.Unholy},
		{ Prefabs.AB_Vampire_VeilOfChaos_Bomb, ExtendedColor.Chaos},
		{ Prefabs.AB_Vampire_VeilOfChaos_SpellMod_BonusDummy_Bomb, ExtendedColor.Chaos},
		{ Prefabs.AB_Vampire_VeilOfFrost_AoE, ExtendedColor.Frost},
		{ Prefabs.AB_Vampire_VeilOfFrost_SpellMod_IllusionFrostBlast, ExtendedColor.Frost},
		{ Prefabs.AB_Vampire_VeilOfIllusion_SpellMod_RecastDetonate, ExtendedColor.Illusion},
		{ Prefabs.AB_Vampire_VeilOfStorm_LightningTriggerBuff, ExtendedColor.Storm},
		{ Prefabs.AB_Vampire_VeilOfStorm_SpellMod_SparklingIllusion, ExtendedColor.Storm},
	};
}
