using PvpArena.Data;
using Unity.Entities;
using ProjectM.Network;
using PvpArena.Models;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;
using PvpArena.Helpers;

public static class KitCommands
{
	[Command("kit", description: "Gives a kit of starting items", usage: ".kit", adminOnly: false, includeInHelp:true, category:"Kits")]
	public static void GearKitCommand(Player sender)
	{
		Helper.GiveStartingGear(sender);
		
		sender.ReceiveMessage("Gave:".Success()+" Legendaries / Armor / Necks".White());
	}
	
	[Command("kit legendary", description: "Gives default legendaries", usage: ".kit legendary", aliases: new string[] { "kit legendaries" }, adminOnly: false, includeInHelp:false, category:"Kits")]
	public static void LegendaryKitCommand (Player sender)
	{
		Helper.GiveDefaultLegendaries(sender);
		
		sender.ReceiveMessage("Gave:".Success()+" Legendaries".White());
	}

	[Command("kit jewels", description: "Gives default jewels", usage: ".kit jewels", aliases: new string[] { "kit jewels" }, adminOnly: false, includeInHelp: false, category: "Kits")]
	public static void JewelKitCommand(Player sender)
	{
		Helper.GiveDefaultJewels(sender);

		sender.ReceiveMessage("Gave:".Success() + " Jewels".White());
	}

	[Command("kit gear", description: "Gives a kit of gear and necklaces", usage:".kit gear", adminOnly: false, includeInHelp:true, category:"Kits")]
	public static void GearKitGearCommand (Player sender)
	{
		Helper.GiveArmorAndNecks(sender);	
		sender.ReceiveMessage("Gave:".Success()+" Armor / Necks".White());
	}
	
	[Command("kit bags", description: "Gives a kit of bags", usage:".kit bags", adminOnly: false, includeInHelp:true, category:"Kits")]
	public static void GearKitBagCommand (Player sender)
	{
		Helper.GiveBags(sender);	
		sender.ReceiveMessage("Gave and equipped:".Success()+" Bags".White());
	}
	

	[Command("kit sanguine", description: "Gives sanguine weapons", usage: ".kit sanguine", adminOnly: false, includeInHelp:true, category:"Kits")]
	public static void SanguineKitCommand(Player sender)
	{
		foreach (var weapon in Kit.SanguineWeapons)
		{
			Helper.AddItemToInventory(sender.Character, weapon, 1, out Entity itemEntity);
		}
		
		sender.ReceiveMessage("Gave:".Success()+" Sanguine".White());
	}

	[Command("heals", description: "Gives health pots", usage: ".heals", aliases: new string[] {"heal"}, adminOnly: false, includeInHelp: false, category: "Healing Potions")]
	public static void HealsKitCommand(Player sender)
	{
		Helper.AddItemToInventory(sender.Character, Prefabs.Item_Consumable_Canteen_BloodRoseBrew_T01, 1, out Entity itemEntity);
		Helper.AddItemToInventory(sender.Character, Prefabs.Item_Consumable_GlassBottle_BloodRosePotion_T02, 1, out itemEntity);
		
		sender.ReceiveMessage("Gave:".Success()+ " Healing potions".White());
	}
	
	[Command("od", description: "Gives trippy shroom", usage: ".od", aliases: new string[] {"od"}, adminOnly: false, includeInHelp: false, category: "Trippy Shroom")]
	public static void OverdoseShroomCommand(Player sender)
	{
		Helper.AddItemToInventory(sender.Character, Prefabs.Item_Consumable_TrippyShroom, 1, out Entity itemEntity);
		
		sender.ReceiveMessage("Gave:".Success()+ " Trippy Shroom".White());
	}
}
