using System.Collections.Generic;
using System.Linq;
using ProjectM;
using PvpArena.Models;

namespace PvpArena.Data;

public static class JewelData
{
	static JewelData()
	{
		prefabToAbilityNameDictionary = abilityToPrefabDictionary.ToDictionary(pair => pair.Value, pair => pair.Key);
	}

	public static float RANDOM_POWER = -1;

	public static readonly Dictionary<string, PrefabGUID> abilityToPrefabDictionary = new Dictionary<string, PrefabGUID>
	{
		{ "bloodfountain", Prefabs.AB_Blood_BloodFountain_AbilityGroup },
		{ "bloodrage", Prefabs.AB_Blood_BloodRage_AbilityGroup },
		{ "bloodrite", Prefabs.AB_Blood_BloodRite_AbilityGroup },
		{ "sanguinecoil", Prefabs.AB_Blood_SanguineCoil_AbilityGroup },
		{ "shadowbolt", Prefabs.AB_Blood_Shadowbolt_AbilityGroup },
		{ "aftershock", Prefabs.AB_Chaos_Aftershock_Group },
		{ "chaosbarrier", Prefabs.AB_Chaos_Barrier_AbilityGroup },
		{ "powersurge", Prefabs.AB_Chaos_PowerSurge_AbilityGroup },
		{ "void", Prefabs.AB_Chaos_Void_AbilityGroup },
		{ "chaosvolley", Prefabs.AB_Chaos_Volley_AbilityGroup },
		{ "crystallance", Prefabs.AB_Frost_CrystalLance_AbilityGroup },
		{ "frostbat", Prefabs.AB_Frost_FrostBat_AbilityGroup },
		{ "iceblock", Prefabs.AB_Frost_IceBlock_AbilityGroup },
		{ "icenova", Prefabs.AB_Frost_IceNova_AbilityGroup },
		{ "frostbarrier", Prefabs.AB_FrostBarrier_AbilityGroup },
		{ "misttrance", Prefabs.AB_Illusion_MistTrance_AbilityGroup },
		{ "mosquito", Prefabs.AB_Illusion_Mosquito_AbilityGroup },
		{ "phantomaegis", Prefabs.AB_Illusion_PhantomAegis_AbilityGroup },
		{ "spectralwolf", Prefabs.AB_Illusion_SpectralWolf_AbilityGroup },
		{ "wraithspear", Prefabs.AB_Illusion_WraithSpear_AbilityGroup },
		{ "balllightning", Prefabs.AB_Storm_BallLightning_AbilityGroup },
		{ "cyclone", Prefabs.AB_Storm_Cyclone_AbilityGroup },
		{ "discharge", Prefabs.AB_Storm_Discharge_AbilityGroup },
		{ "lightningcurtain", Prefabs.AB_Storm_LightningWall_AbilityGroup },
		{ "polarityshift", Prefabs.AB_Storm_PolarityShift_AbilityGroup },
		{ "boneexplosion", Prefabs.AB_Unholy_CorpseExplosion_AbilityGroup },
		{ "corruptedskull", Prefabs.AB_Unholy_CorruptedSkull_AbilityGroup },
		{ "deathknight", Prefabs.AB_Unholy_DeathKnight_AbilityGroup },
		{ "soulburn", Prefabs.AB_Unholy_Soulburn_AbilityGroup },
		{ "wardofthedamned", Prefabs.AB_Unholy_WardOfTheDamned_AbilityGroup },
		{ "veilofblood", Prefabs.AB_Vampire_VeilOfBlood_Group },
		{ "veilofbones", Prefabs.AB_Vampire_VeilOfBones_AbilityGroup },
		{ "veilofchaos", Prefabs.AB_Vampire_VeilOfChaos_Group },
		{ "veiloffrost", Prefabs.AB_Vampire_VeilOfFrost_Group },
		{ "veilofillusion", Prefabs.AB_Vampire_VeilOfIllusion_AbilityGroup },
		{ "veilofstorm", Prefabs.AB_Vampire_VeilOfStorm_Group }
	};

	public static readonly Dictionary<PrefabGUID, string> prefabToAbilityNameDictionary = new Dictionary<PrefabGUID, string>();

