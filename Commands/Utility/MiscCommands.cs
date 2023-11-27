using PvpArena.Data;
using Unity.Entities;
using ProjectM;
using Bloodstone.API;
using ProjectM.Network;
using PvpArena.Models;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;
using PvpArena.Configs;
using PvpArena.Helpers;

namespace PvpArena.Commands;

internal static class MiscellaneousCommands
{
	[Command("discord", description: "Get the link of the discord", usage:".discord", aliases: new string[] { "disc" }, adminOnly: false, includeInHelp: true, category: "Misc")]
	public static void DiscordCommand(Player sender)
	{
		sender.ReceiveMessage($"{PvpArenaConfig.Config.DiscordLink}".Emphasize());
	}

	[Command("spawn-horse", description: "Used for debugging", adminOnly: true)]
	public static void SpawnHorseCommand(Player sender, float speed = 4f, float acceleration = 7.0f, float rotation = 14.0f)
	{
		rotation *= 10;

		PrefabSpawnerService.SpawnWithCallback(Prefabs.CHAR_Mount_Horse, sender.Position, (System.Action<Entity>)((Entity e) =>
		{
			var mountable = e.Read<Mountable>();
			mountable.MaxSpeed = speed;
			mountable.Acceleration = acceleration;
			mountable.RotationSpeed = rotation;
			e.Write(mountable);
			if (Helper.BuffEntity(e, Prefabs.Buff_Manticore_ImmaterialHomePos, out var buffEntity, (float)Helper.NO_DURATION, true))
			{
				buffEntity.Add<BuffModificationFlagData>();
				buffEntity.Write(new BuffModificationFlagData
				{
					ModificationTypes = (long)(BuffModificationTypes.TargetSpellImpaired | BuffModificationTypes.Immaterial | BuffModificationTypes.DisableMapCollision | BuffModificationTypes.DisableDynamicCollision)
				});
			}
			sender.ReceiveMessage("Spawned horse!".Success());
		}));
	}

	[Command("control", description: "Takes control over hovered NPC (Unstable, work-in-progress)", adminOnly: true)]
	public static void ControlCommand(Player sender)
	{
		ControlDebugEvent controlDebugEvent;
		DebugEventsSystem des = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
		var entityInput = sender.Character.Read<EntityInput>();
		if (entityInput.HoveredEntity.Index > 0)
		{
			Entity newCharacter = entityInput.HoveredEntity;
			if (!newCharacter.Has<PlayerCharacter>())
			{
				controlDebugEvent = new ControlDebugEvent
				{
					EntityTarget = newCharacter,
					Target = entityInput.HoveredEntityNetworkId
				};
				des.ControlUnit(sender.ToFromCharacter(), controlDebugEvent);
				sender.ReceiveMessage($"Controlling hovered unit");
				return;
			}
		}
		controlDebugEvent = new ControlDebugEvent
		{
			EntityTarget = sender.Character,
			Target = sender.Character.Read<NetworkId>()
		};
		des.ControlUnit(sender.ToFromCharacter(), controlDebugEvent);
		sender.ReceiveMessage("Controlling self");
	}
}