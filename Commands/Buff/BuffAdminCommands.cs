using System;
using ProjectM;
using ProjectM.Gameplay.Scripting;
using PvpArena.Data;
using PvpArena.Helpers;
using PvpArena.Models;
using PvpArena.Services;
using Unity.Entities;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;

namespace PvpArena.Commands;

partial class BuffCommands
{
	[Command("add-buff", description: "Buff a player with a prefab name or guid", usage: ".add-buff buffGuid, player, duration, persistsThroughDeath, effectsOnStart", aliases: new string[] { "buff" }, adminOnly: true)]
	public void BuffCommand (Player sender, PrefabGUID buffGuid, Player player = null, float duration = Helper.NO_DURATION, bool persistsThroughDeath = false)
	{
		var Player = player != null ? player : sender;

		try
		{
			Helper.BuffPlayer(Player, buffGuid, out var buffEntity, duration, persistsThroughDeath);
			sender.ReceiveMessage("Added buff.".Success());
		}
		catch (Exception e)
		{
			sender.ReceiveMessage(e.ToString().Error());
			return;
		}
	}

	[Command("remove-buff", description: "Removes a buff", aliases: new string[] { "unbuff", "debuff" }, adminOnly: true)]
	public void UnbuffCommand (Player sender, PrefabGUID buffGuid, Player player = null)
	{
		var Player = player != null ? player : sender;
		Helper.RemoveBuff(Player.Character, buffGuid);
		sender.ReceiveMessage("Removed buff.".Success());
	}

	[Command("list-buffs", description: "Lists the buffs a player has", adminOnly: true)]
	public void ListBuffsCommand (Player sender, Player player = null)
	{
		var Player = player != null ? player : sender;

		var buffs = Helper.GetEntityBuffs(Player.Character);
		foreach (var buff in buffs)
		{
			sender.ReceiveMessage(buff.LookupName().White());
		}

		sender.ReceiveMessage($"Done");
	}

	[Command("buff-target", description: "Used for debugging", adminOnly: true)]
	public static void BuffHoveredTargetCommand(Player sender, PrefabGUID buffGuid, float duration = Helper.DEFAULT_DURATION, bool persistsThroughDeath = false)
	{
		Entity entity = Helper.GetHoveredEntity(sender.User);
		Helper.BuffEntity(entity, buffGuid, out var buffEntity, Helper.NO_DURATION, persistsThroughDeath);
		Helper.ModifyBuff(buffEntity, BuffModificationTypes.None);
		buffEntity.Add<DestroyBuffOnDamageTaken>();
		
		sender.ReceiveMessage("Done");
	}

	[Command("clear-target-buffs", description: "Used for debugging", adminOnly: true)]
	public static void ClearHoveredTargetBuffsCommand(Player sender)
	{
		Entity entity = Helper.GetHoveredEntity(sender.User);
		Helper.ClearExtraBuffs(sender.Character, new Helper.ResetOptions
		{
			RemoveConsumables = true,
			RemoveShapeshifts = true
		});

		sender.ReceiveMessage($"Done: {entity.Read<PrefabGUID>().LookupNameString()}");
	}

    [Command("remove-target-buff", description: "Removes a buff", adminOnly: true)]
    public void UnbuffTargetCommand(Player sender, PrefabGUID buffGuid)
    {
        var entity = Helper.GetHoveredEntity(sender.User);
        Helper.RemoveBuff(entity, buffGuid);
        sender.ReceiveMessage("Removed buff.".Success());
    }

    [Command("list-target-buffs", description: "Lists the buffs a hovered character has", adminOnly: true)]
	public void ListTargetBuffsCommand(Player sender)
	{
		var target = Helper.GetHoveredEntity(sender.User);
		var buffs = Helper.GetEntityBuffs(target);
		foreach (var buff in buffs)
		{
			sender.ReceiveMessage(buff.LookupName().White());
		}

		sender.ReceiveMessage($"Done: {target.LookupName()}");
	}

	[Command("clear-buffs", description: "Removes any extra buffs on a player", adminOnly: true)]
	public void ClearBuffsCommand (Player sender, Player player = null)
	{
		var Player = player != null ? player : sender;
		Helper.ClearExtraBuffs(Player.Character, new Helper.ResetOptions
		{
			RemoveConsumables = true,
			RemoveShapeshifts = true
		});
		sender.ReceiveMessage("Extra buffs cleared.".Success());
	}
}