	public static List<PrefabData> JewelPrefabData = new List<PrefabData>()
	{
		new PrefabData(Prefabs.AB_Blood_BloodFountain_AbilityGroup, "AB_Blood_BloodFountain_AbilityGroup",
			"Blood Fountain"),
		new PrefabData(Prefabs.AB_Blood_BloodRage_AbilityGroup, "AB_Blood_BloodRage_AbilityGroup", "Blood Rage"),
		new PrefabData(Prefabs.AB_Blood_BloodRite_AbilityGroup, "AB_Blood_BloodRite_AbilityGroup", "Blood Rite"),
		new PrefabData(Prefabs.AB_Blood_SanguineCoil_AbilityGroup, "AB_Blood_SanguineCoil_AbilityGroup",
			"Sanguine Coil"),
		new PrefabData(Prefabs.AB_Blood_Shadowbolt_AbilityGroup, "AB_Blood_Shadowbolt_AbilityGroup", "Shadow Bolt"),
		new PrefabData(Prefabs.AB_Chaos_Aftershock_Group, "AB_Chaos_Aftershock_Group", "Aftershock"),
		new PrefabData(Prefabs.AB_Chaos_Barrier_AbilityGroup, "AB_Chaos_Barrier_AbilityGroup", "Chaos Barrier"),
		new PrefabData(Prefabs.AB_Chaos_PowerSurge_AbilityGroup, "AB_Chaos_PowerSurge_AbilityGroup", "Power Surge"),
		new PrefabData(Prefabs.AB_Chaos_Void_AbilityGroup, "AB_Chaos_Void_AbilityGroup", "Void"),
		new PrefabData(Prefabs.AB_Chaos_Volley_AbilityGroup, "AB_Chaos_Volley_AbilityGroup", "Chaos Volley"),
		new PrefabData(Prefabs.AB_Frost_CrystalLance_AbilityGroup, "AB_Frost_CrystalLance_AbilityGroup",
			"Crystal Lance"),
		new PrefabData(Prefabs.AB_Frost_FrostBat_AbilityGroup, "AB_Frost_FrostBat_AbilityGroup", "Frost Bat"),
		new PrefabData(Prefabs.AB_Frost_IceBlock_AbilityGroup, "AB_Frost_IceBlock_AbilityGroup", "Ice Block"),
		new PrefabData(Prefabs.AB_Frost_IceNova_AbilityGroup, "AB_Frost_IceNova_AbilityGroup", "Ice Nova"),
		new PrefabData(Prefabs.AB_FrostBarrier_AbilityGroup, "AB_FrostBarrier_AbilityGroup", "Frost Barrier"),
		new PrefabData(Prefabs.AB_Illusion_MistTrance_AbilityGroup, "AB_Illusion_MistTrance_AbilityGroup",
			"Mist Trance"),
		new PrefabData(Prefabs.AB_Illusion_Mosquito_AbilityGroup, "AB_Illusion_Mosquito_AbilityGroup", "Mosquito"),
		new PrefabData(Prefabs.AB_Illusion_PhantomAegis_AbilityGroup, "AB_Illusion_PhantomAegis_AbilityGroup",
			"Phantom Aegis"),
		new PrefabData(Prefabs.AB_Illusion_SpectralWolf_AbilityGroup, "AB_Illusion_SpectralWolf_AbilityGroup",
			"Spectral Wolf"),
		new PrefabData(Prefabs.AB_Illusion_WraithSpear_AbilityGroup, "AB_Illusion_WraithSpear_AbilityGroup",
			"Wraith Spear"),
		new PrefabData(Prefabs.AB_Storm_BallLightning_AbilityGroup, "AB_Storm_BallLightning_AbilityGroup",
			"Ball Lightning"),
		new PrefabData(Prefabs.AB_Storm_Cyclone_AbilityGroup, "AB_Storm_Cyclone_AbilityGroup", "Cyclone"),
		new PrefabData(Prefabs.AB_Storm_Discharge_AbilityGroup, "AB_Storm_Discharge_AbilityGroup", "Discharge"),
		new PrefabData(Prefabs.AB_Storm_LightningWall_AbilityGroup, "AB_Storm_LightningWall_AbilityGroup",
			"Lightning Curtain"),
		new PrefabData(Prefabs.AB_Storm_PolarityShift_AbilityGroup, "AB_Storm_PolarityShift_AbilityGroup",
			"Polarity Shift"),
		new PrefabData(Prefabs.AB_Unholy_CorpseExplosion_AbilityGroup, "AB_Unholy_CorpseExplosion_AbilityGroup",
			"Bone Explosion"),
		new PrefabData(Prefabs.AB_Unholy_CorruptedSkull_AbilityGroup, "AB_Unholy_CorruptedSkull_AbilityGroup",
			"Corrupted Skull"),
		new PrefabData(Prefabs.AB_Unholy_DeathKnight_AbilityGroup, "AB_Unholy_DeathKnight_AbilityGroup",
			"Death Knight"),
		new PrefabData(Prefabs.AB_Unholy_Soulburn_AbilityGroup, "AB_Unholy_Soulburn_AbilityGroup", "Soulburn"),
		new PrefabData(Prefabs.AB_Unholy_WardOfTheDamned_AbilityGroup, "AB_Unholy_WardOfTheDamned_AbilityGroup",
			"Ward of the Damned"),
		new PrefabData(Prefabs.AB_Vampire_VeilOfBlood_Group, "AB_Vampire_VeilOfBlood_Group", "Veil of Blood"),
		new PrefabData(Prefabs.AB_Vampire_VeilOfBones_AbilityGroup, "AB_Vampire_VeilOfBones_AbilityGroup",
			"Veil of Bones"),
		new PrefabData(Prefabs.AB_Vampire_VeilOfChaos_Group, "AB_Vampire_VeilOfChaos_Group", "Veil of Chaos"),
		new PrefabData(Prefabs.AB_Vampire_VeilOfFrost_Group, "AB_Vampire_VeilOfFrost_Group", "Veil of Frost"),
		new PrefabData(Prefabs.AB_Vampire_VeilOfIllusion_AbilityGroup, "AB_Vampire_VeilOfIllusion_AbilityGroup",
			"Veil of Illusion"),
		new PrefabData(Prefabs.AB_Vampire_VeilOfStorm_Group, "AB_Vampire_VeilOfStorm_Group", "Veil of Storm")
	};

