using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectM;
using Unity.Entities;

namespace PvpArena.Models;
public class AbilityBar
{
	public PrefabGUID Unknown { get; set; } = PrefabGUID.Empty;
	public PrefabGUID Auto { get; set; } = PrefabGUID.Empty;
	public PrefabGUID Weapon1 { get; set; } = PrefabGUID.Empty;
	public PrefabGUID Weapon2 { get; set; } = PrefabGUID.Empty;
	public PrefabGUID Dash { get; set; } = PrefabGUID.Empty;
	public PrefabGUID Spell1 { get; set; } = PrefabGUID.Empty;
	public PrefabGUID Spell2 { get; set; } = PrefabGUID.Empty;
	public PrefabGUID Ult { get; set; } = PrefabGUID.Empty;

	public void SetAbility(PrefabGUID ability, string slot)
	{
		switch (slot.ToLower())
		{
			case "auto": Auto = ability; break;
			case "dash": Dash = ability; break;
			case "weapon1": Weapon1 = ability; break;
			case "weapon2": Weapon2 = ability; break;
			case "spell1": Spell1 = ability; break;
			case "spell2": Spell2 = ability; break;
			case "ult": Ult = ability; break;
			default: throw new ArgumentException("Invalid slot");
		}
	}
	public void ApplyChangesHard(Entity buffEntity)
	{
		ApplyChanges(buffEntity, true);
	}

	public void ApplyChangesSoft(Entity buffEntity)
	{
		ApplyChanges(buffEntity, false);
	}

	private void ApplyChanges(Entity buffEntity, bool isHard)
	{
		var buffer = buffEntity.AddBuffer<ReplaceAbilityOnSlotBuff>();
		var abilities = new List<PrefabGUID> { Auto, Weapon1, Dash, Unknown, Weapon2, Spell1, Spell2, Ult };

		for (int i = 0; i < abilities.Count; i++)
		{
			var priority = isHard || abilities[i] != PrefabGUID.Empty ? 101 : 0;
			buffer.Add(new ReplaceAbilityOnSlotBuff
			{
				Slot = i,
				CastBlockType = GroupSlotModificationCastBlockType.WholeCast,
				NewGroupId = abilities[i],
				ReplaceGroupId = abilities[i],
				Priority = priority,
				Target = ReplaceAbilityTarget.BuffOwner
			});
		}
		buffEntity.Add<ReplaceAbilityOnSlotData>();
	}
}
