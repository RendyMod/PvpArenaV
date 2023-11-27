
using ProjectM;
using ProjectM.CastleBuilding;
using PvpArena;
using PvpArena.Data;
using PvpArena.Helpers;
using PvpArena.Models;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;

public class LogCommands 
{
	[Command("log-nearby-entities", description: "Used for debugging", adminOnly: true)]
	public void LogNearbyEntitiesCommand(Player sender)
	{

		var entities = Helper.GetEntitiesByComponentTypes<PrefabGUID, LocalToWorld>();
		var myPos = sender.Position;

		foreach (var entity in entities)
		{
			var entityPos = entity.Read<LocalToWorld>().Position;
			var distance = math.distance(myPos, entityPos);
			if (distance < 2)
			{
				Plugin.PluginLog.LogInfo(entity.Read<PrefabGUID>().LookupName());

				if (entity.Read<PrefabGUID>() == Prefabs.AB_Storm_LightningWall_Object)
				{
					entity.LogComponentTypes();
				}

			}
		}
	}

	[Command("log-components", description: "Logs components of hovered entity", adminOnly: true)]
	public void LogComponentsCommand(Player sender)
	{
		var entity = Helper.GetHoveredEntity<CastleHeartConnection>(sender.Character);
		if (entity != Entity.Null)
		{
			entity.LogComponentTypes();
		}
	}


	[Command("log-position", description: "Logs components of hovered entity", adminOnly: true)]
	public void LogPositionCommand(Player sender)
	{
		var entity = Helper.GetHoveredEntity(sender.Character);
		if (entity != Entity.Null)
		{
			entity.LogPrefabName();
			var localToWorld = entity.Read<LocalToWorld>();
			var message = $"\"X\": {localToWorld.Position.x},\n\"Y\": {localToWorld.Position.y},\n\"Z\": {localToWorld.Position.z}";
			sender.ReceiveMessage(message);
			Plugin.PluginLog.LogInfo(message);
		}
	}

	[Command("log-structure-zone", description: "Logs dimensions of hovered entity", adminOnly: true)]
	public void LogSizeCommand(Player sender, int x, int y)
	{
		var entity = Helper.GetHoveredEntity(sender.Character);
		if (entity != Entity.Null && entity.Has<TileBounds>())
		{
			sender.ReceiveMessage($"Printed zone for: {entity.Read<PrefabGUID>().LookupNameString()}");
			Plugin.PluginLog.LogInfo(RectangleZone.FromEntity(entity, x, y).ToString());
		}
		else
		{
			sender.ReceiveMessage("Invalid entity");
		}
	}
}