	public static readonly Dictionary<string, SchoolData> abilityToSchoolDictionary = new Dictionary<string, SchoolData>
	{
		{ "bloodfountain", SchoolData.Blood },
		{ "bloodrage", SchoolData.Blood },
		{ "bloodrite", SchoolData.Blood },
		{ "sanguinecoil", SchoolData.Blood },
		{ "shadowbolt", SchoolData.Blood },
		{ "aftershock", SchoolData.Chaos },
		{ "chaosbarrier", SchoolData.Chaos },
		{ "powersurge", SchoolData.Chaos },
		{ "void", SchoolData.Chaos },
		{ "chaosvolley", SchoolData.Chaos },
		{ "crystallance", SchoolData.Frost },
		{ "frostbat", SchoolData.Frost },
		{ "iceblock", SchoolData.Frost },
		{ "icenova", SchoolData.Frost },
		{ "frostbarrier", SchoolData.Frost },
		{ "misttrance", SchoolData.Illusion },
		{ "mosquito", SchoolData.Illusion },
		{ "phantomaegis", SchoolData.Illusion },
		{ "spectralwolf", SchoolData.Illusion },
		{ "wraithspear", SchoolData.Illusion },
		{ "balllightning", SchoolData.Storm },
		{ "cyclone", SchoolData.Storm },
		{ "discharge", SchoolData.Storm },
		{ "lightningcurtain", SchoolData.Storm },
		{ "polarityshift", SchoolData.Storm },
		{ "boneexplosion", SchoolData.Unholy },
		{ "corruptedskull", SchoolData.Unholy },
		{ "deathknight", SchoolData.Unholy },
		{ "soulburn", SchoolData.Unholy },
		{ "wardofthedamned", SchoolData.Unholy },
		{ "veilofblood", SchoolData.Blood },
		{ "veilofbones", SchoolData.Unholy },
		{ "veilofchaos", SchoolData.Chaos },
		{ "veiloffrost", SchoolData.Frost },
		{ "veilofillusion", SchoolData.Illusion },
		{ "veilofstorm", SchoolData.Storm },
	};

