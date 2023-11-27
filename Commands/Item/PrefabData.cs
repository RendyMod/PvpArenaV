using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace PvpArena.Models;


public class PrefabData
{
	public PrefabGUID PrefabGUID { get; }
	public string FormalPrefabName { get; }
	public string OverrideName { get; private set; }

	public PrefabData(PrefabGUID prefabGUID, string formalPrefabName, string overrideName = "")
	{
		PrefabGUID = prefabGUID;
		FormalPrefabName = formalPrefabName;
		OverrideName = overrideName;
	}

	public string GetName()
	{
		return string.IsNullOrEmpty(OverrideName) ? FormalPrefabName : OverrideName;
	}
}

public class ItemPrefabData : PrefabData
{
	public ItemPrefabData(PrefabGUID prefabGUID, string formalPrefabName, string overrideName = "") : base(prefabGUID, formalPrefabName, overrideName)
	{
	}
}

public class BloodPrefabData : PrefabData
{
	public BloodPrefabData(PrefabGUID prefabGUID, string formalPrefabName, string overrideName = "") : base(prefabGUID, formalPrefabName, overrideName)
	{
	}
}

public class JewelPrefabData : PrefabData
{
	public JewelPrefabData(PrefabGUID prefabGUID, string formalPrefabName, string overrideName = "") : base(prefabGUID, formalPrefabName, overrideName)
	{
	}
}
