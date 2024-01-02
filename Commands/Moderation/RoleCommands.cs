// Other comopnents of the middleware

using Cpp2IL.Core.Api;
using PvpArena.Frameworks.CommandFramework;
using PvpArena.Models;
using PvpArena.Services;

public static class RoleCommands
{
	// Role Management commands
	// - Create a role
	// - Assign a command to a role
	// - Assign a user to a role
	// - Remove a role from a user
	// - Remove a role from a command
	// - List all roles
	// - List all commands for a role
	// - LIst all users for a role
	// - List roles for user
	[CommandFramework.Command("create-role", adminOnly:true)]
	public static void CreateRole(Player sender, string name)
	{
		RoleRepository.AddRole(name);
		sender.ReceiveMessage($"Added {name.Emphasize()} role".Success());
	}

	[CommandFramework.Command("grant-role-command-access", adminOnly:true)]
	public static void AllowCommand(Player sender, string role, string command)
	{
		RoleRepository.AddRoleToCommand(command, role);
		sender.ReceiveMessage($"{role.Emphasize()} role can now use {command.Emphasize()} command".Success());
	}

	[CommandFramework.Command("revoke-command-role-access", adminOnly:true)]
	public static void DenyCommand(Player sender, string role, string command)
	{
		RoleRepository.RemoveRoleFromCommand(command, role);
		sender.ReceiveMessage($"{role.Emphasize()} role can no longer use {command.Emphasize()} command".Success());
	}

	[CommandFramework.Command("assign-role", adminOnly:true)]
	public static void AssignPlayerToRole(Player sender, Player player, string role)
	{
		RoleRepository.AddPlayerToRole(player, role);
		sender.ReceiveMessage($"Assigned {role.Emphasize()} role to {player.Name.Emphasize()}".Success());
	}

	[CommandFramework.Command("unassign-role", adminOnly:true)]
	public static void UnassignUserFromRole(Player sender, Player player, string role)
	{
		RoleRepository.RemovePlayerFromRole(player, role);
		sender.ReceiveMessage($"Unassigned {role.Emphasize()} role to {player.Name.Emphasize()}".Success());
	}

	[CommandFramework.Command("list-all-roles", adminOnly:true)]
	public static void ListRoles(Player sender)
	{
		sender.ReceiveMessage(("Roles: " + string.Join(", ", RoleRepository.Roles).Emphasize()).Success());
	}

	[CommandFramework.Command("list-player-roles", adminOnly:true)]
	public static void ListRoles(Player sender, Player player)
	{
		sender.ReceiveMessage($"Roles: {string.Join(", ", RoleRepository.ListPlayerRoles(player)).Emphasize()}".Success());
	}

	[CommandFramework.Command("list-command-roles", adminOnly:true)]
	public static void ListCommands(Player sender, string command)
	{
		sender.ReceiveMessage($"Roles: {string.Join(", ", RoleRepository.ListCommandRoles(command)).Emphasize()}".Success());
	}
}
