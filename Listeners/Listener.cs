using HarmonyLib;
using Unity.Entities;
using System.Collections.Generic;
using Unity.Collections;
using ProjectM;

namespace PvpArena.Listeners;

[HarmonyPatch(typeof(ServerTimeSystem_Server), nameof(ServerTimeSystem_Server.OnUpdate))]
public static class Listener
{
	private static Dictionary<EntityQuery, EntityQueryListener> _listeners = new Dictionary<EntityQuery, EntityQueryListener>();
	private static Dictionary<Entity, int> _entityVersion = new Dictionary<Entity, int>();

	public static void AddListener(EntityQuery query, EntityQueryListener listener)
	{
		_listeners[query] = listener;
	}

	public static void Dispose()
	{
		foreach (var listener in _listeners)
		{
			listener.Key.Dispose();
		}
		_listeners.Clear();
	}

	public static void Prefix()
	{
		foreach (var kvp in _listeners)
		{
			var query = kvp.Key;
			var listener = kvp.Value;

			var entities = query.ToEntityArray(Allocator.Temp);
			foreach (var entity in entities)
			{
				int currentVersion = entity.Version;
				if (!_entityVersion.TryGetValue(entity, out int lastVersion) || lastVersion != currentVersion)
				{
					if (lastVersion == 0)
					{
						listener.OnNewMatchFound(entity);
					}
					else
					{
						listener.OnNewMatchRemoved(entity);
					}
					_entityVersion[entity] = currentVersion;
				}
			}

			entities.Dispose();
		}
	}
}


// Interface for listeners
public interface EntityQueryListener
{
	public void OnNewMatchFound(Entity entity);
	public void OnNewMatchRemoved(Entity entity);
}
