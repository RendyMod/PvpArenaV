using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectM;
using PvpArena.Factories;

namespace PvpArena.Data;
public static class BuffModifiers
{
	public static ModifyUnitStatBuff_DOTS PancakeShapeshiftSpeed = new ModifyUnitStatBuff_DOTS
	{
		ModificationType = ModificationType.Set,
		StatType = UnitStatType.MovementSpeed,
		Id = ModificationIdFactory.NewId(),
		Value = 7
	};

	public static ModifyUnitStatBuff_DOTS FastRespawnMoveSpeed = new ModifyUnitStatBuff_DOTS
	{
		ModificationType = ModificationType.Set,
		StatType = UnitStatType.MovementSpeed,
		Id = ModificationIdFactory.NewId(),
		Value = 8
	};

	public static ModifyUnitStatBuff_DOTS ShapeshiftFastMoveSpeed = new ModifyUnitStatBuff_DOTS
	{
		ModificationType = ModificationType.Set,
		StatType = UnitStatType.MovementSpeed,
		Id = ModificationIdFactory.NewId(),
		Value = 12
	};

	public static ModifyUnitStatBuff_DOTS PancakeSlowRelicSpeed = new ModifyUnitStatBuff_DOTS
	{
		ModificationType = ModificationType.Set,
		StatType = UnitStatType.MovementSpeed,
		Id = ModificationIdFactory.NewId(),
		Value = CaptureThePancakeConfig.Config.ShardSpeed
	};

	public static ModifyUnitStatBuff_DOTS OnDeathFastMoveSpeed = new ModifyUnitStatBuff_DOTS
	{
		ModificationType = ModificationType.Set,
		StatType = UnitStatType.MovementSpeed,
		Id = ModificationIdFactory.NewId(),
		Value = 15
	};

	public static ModifyUnitStatBuff_DOTS SilverResistance = new ModifyUnitStatBuff_DOTS
	{
		ModificationType = ModificationType.Set,
		StatType = UnitStatType.SilverResistance,
		Id = ModificationIdFactory.NewId(),
		Value = 1000
	};

	public static ModifyUnitStatBuff_DOTS CloakHealth = new ModifyUnitStatBuff_DOTS
	{
		ModificationType = ModificationType.Add,
		StatType = UnitStatType.MaxHealth,
		Id = ModificationIdFactory.NewId(),
		Value = 24
	};

	public static BuffModificationTypes PigModifications = BuffModificationTypes.Invulnerable | BuffModificationTypes.Immaterial | BuffModificationTypes.DisableDynamicCollision | BuffModificationTypes.AbilityCastImpair;

	public static BuffModificationFlagData DefaultShapeshiftModifications = new BuffModificationFlagData()
	{
		ModificationTypes = (long)(BuffModificationTypes.Invulnerable | BuffModificationTypes.Immaterial | BuffModificationTypes.DisableDynamicCollision),
		ModificationId = ModificationIdFactory.NewId(),
	};
}
