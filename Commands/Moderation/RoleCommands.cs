// Other comopnents of the middleware

using PvpArena.Frameworks.CommandFramework;
using PvpArena.Models;

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

	[CommandFramework.Command("grant-command-role-access", adminOnly:true)]
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
	public static void AssignUserToRole(Player sender, string user, string role)
	{
		RoleRepository.AddUserToRole(user, role);
		sender.ReceiveMessage($"Assigned {role.Emphasize()} role to {user.Emphasize()}".Success());
	}

	[CommandFramework.Command("unassign-role", adminOnly:true)]
	public static void UnassignUserFromRole(Player sender, string user, string role)
	{
		RoleRepository.RemoveUserFromRole(user, role);
		sender.ReceiveMessage($"Unassigned {role.Emphasize()} role to {user.Emphasize()}".Success());
	}

	[CommandFramework.Command("list-all-roles", adminOnly:true)]
	public static void ListRoles(Player sender)
	{
		sender.ReceiveMessage(("Roles: " + string.Join(", ", RoleRepository.Roles).Emphasize()).Success());
	}

	[CommandFramework.Command("list-user-roles", adminOnly:true)]
	public static void ListRoles(Player sender, string user)
	{
		sender.ReceiveMessage($"Roles: {string.Join(", ", RoleRepository.ListUserRoles(user)).Emphasize()}".Success());
	}

	[CommandFramework.Command("list-command-roles", adminOnly:true)]
	public static void ListCommands(Player sender, string command)
	{
		sender.ReceiveMessage($"Roles: {string.Join(", ", RoleRepository.ListCommandRoles(command)).Emphasize()}".Success());
	}
}
