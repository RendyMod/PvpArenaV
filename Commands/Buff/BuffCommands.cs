using Bloodstone.API;
using ProjectM;
using PvpArena.Data;
using PvpArena.Helpers;
using PvpArena.Models;
using PvpArena.Services;
using Unity.Entities;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;

namespace PvpArena.Commands;

partial class BuffCommands
{
	[Command("buffs", description: "Toggles rage and witch pot buffs", usage:".buffs (or use Blood Hunger)", aliases: new string[] { "bufs", "buf", "bu", "buff",}, adminOnly: false, includeInHelp:true, category:"Witch & Rage Buffs")]
	public void BuffsCommand (Player sender)
	{
		Helper.ToggleBuffsOnPlayer(sender);
	}
}