	public static Dictionary<string, List<KeyValuePair<PrefabGUID, string>>> SpellMods =
		new Dictionary<string, List<KeyValuePair<PrefabGUID, string>>>
		{
			{
				"bloodfountain", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodFountain_RecastLesser,
						"Recast a smaller " + "Blood Fountain".Colorify(ExtendedColor.LightBlood)),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodFountain_FirstImpactApplyLeech,
						"Hit applies " + "Leech".Colorify(ExtendedColor.Blood)),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodFountain_FirstImpactFadingSnare,
						"Hit applies " + "fading snare".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodFountain_FirstImpactDispell,
						"Hit " + "removes negative effects".Emphasize() + " from allies"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodFountain_SecondImpactKnockback,
						"Explosion " + "pushes".Emphasize() + " enemies " + "back".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodFountain_SecondImpactSpeedBuff,
						"Explosion increases ally " + "MS".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodFountain_ConsumeLeechBonusDamage,
						"Explosion consumes " + "Leech".Colorify(ExtendedColor.Blood) + " to deal " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodFountain_FirstImpactHealIncrease,
						"Increase hit " + "healing".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodFountain_SecondImpactDamageIncrease,
						"Increase explosion " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodFountain_SecondImpactHealIncrease,
						"Increase explosion " + "healing".Emphasize()),
				}
			},
			{
				"bloodrage", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodRage_HealOnKill,
						"Kill an enemy to " + "heal".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_DispellDebuffs,
						"Cast " + "removes".Emphasize() + " all " + "negative effects".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodRage_Shield,
						"Cast grants a " + "shield".Emphasize() + " to caster and allies"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_ApplyFadingSnare_Medium,
						"Cast applies a " + "fading snare".Emphasize() + " on enemies"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodRage_DamageBoost,
						"Increases " + "physical power".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodRage_IncreaseLifetime,
						"Increase "+ "effect duration".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodRage_IncreaseMoveSpeed, "Increase " + "MS".Emphasize()),
				}
			},
			{
				"bloodrite", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodRite_Stealth,
						"Turn " + "invisible".Emphasize() + " while " + "immaterial".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodRite_ConsumeLeechHealXTimes,
						"Trigger consumes " + "Leech".Colorify(ExtendedColor.Blood) + " to " + "heal".Emphasize() + " per target"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodRite_ConsumeLeechReduceCooldownXTimes,
						"Trigger consumes " + "Leech".Colorify(ExtendedColor.Blood) + " to " + "decrease CD".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_ApplyFadingSnare_Medium,
						"Trigger applies a " + "fading snare".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodRite_DamageOnAttack,
						"Trigger for first auto to deal " + "bonus damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_IncreaseMoveSpeedDuringChannel_High,
						"Increase " + "MS".Emphasize() + " during channel"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodRite_IncreaseLifetime,
						"Increase " + "immaterial".Emphasize() + " duration"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodRite_BonusDamage, "Increase " + "damage".Emphasize()),
				}
			},
			{
				"sanguinecoil", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_SanguineCoil_KillRecharge,
						"Lethal attacks " + "restore 1 charge".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_SanguineCoil_ConsumeLeechBonusDamage,
						"Hit consumes " + "Leech".Colorify(ExtendedColor.Blood) + " to deal " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Blood_ConsumeLeechSelfHeal,
						"Hit consumes " + "Leech".Colorify(ExtendedColor.Blood) + " to " + "heal".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_SanguineCoil_AddBounces,
						"Hit " + "bounces".Emphasize() + " to an " + "additional target".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_AddCharges, "Increase "+ "charges".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_SanguineCoil_BonusDamage, "Increase " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_SanguineCoil_BonusHealing,
						"Increase " + "healing".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Projectile_RangeAndVelocity,
						"Increase " + "projectile range".Emphasize() + " and " + "speed".Emphasize()),
				}
			},
			{
				"shadowbolt", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shadowbolt_ConsumeLeechBonusDamage,
						"Hit consumes " + "Leech".Colorify(ExtendedColor.Blood) + " to " + "deal damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Blood_ConsumeLeechSelfHeal,
						"Hit consumes " + "Leech".Colorify(ExtendedColor.Blood) + " to " + "heal".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shadowbolt_ExplodeOnHit,
						"Hit conjures an " + "AoE".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shadowbolt_ForkOnHit,
						"Hit forks into " + "2 projectiles".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_KnockbackOnHit_Medium,
						"Hit " + "pushes".Emphasize() + " enemies " + "back".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Projectile_RangeAndVelocity,
						"Increase " + "projectile range".Emphasize() + " and " + "speed".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Cooldown_Medium, "Decrease " + "CD".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_CastRate, "Decrease " + "cast time".Emphasize()),
				}
			},
			{
				"veilofblood", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfBlood_DashInflictLeech,
						"Dashing through an enemy applies " + "Leech".Colorify(ExtendedColor.Blood)),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfBlood_BloodNova,
						"Next auto conjurs an " + "AoE".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfBlood_Empower,
						"Next auto consumes " + "Leech".Colorify(ExtendedColor.Blood) + " for " + "phys power".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfBlood_AttackInflictFadingSnare,
						"Next primary attack applies a " + "fading snare".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Veil_BuffAndIllusionDuration,
						"Increase " + "elude".Emphasize() + " duration"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Veil_BonusDamageOnPrimary,
						"Increase " + "damage".Emphasize() + " of next " + "primary attack".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfBlood_SelfHealing, "Increase " + "healing".Emphasize()),
				}
			},
			{
				"aftershock", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Aftershock_KnockbackArea,
						"Cast " + "knocks".Emphasize() + " enemies " + "back".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Aftershock_InflictSlowOnProjectile,
						"Cast applies a " + "fading snare".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Chaos_ConsumeIgniteIntoCombustion,
						"Explosion consumes " + "Ignite".Colorify(ExtendedColor.Chaos) + " to conjure an " + "AoE".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Chaos_ConsumeIgniteIntoHeated,
						"Explosion consumes " + "Ignite".Colorify(ExtendedColor.Chaos) + " to increase " + "MS".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Aftershock_BonusDamage,
						"Increase " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Projectile_IncreaseRange_Medium,
						"Increase " + "projectile range".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Cooldown_Medium, "Decrease " + "CD".Emphasize()),
				}
			},
			{
				"chaosbarrier", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Barrier_ArcProjectiles,
						"Recast launches " + "3 projectiles".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Barrier_StunOnAbsorbMelee,
						"Barrier hit applies " + "Stun".Colorify(ExtendedColor.Chaos) + " to the " + "attacker".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Chaos_ConsumeIgniteIntoCombustion,
						"Projectile hit consumes " + "Ignite".Colorify(ExtendedColor.Chaos) + " to conjure an " + "AoE".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Barrier_ForkOnHit,
						"Projectile hit forks into " + "2 projectiles".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Barrier_BonusDamage, "Increase " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_IncreaseMoveSpeedDuringChannel_Low,
						"Increase " + "MS".Emphasize() + " during channel"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Barrier_IncreasePullRange,
						"Increase " + "pull distance".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(
						Prefabs.SpellMod_Chaos_Barrier_ConsumeAttackReduceCooldownXTimes,
						"Decrease " + "CD".Emphasize() + " on " + "absorbed hit".Emphasize()),
				}
			},
			{
				"chaosvolley", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Volley_SecondProjectileBonusDamage,
						"Bonus " + "dmg".Emphasize() + " for hitting other enemy with 2nd shot"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Chaos_ConsumeIgniteIntoCombustion,
						"Hit consumes " + "Ignite".Colorify(ExtendedColor.Chaos) + " to conjure an " + "AoE".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Chaos_ConsumeIgniteIntoHeated,
						"Hit consumes " + "Ignite".Colorify(ExtendedColor.Chaos) + " to increase " + "MS".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_KnockbackOnHit_Light,
						"Hit " + "pushes".Emphasize() + " enemies " + "back".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Volley_BonusDamage, "Increase " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Projectile_RangeAndVelocity,
						"Increase " + "projectile range".Emphasize() + " and " + "speed".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Cooldown_Medium, "Decrease " + "CD".Emphasize()),
				}
			},
			{
				"powersurge", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_PowerSurge_RecastDestonate,
						"Recast to " + "pull".Emphasize() + " enemies toward the target"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_DispellDebuffs,
						"Removes all " + "negative effects".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_PowerSurge_Shield, "Apply a " + "shield".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_PowerSurge_IncreaseDurationOnKill,
						"Lethal attacks during the effect " + "reduce CD".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_PowerSurge_AttackSpeed, "Increase " + "AS".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_PowerSurge_Haste, "Increase " + "MS".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_PowerSurge_Lifetime,
						"Increase " + "effect duration".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_PowerSurge_EmpowerPhysical,
						"Increase " + "physical power".Emphasize()),
				}
			},
			{
				"void", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Chaos_ConsumeIgniteIntoCombustion,
						"Explosion consumes " + "Ignite".Colorify(ExtendedColor.Chaos) + " to conjure an " + "AoE".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Chaos_ConsumeIgniteIntoHeated,
						"Explosion consume " + "Ignite".Colorify(ExtendedColor.Chaos) + " to increase " + "MS".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Void_FragBomb,
						"Explosion conjures " + "3 AoEs".Emphasize() + " that explode"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Void_BurnArea,
						"Explosion leaves behind an " + "AoE".Emphasize() + " that deals " + "dmg".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Void_BonusDamage, "Increase " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_TargetAoE_IncreaseRange_Medium,
						"Increase " + "range".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Void_ReduceChargeCD,
						"Increase " + "recharge rate".Emphasize()),
				}
			},
			{
				"veilofchaos", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfChaos_BonusIllusion,
						"Recast conjurs a second exploding " + "illusion".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfChaos_ApplySnareOnExplode,
						"Explosion applies a " + "fading snare".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(
						Prefabs.SpellMod_Shared_Chaos_ConsumeIgniteIntoCombustion_OnAttack,
						"Next auto consumes " + "Ignite".Colorify(ExtendedColor.Chaos) + " to conjure an " + "AoE".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Chaos_ConsumeIgniteIntoHeated_OnAttack,
						"Next auto consumes " + "Ignite".Colorify(ExtendedColor.Chaos) + " to increase " + "MS".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Veil_BuffAndIllusionDuration,
						"Increase " + "elude duration".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfChaos_BonusDamageOnExplode,
						"Increase explosion " + "damage".Emphasize() + " of any " + "illusion".Emphasize()),
				}
			},
			{
				"crystallance", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_CrystalLance_PierceEnemies,
						"Projectile " + "pierces".Emphasize() + " dealing " + "reduced damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Frost_SplinterNovaOnFrosty,
						"Hit on a " + "Chilled/Frozen".Colorify(ExtendedColor.Frost) + " enemy throws projectiles"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_CrystalLance_BonusDamageToFrosty,
						"Increase damage to " + "Chilled/Frozen".Colorify(ExtendedColor.Frost) + " enemies"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_CrystalLance_IncreaseFreeze,
						"Increase " + "Freeze".Colorify(ExtendedColor.Frost) + " duration to " + "Chilled".Colorify(ExtendedColor.Frost) + " enemies"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Projectile_RangeAndVelocity,
						"Increase " + "projectile range".Emphasize() + " and " + "speed".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_CastRate, "Decrease " + "cast time".Emphasize()),
				}
			},
			{
				"frostbarrier", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(
						Prefabs.SpellMod_FrostBarrier_ConsumeAttackReduceCooldownXTimes, "Barrier hits decrease " + "CD".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Frost_ConsumeChillIntoFreeze_Recast,
						"Recast consumes " + "Chill".Colorify(ExtendedColor.Frost) + " and applies " + "Freeze".Colorify(ExtendedColor.Frost)),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_FrostBarrier_KnockbackOnRecast,
						"Recast "+ "pushes".Emphasize() + " enemies " + "back".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_FrostBarrier_ShieldOnFrostyRecast,
						"Recast " + "shields".Emphasize() + " caster when hitting " + "Chilled".Colorify(ExtendedColor.Frost) + " enemy"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_FrostBarrier_BonusDamage, "Increase " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_FrostBarrier_BonusSpellPowerOnAbsorb,
						"Increase " + "spell power".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_IncreaseMoveSpeedDuringChannel_Low,
						"Increase " + "MS".Emphasize() + " during channel"),
				}
			},
			{
				"frostbat", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Frost_SplinterNovaOnFrosty,
						"Hitting " + "Chilled/Frozen".Colorify(ExtendedColor.Frost) + " enemy launches " + "projectiles".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Frost_ShieldOnFrosty,
						"Hit on an " + "Chilled/Frozen".Colorify(ExtendedColor.Frost) + " enemy shields caster"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_FrostBat_AreaDamage,
						"Hit conjures an " + "AoE".Emphasize() + " that deals " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_FrostBat_BonusDamageToFrosty,
						"Increase damage to " + "Chilled/Frozen".Colorify(ExtendedColor.Frost) + " enemies"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Projectile_RangeAndVelocity,
						"Increase " + "projectile range".Emphasize() + " and " + "speed".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_CastRate, "Decrease " + "cast time".Emphasize()),
				}
			},
			{
				"iceblock", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_IceBlock_InflictChillOnAttackers,
						"Chill".Colorify(ExtendedColor.Frost) + " attackers if hit by physical damage"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_IceBlock_FrostWeapon,
						"Next auto deals " + "damage".Emphasize() + " and inflicts " + "Chill".Colorify(ExtendedColor.Frost)),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Frost_ConsumeChillIntoFreeze_Nova,
						"Conjure a caster-centred " + "AoE".Emphasize() + " once the spell ends"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_IceBlock_BonusAbsorb, "Increase " + "shield".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_IceBlock_BonusHealing, "Increase " + "healing".Emphasize()),
				}
			},
			{
				"icenova", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_IceNova_RecastLesserNova,
						"Recast to conjure an " + "AoE".Emphasize() + " that explodes"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Frost_ConsumeChillIntoFreeze,
						"Explosion consumes " + "Chill".Colorify(ExtendedColor.Frost) + " to apply " + "Freeze".Colorify(ExtendedColor.Frost)),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_IceNova_ApplyShield,
						"Explosion " + "shields".Emphasize() + " caster and allies"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_TargetAoE_IncreaseRange_Medium,
						"Increase " + "range".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_IceNova_BonusDamageToFrosty,
						"Increase " + "damage".Emphasize() + " to " + "Chilled/Frozen".Colorify(ExtendedColor.Frost) + " enemies"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Cooldown_Medium, "Decrease " + "CD".Emphasize()),
				}
			},
			{
				"veiloffrost", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Frost_ConsumeChillIntoFreeze_OnAttack,
						"Next auto consumes " + "Chill".Colorify(ExtendedColor.Frost) + " and applies " + "Freeze".Colorify(ExtendedColor.Frost) + ""),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Frost_ShieldOnFrosty,
						"Next auto on a " + "Chilled".Colorify(ExtendedColor.Frost) + " enemy " + "shields".Emphasize() + " caster"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfFrost_IllusionFrostBlast,
						"Illusion explodes in an " + "AoE".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Veil_BuffAndIllusionDuration,
						"Increase " + "elude duration".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfFrost_BonusDamage,
						"Increase " + "damage".Emphasize() + " of next primary " + "attack".Emphasize()),
				}
			},
			{
				"misttrance", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_MistTrance_ReduceSecondaryWeaponCD,
						"Trigger " + "reduces secondary weapon skill CD".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_MistTrance_PhantasmOnTrigger,
						"Trigger grants " + "Phantasm".Colorify(ExtendedColor.Illusion) + ""),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_MistTrance_HasteOnTrigger,
						"Trigger increases " + "MS".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_MIstTrance_DamageOnAttack,
						"Trigger increases " + "first primary attack damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_MistTrance_FearOnTrigger,
						"Trigger applies " + "Fear".Colorify(ExtendedColor.Illusion) + " to enemies near caster"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_KnockbackOnHit_Medium,
						"Trigger " + "pushes".Emphasize() + " enemies " + "back".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Illusion_ConsumeWeakenShield_Counter,
						"Teleport consumes " + "Weaken".Colorify(ExtendedColor.Illusion) + " to grant a " + "shield".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_IncreaseMoveSpeedDuringChannel_High,
						"Increase " + "MS".Emphasize() + " during channel"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_TravelBuff_IncreaseRange_Medium,
						"Increase " + "distance".Emphasize() + " travelled"),
				}
			},
			{
				"mosquito", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Mosquito_ShieldOnSpawn,
						"Cast " + "shields".Emphasize() + " caster and allies"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Illusion_ConsumeWeakenReduceCooldown,
						"Explosion consumes " + "Weaken".Colorify(ExtendedColor.Illusion) + " to reduce " + "CD".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Mosquito_WispsOnDestroy,
						"Explosion summons 3 " + "Wisps".Colorify(ExtendedColor.Illusion) + " that " + "heal".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Mosquito_BonusDamage, "Increase " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Mosquito_BonusFearDuration,
						"Increase " + "Fear".Colorify(ExtendedColor.Illusion) + " duration".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Mosquito_BonusHealthAndSpeed,
						"Increase summon max " + "HP".Emphasize() + " and " + "MS".Emphasize()),
				}
			},
			{
				"phantomaegis", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_PhantomAegis_ConsumeShieldAndPullAlly,
						"Recast to remove the effect and " + "pull the target".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Illusion_ConsumeWeakenReduceCooldown,
						"Cast consumes " + "Weaken".Colorify(ExtendedColor.Illusion) + " to reduce " + "CD".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_PhantomAegis_ConsumeWeakenIntoFear,
						"Cast consumes " + "Weaken".Colorify(ExtendedColor.Illusion) + " to apply " + "Fear".Colorify(ExtendedColor.Illusion)),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_DispellDebuffs,
						"Cast " + "removes all negative effects".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_KnockbackOnHit_Medium,
						"Cast " + "knocks".Emphasize() + " enemies " + "back".Emphasize() + " from target"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_PhantomAegis_ExplodeOnDestroy,
						"Expiration conjurs a target-centred " + "AoE".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_PhantomAegis_IncreaseLifetime,
						"Increase " + "duration".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_MovementSpeed_Normal, "Increase " + "MS".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_PhantomAegis_IncreaseSpellPower,
						"Increase " + "spell power".Emphasize()),
				}
			},
			{
				"spectralwolf", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_SpectralWolf_FirstBounceInflictFadingSnare,
						"First hit applies a " + "fading snare".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Illusion_ConsumeWeakenShield,
						"Hit consumes " + "Weaken".Colorify(ExtendedColor.Illusion) + " to grant a " + "shield".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_SpectralWolf_ConsumeWeakenApplyXPhantasm,
						"Hit consumes " + "Weaken".Colorify(ExtendedColor.Illusion) + " to grant " + "Phantasm".Colorify(ExtendedColor.Illusion)),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Illusion_ConsumeWeakenSpawnWisp,
						"Hit consumes " + "Weaken".Colorify(ExtendedColor.Illusion) + " to spawn a " + "Wisp".Colorify(ExtendedColor.Illusion)),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_SpectralWolf_ReturnToOwner,
						"Hit returns to caster on last bounce to " + "heal".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_SpectralWolf_AddBounces,
						"Increase " + "max bounces".Emphasize() + " by " + "1".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Projectile_RangeAndVelocity,
						"Increase " + "projectile range".Emphasize() + " and " + "speed".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_SpectralWolf_DecreaseBounceDamageReduction,
						"Decrease " + "damage penalty".Emphasize() + " per bounce"),
				}
			},
			{
				"wraithspear", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Illusion_ConsumeWeakenSpawnWisp,
						"Hit summons a " + "Wisp".Colorify(ExtendedColor.Illusion) + " that " + "heals".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Illusion_ConsumeWeakenShield,
						"Hit consumes " + "Weaken".Colorify(ExtendedColor.Illusion) + " to " + "shield".Emphasize() + " per target"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_WraithSpear_ShieldAlly,
						"Hit grants allies a " + "shield".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_ApplyFadingSnare_Medium,
						"Hit applies a " + "fading snare".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_WraithSpear_BonusDamage, "Increase " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Projectile_IncreaseRange_Medium,
						"Increase " + "projectile range".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_WraithSpear_ReducedDamageReduction,
						"Decrease " + "damage penalty".Emphasize() + " per hit"),
				}
			},
			{
				"veilofillusion", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfIllusion_RecastDetonate,
						"Recast to " + "explode".Emphasize() + " the " + "illusion".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfIllusion_IllusionProjectileDamage,
						"Illusion projectiles deal " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfIllusion_PhantasmOnHit,
						"Next primary attack grants " + "Phantasm".Colorify(ExtendedColor.Illusion)),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Veil_BonusDamageOnPrimary,
						"Next primary attack deals " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfIllusion_AttackInflictFadingSnare,
						"Next primary attack applies a " + "fading snare".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Illusion_ConsumeWeakenShield_OnAttack,
						"Next auto consumes " + "Weaken".Colorify(ExtendedColor.Illusion) + " to grant " + "shield".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Veil_BuffAndIllusionDuration,
						"Increase " + "elude duration".Emphasize()),
				}
			},
			{
				"balllightning", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BallLightning_DetonateOnRecast,
						"Recast " + "detonates".Emphasize() + " the ball to deal " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Storm_ConsumeStaticIntoStun_Explode,
						"Explosion consumes " + "Static".Colorify(ExtendedColor.Storm) + " to apply " + "Stun".Colorify(ExtendedColor.Storm)),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BallLightning_KnockbackOnExplode,
						"Explosion " + "pushes".Emphasize() + " enemies " + "back".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BallLightning_Haste,
						"Explosion increases caster and ally " + "MS".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BallLightning_BonusDamage,
						"Increase tick " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Projectile_IncreaseRange_Medium,
						"Increase " + "projectile range".Emphasize()),
				}
			},
			{
				"cyclone", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Storm_ConsumeStaticIntoStun,
						"Hit consumes " + "Static".Colorify(ExtendedColor.Storm) + " to apply " + "Stun".Colorify(ExtendedColor.Storm) + ""),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Storm_ConsumeStaticIntoWeaponCharge,
						"Hit consumes " + "Static".Colorify(ExtendedColor.Storm) + " for bonus auto " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Cyclone_BonusDamage, "Increase " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Projectile_RangeAndVelocity,
						"Increase " + "projectile range".Emphasize() + " and " + "speed".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Cyclone_IncreaseLifetime,
						"Increase " + "projectile duration".Emphasize()),
				}
			},
			{
				"discharge", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Discharge_DoubleDash,
						"Trigger to " + "travel a second time".Emphasize() + " after first travel"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Storm_ConsumeStaticIntoWeaponCharge,
						"Consumes " + "Static".Colorify(ExtendedColor.Storm) + " for bonus auto " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Discharge_BonusDamage, "Increase " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_IncreaseMoveSpeedDuringChannel_High,
						"Increase " + "MS".Emphasize() + " during channel"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Discharge_IncreaseAirDuration,
						"Increase " + "airborne duration".Emphasize()),
				}
			},
			{
				"lightningcurtain", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_LightningWall_FadingSnare,
						"Hit applies a " + "fading snare".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_LightningWall_ApplyShield,
						"Hit on caster or ally grants a " + "shield".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Storm_ConsumeStaticIntoStun,
						"Hit to consume " + "Static".Colorify(ExtendedColor.Storm) + " and apply " + "Stun".Colorify(ExtendedColor.Storm) + ""),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_LightningWall_ConsumeProjectileWeaponCharge,
						"Block projectiles for " + "bonus auto damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_LightningWall_BonusDamage,
						"Increase tick " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_LightningWall_IncreaseMovementSpeed,
						"Increase " + "MS".Emphasize()),
				}
			},
			{
				"polarityshift", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Storm_PolarityShift_AreaImpactOrigin,
						"Hit conjures an " + "AoE".Emphasize() + " at the caster's location"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Storm_ConsumeStaticIntoWeaponCharge,
						"Hit consumes " + "Static".Colorify(ExtendedColor.Storm) + " for bonus auto " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_ApplyFadingSnare_Medium,
						"Hit applies a " + "fading snare".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Storm_PolarityShift_AreaImpactDestination,
						"Teleport conjures an " + "AoE".Emphasize() + " at the target's location"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Projectile_RangeAndVelocity,
						"Increase " + "projectile range".Emphasize() + " and " + "speed".Emphasize()),
				}
			},
			{
				"veilofstorm", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfStorm_SparklingIllusion,
						"Illusion conjures an " + "AoE".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Veil_BonusDamageOnPrimary,
						"Next primary attack deals " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Storm_ConsumeStaticIntoStun,
						"Next primary attack consumes " + "Static".Colorify(ExtendedColor.Storm) + " to " + "Stun".Colorify(ExtendedColor.Storm)),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfStorm_AttackInflictFadingSnare,
						"Next primary attack applies a " + "fading snare".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Veil_BuffAndIllusionDuration,
						"Increase " + "elude duration".Emphasize()),
				}
			},
			{
				"boneexplosion", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Unholy_ApplyAgony,
						"Hit on " + "Condemned".Colorify(ExtendedColor.Unholy) + " enemy applies " + "Agony".Colorify(ExtendedColor.Unholy)),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_CorpseExplosion_KillingBlow,
						"Hit on low HP enemy deals bonus " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_CorpseExplosion_SkullNova,
						"Explosion conjures " + "projectiles".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_CorpseExplosion_DoubleImpact,
						"Explosion conjures a 2nd " + "AoE".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_CorpseExplosion_HealMinions,
						"Explosion " +  "heals skeleton".Emphasize() + " and resets its uptime"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_CorpseExplosion_SnareBonus,
						"Explosion applies a " + "fading snare".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_CorpseExplosion_BonusDamage,
						"Increase " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_TargetAoE_IncreaseRange_Medium,
						"Increase " + "range".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Cooldown_Medium, "Decrease " + "CD".Emphasize()),
				}
			},
			{
				"corruptedskull", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_CorruptedSkull_LesserProjectiles,
						"Launch " + "2 projectiles".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_CorruptedSkull_DetonateSkeleton,
						"Hit on allied skeleton causes it to " + "explode".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Unholy_ApplyAgony,
						"Hit on a " + "Condemned".Colorify(ExtendedColor.Unholy) + " enemy applies " + "Agony".Colorify(ExtendedColor.Unholy)),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Unholy_ApplyBane,
						"Hit on a " + "Condemned".Colorify(ExtendedColor.Unholy) + " enemy applies " + "Bane".Colorify(ExtendedColor.Unholy)),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_KnockbackOnHit_Medium,
						"Hit " + "pushes".Emphasize() + " enemies " + "back".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_CorruptedSkull_BoneSpirit,
						"Hit conjures a " + "projectile".Emphasize() + " that circles enemy"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_CorruptedSkull_BonusDamage,
						"Increase " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Projectile_RangeAndVelocity,
						"Increase " + "projectile range".Emphasize() + " and " + "speed".Emphasize()),
				}
			},
			{
				"deathknight", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_DeathKnight_SnareEnemiesOnSummon,
						"Cast applies a " + "fading snare".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Unholy_ApplyAgony,
						"Hit on a " + "Condemned".Colorify(ExtendedColor.Unholy) + " enemy applies " + "Agony".Colorify(ExtendedColor.Unholy)),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Unholy_ApplyBane,
						"Hit on a " + "Condemned".Colorify(ExtendedColor.Unholy) + " enemy applies " + "Bane".Colorify(ExtendedColor.Unholy)),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_DeathKnight_SkeletonMageOnDeath,
						"Hit summons a " + "Skeleton Mage".Colorify(ExtendedColor.Unholy)),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_DeathKnight_BonusDamage, "Increase " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_DeathKnight_BonusDamageBelowTreshhold,
						"Increase " + "damage".Emphasize() + " to enemies " + "below 30% HP".Emphasize()),
				}
			},
			{
				"soulburn", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_DispellDebuffs_Self,
						"Cast " + "removes all negative effects".Emphasize() + " from caster"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Soulburn_ConsumeSkeletonEmpower,
						"Cast consumes skeletons to " + "increase power".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Soulburn_ConsumeSkeletonHeal,
						"Cast consumes skeletons to " + "heal".Emphasize() + " per skeleton"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Unholy_ApplyAgony,
						"Hit on a " + "Condemned".Colorify(ExtendedColor.Unholy) + " enemy applies " + "Agony".Colorify(ExtendedColor.Unholy)),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Soulburn_IncreaseTriggerCount,
						"Increase targets hit by " + "1".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Soulburn_IncreasedSilenceDuration,
						"Increase " + "Silence".Colorify(ExtendedColor.Unholy) + " duration"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Soulburn_BonusDamage, "Increase " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Soulburn_BonusLifeDrain,
						"Increase " + "life drain".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Soulburn_ReduceCooldownOnSilence,
						"Decrease " + "CD".Emphasize() +  " for each " + "Silenced".Colorify(ExtendedColor.Unholy) + " enemy"),
				}
			},
			{
				"wardofthedamned", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_WardOfTheDamned_MightSpawnMageSkeleton,
						"Barrier hit can summon a " + "Skeleton Mage".Colorify(ExtendedColor.Unholy)),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_WardOfTheDamned_DamageMeleeAttackers,
						"Melee barrier hits deal " + "damage".Emphasize() + " to attacker"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_WardOfTheDamned_HealOnAbsorbProjectile,
						"Projectile barrier hits " + "heal".Emphasize() + " you"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_WardOfTheDamned_KnockbackOnRecast,
						"Recast " + "pushes".Emphasize() + " enemies " + "back".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_WardOfTheDamned_EmpowerSkeletonsOnRecast,
						"Recast increases allied skeleton " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_WardOfTheDamned_ShieldSkeletonsOnRecast,
						"Recast " + "shields".Emphasize() + " allied skeletons"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_WardOfTheDamned_BonusDamageOnRecast,
						"Increase recast " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_IncreaseMoveSpeedDuringChannel_Low,
						"Increase " + "MS".Emphasize() + " during channel"),
				}
			},
			{
				"veilofbones", new List<KeyValuePair<PrefabGUID, string>>
				{
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfBones_DashInflictCondemn,
						"Dashing through an enemy applies " + "Condemn".Colorify(ExtendedColor.Unholy)),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfBones_DashHealMinions,
						"Dashing through " + "skeletons".Emphasize() + " resets & heal them"),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Unholy_ApplyAgony_OnAttack,
						"Next auto on a " + "Condemned".Colorify(ExtendedColor.Unholy) + " enemy applies " + "Agony".Colorify(ExtendedColor.Unholy)),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Unholy_ApplyBane_OnAttack,
						"Next auto on a " + "Condemned".Colorify(ExtendedColor.Unholy) + " enemy applies " + "Bane".Colorify(ExtendedColor.Unholy)),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Veil_BonusDamageOnPrimary,
						"Next auto deals " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfBones_BonusDamageBelowTreshhold,
						"Next auto to low " + "HP".Emphasize() + " enemy deals more " + "damage".Emphasize()),
					new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Veil_BuffAndIllusionDuration,
						"Increase " + "elude duration".Emphasize()),
				}
			},
		};
}
