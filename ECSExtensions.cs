using System;
using System.Runtime.InteropServices;
using Bloodstone.API;
using Il2CppInterop.Runtime;
using ProjectM;
using Unity.Entities;

namespace PvpArena;

#pragma warning disable CS8500
public static class ECSExtensions
{
	public unsafe static void Modify<T>(this Entity entity, Action<T> modifyAction) where T : struct
	{
		// Read the existing component data
		T componentData = Read<T>(entity);

		// Apply the modification
		modifyAction(componentData);

		// Write the modified component back
		Write(entity, componentData);
	}

	public unsafe static void Write<T>(this Entity entity, T componentData) where T : struct
	{
		// Get the ComponentType for T
		var ct = new ComponentType(Il2CppType.Of<T>());

		// Marshal the component data to a byte array
		byte[] byteArray = StructureToByteArray(componentData);

		// Get the size of T
		int size = Marshal.SizeOf<T>();

		// Create a pointer to the byte array
		fixed (byte* p = byteArray)
		{
			// Set the component data
			VWorld.Server.EntityManager.SetComponentDataRaw(entity, ct.TypeIndex, p, size);
		}
	}

	// Helper function to marshal a struct to a byte array
	public static byte[] StructureToByteArray<T>(T structure) where T : struct
	{
		int size = Marshal.SizeOf(structure);
		byte[] byteArray = new byte[size];
		IntPtr ptr = Marshal.AllocHGlobal(size);

		Marshal.StructureToPtr(structure, ptr, true);
		Marshal.Copy(ptr, byteArray, 0, size);
		Marshal.FreeHGlobal(ptr);

		return byteArray;
	}

	public unsafe static T Read<T>(this Entity entity) where T : struct
	{
		// Get the ComponentType for T
		var ct = new ComponentType(Il2CppType.Of<T>());

		// Get a pointer to the raw component data
		void* rawPointer = VWorld.Server.EntityManager.GetComponentDataRawRO(entity, ct.TypeIndex);

		// Marshal the raw data to a T struct
		T componentData = Marshal.PtrToStructure<T>(new IntPtr(rawPointer));

		return componentData;
	}

	public static DynamicBuffer<T> ReadBuffer<T>(this Entity entity) where T : struct
	{
		return VWorld.Server.EntityManager.GetBuffer<T>(entity);
	}

	public static void Add<T>(this Entity entity)
	{
		var ct = new ComponentType(Il2CppType.Of<T>());
		VWorld.Server.EntityManager.AddComponent(entity, ct);
	}

	public static void Remove<T>(this Entity entity)
	{
		var ct = new ComponentType(Il2CppType.Of<T>());
		VWorld.Server.EntityManager.RemoveComponent(entity, ct);
	}

	public static bool Has<T>(this Entity entity)
	{
		var ct = new ComponentType(Il2CppType.Of<T>());
		return VWorld.Server.EntityManager.HasComponent(entity, ct);
	}

    public static void LogComponentTypes(this Entity entity)
    {
        var comps = VWorld.Server.EntityManager.GetComponentTypes(entity);
		if (entity.Has<PrefabGUID>())
		{
			entity.LogPrefabName();
		}
        foreach (var comp in comps)
        {
            Plugin.PluginLog.LogInfo($"{comp}");
        }
		Plugin.PluginLog.LogInfo("===");
    }

    public static void LogComponentTypes(this EntityQuery entityQuery)
    {
        var types = entityQuery.GetQueryTypes();
        foreach (var t in types)
        {
			Plugin.PluginLog.LogInfo($"Query Component Type: {t}");
        }
		Plugin.PluginLog.LogInfo($"===");
    }

	public static void LogPrefabName(this Entity entity)
	{
		Plugin.PluginLog.LogInfo(entity.Read<PrefabGUID>().LookupName());
	}

	public static string LookupName(this Entity entity)
	{
		return entity.Read<PrefabGUID>().LookupName();
	}
	public static string LookupNameString(this PrefabGUID prefabGuid)
	{
		var prefabCollectionSystem = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>();
		return prefabCollectionSystem.PrefabGuidToNameDictionary.ContainsKey(prefabGuid)
			? prefabCollectionSystem.PrefabGuidToNameDictionary[prefabGuid] : "GUID Not Found";
	}

	public static string LookupName(this PrefabGUID prefabGuid)
	{
		var prefabCollectionSystem = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>();
		return (prefabCollectionSystem.PrefabGuidToNameDictionary.ContainsKey(prefabGuid)
			? prefabCollectionSystem.PrefabGuidToNameDictionary[prefabGuid] + " " + prefabGuid : "GUID Not Found").ToString();
	}

	public static void LogPrefabName(this PrefabGUID prefabGuid)
	{
		Plugin.PluginLog.LogInfo(prefabGuid.LookupName());
	}

	public static void Destroy(this Entity entity)
	{
		VWorld.Server.EntityManager.DestroyEntity(entity);
	}
}
#pragma warning restore CS8500
