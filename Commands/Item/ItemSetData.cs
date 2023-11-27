using System.Collections.Generic;
using ProjectM;
using PvpArena.Data;

public class ItemSet
{
	public PrefabGUID equipmentSet = new PrefabGUID(0);
	public PrefabGUID chestPrefab = new PrefabGUID(0);
	public PrefabGUID glovesPrefab = new PrefabGUID(0);
	public PrefabGUID legsPrefab = new PrefabGUID(0);
	public PrefabGUID bootsPrefab = new PrefabGUID(0);

	public ItemSet ()
	{
	}

	public static ItemSet DarkMatterSet = new ItemSet()
	{
		equipmentSet = new PrefabGUID(0),
		chestPrefab = Prefabs.Item_Chest_T09_DarkMatter,
		glovesPrefab = Prefabs.Item_Gloves_T09_DarkMatter,
		legsPrefab = Prefabs.Item_Legs_T09_DarkMatter,
		bootsPrefab = Prefabs.Item_Boots_T09_DarkMatter,
	};

	public static ItemSet NoctumSet = new ItemSet()
	{
		equipmentSet = Prefabs.SetBonus_T08_Shadowmoon,
		chestPrefab = Prefabs.Item_Chest_T08_Noctum,
		glovesPrefab = Prefabs.Item_Gloves_T08_Noctum,
		legsPrefab = Prefabs.Item_Legs_T08_Noctum,
		bootsPrefab = Prefabs.Item_Boots_T08_Noctum,
	};

	public static ItemSet BloodmoonSet = new ItemSet()
	{
		equipmentSet = Prefabs.SetBonus_T08_Shadowmoon,
		chestPrefab = Prefabs.Item_Chest_T08_Shadowmoon,
		glovesPrefab = Prefabs.Item_Gloves_T08_Shadowmoon,
		legsPrefab = Prefabs.Item_Legs_T08_Shadowmoon,
		bootsPrefab = Prefabs.Item_Boots_T08_Shadowmoon,
	};

	public static ItemSet SilkSet = new ItemSet()
	{
		equipmentSet = Prefabs.SetBonus_T07_Dawnthorn,
		chestPrefab = Prefabs.Item_Chest_T07_Silk,
		glovesPrefab = Prefabs.Item_Gloves_T07_Silk,
		legsPrefab = Prefabs.Item_Legs_T07_Silk,
		bootsPrefab = Prefabs.Item_Boots_T07_Silk,
	};

	public static ItemSet IronSet = new ItemSet()
	{
		equipmentSet = Prefabs.SetBonus_T06_MercilessHollowFang,
		chestPrefab = Prefabs.Item_Chest_T06_Iron,
		glovesPrefab = Prefabs.Item_Gloves_T06_Iron,
		legsPrefab = Prefabs.Item_Legs_T06_Iron,
		bootsPrefab = Prefabs.Item_Boots_T06_Iron,
	};

	public static ItemSet CottonSet = new ItemSet()
	{
		equipmentSet = new PrefabGUID(0),
		chestPrefab = Prefabs.Item_Chest_T05_Cotton,
		glovesPrefab = Prefabs.Item_Gloves_T05_Cotton,
		legsPrefab = Prefabs.Item_Legs_T05_Cotton,
		bootsPrefab = Prefabs.Item_Boots_T05_Cotton,
	};

	public static ItemSet CopperSet = new ItemSet()
	{
		equipmentSet = Prefabs.SetBonus_T04_MercilessNightStalker,
		chestPrefab = Prefabs.Item_Chest_T04_Copper,
		glovesPrefab = Prefabs.Item_Gloves_T04_Copper,
		legsPrefab = Prefabs.Item_Legs_T04_Copper,
		bootsPrefab = Prefabs.Item_Boots_T04_Copper,
	};

	public static ItemSet ClothSet = new ItemSet()
	{
		equipmentSet = new PrefabGUID(0),
		chestPrefab = Prefabs.Item_Chest_T03_Cloth,
		glovesPrefab = Prefabs.Item_Gloves_T03_Cloth,
		legsPrefab = Prefabs.Item_Legs_T03_Cloth,
		bootsPrefab = Prefabs.Item_Boots_T03_Cloth,
	};

	public static ItemSet BoneReinforced = new ItemSet()
	{
		equipmentSet = new PrefabGUID(0),
		chestPrefab = Prefabs.Item_Chest_T02_BoneReinforced,
		glovesPrefab = Prefabs.Item_Gloves_T02_BoneReinforced,
		legsPrefab = Prefabs.Item_Legs_T02_BoneReinforced,
		bootsPrefab = Prefabs.Item_Boots_T02_BoneReinforced,
	};

	public static ItemSet Bone = new ItemSet()
	{
		equipmentSet = new PrefabGUID(0),
		chestPrefab = Prefabs.Item_Chest_T01_Bone,
		glovesPrefab = Prefabs.Item_Gloves_T01_Bone,
		legsPrefab = Prefabs.Item_Legs_T01_Bone,
		bootsPrefab = Prefabs.Item_Boots_T01_Bone,
	};
	
	public static ItemSet Naked = new ItemSet()
	{
		equipmentSet = new PrefabGUID(0),
		chestPrefab = Prefabs.Item_Chest_T00_StartingRags,
		glovesPrefab = Prefabs.Item_Gloves_T00_StartingRags,
		legsPrefab = Prefabs.Item_Legs_T00_StartingRags,
		bootsPrefab = Prefabs.Item_Boots_T00_StartingRags,
	};

	public static Dictionary<string, ItemSet> ItemSetDictionary = new Dictionary<string, ItemSet>
	{
		{ "naked", Naked },
		{ "bone", Bone },
		{ "boneReinforced", BoneReinforced },
		{ "cloth", ClothSet },
		{ "copper", CopperSet },
		{ "cotton", CottonSet },
		{ "iron", IronSet },
		{ "silk", SilkSet },
		{ "bloodmoon", BloodmoonSet },
		{ "noctum", NoctumSet },
		{ "darkmatter", DarkMatterSet },
	};
}
