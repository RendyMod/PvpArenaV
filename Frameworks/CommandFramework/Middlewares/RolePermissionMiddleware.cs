using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using PvpArena.Models;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;

public class RolePermissionMiddleware : IMiddleware
{
	public bool CanExecute(Player sender, CommandAttribute command, MethodInfo method)
	{
		if (sender.IsAdmin || !command.AdminOnly) return true;
		if (RoleRepository.CanUserExecuteCommand(sender.Name, command.Name))
		{
			return true;
		}
		else
		{
			sender.ReceiveMessage("Denied".Error());
			return false;
		}
	}
}

public static class RoleRepository
{
	public static void AddRole(string role)
	{
		FileRoleStorage.AddRole(role);
	}

	// this extends the IRoleStorage with ~CRUD operations
	public static void AddUserToRole(string user, string role)
	{
		var roles = FileRoleStorage.GetUserRoles(user) ?? new();
		Unity.Debug.Log(roles.ToString());
		roles.Add(role);
		Unity.Debug.Log(roles.ToString());
		FileRoleStorage.SetUserRoles(user, roles);
	}

	public static void RemoveUserFromRole(string user, string role)
	{
		var roles = FileRoleStorage.GetUserRoles(user) ?? new();
		roles.Remove(role);
		FileRoleStorage.SetUserRoles(user, roles);
	}

	public static void AddRoleToCommand(string command, string role)
	{
		var roles = FileRoleStorage.GetCommandPermission(command) ?? new();
		roles.Add(role);
		FileRoleStorage.SetCommandPermission(command, roles);
	}

	public static void RemoveRoleFromCommand(string command, string role)
	{
		var roles = FileRoleStorage.GetCommandPermission(command) ?? new();
		roles.Remove(role);
		FileRoleStorage.SetCommandPermission(command, roles);
	}

	public static HashSet<string> ListUserRoles(string user) => FileRoleStorage.GetUserRoles(user);

	public static HashSet<string> ListCommandRoles(string command) => FileRoleStorage.GetCommandPermission(command);

	public static HashSet<string> Roles => FileRoleStorage.GetRoles();

	public static bool CanUserExecuteCommand(string user, string command)
	{
		var roles = FileRoleStorage.GetCommandPermission(command);
		if (roles == null) return false;
		var userRoles = FileRoleStorage.GetUserRoles(user);
		if (userRoles == null) return false;
		return roles.Any(userRoles.Contains);
	}
}

public static class FileRoleStorage
{
	private static readonly string _filePath = "BepInEx/config/PvpArena/roles.json";
	private static Dictionary<string, HashSet<string>> _userRoles = new Dictionary<string, HashSet<string>>();
	private static Dictionary<string, HashSet<string>> _commandPermissions = new Dictionary<string, HashSet<string>>();
	private static HashSet<string> _roles = new HashSet<string>();
	public static HashSet<string> Roles => _roles;

	public static void AddRole(string role)
	{
		_roles.Add(role);
		SaveData();
	}

	public static void SaveData()
	{
		var storedData = new StoredData
		{
			UserRoles = _userRoles,
			CommandPermissions = _commandPermissions,
			Roles = _roles
		};
		var options = new JsonSerializerOptions
		{
			WriteIndented = true,
		};
		string jsonData = JsonSerializer.Serialize(storedData, options);

		string directoryPath = Path.GetDirectoryName(_filePath);
		Directory.CreateDirectory(directoryPath);

		try
		{
			File.WriteAllText(_filePath, jsonData);
		}
		catch (Exception e)
		{
			Unity.Debug.Log(e.ToString());
		}
		
	}

	private static void LoadData()
	{
		if (File.Exists(_filePath))
		{
			string jsonData = File.ReadAllText(_filePath);
			var storedData = JsonSerializer.Deserialize<StoredData>(jsonData);
			_userRoles = storedData.UserRoles ?? new Dictionary<string, HashSet<string>>();
			_commandPermissions = storedData.CommandPermissions ?? new Dictionary<string, HashSet<string>>();
			_roles = storedData.Roles ?? new HashSet<string>();
		}
		else
		{
			_userRoles = new Dictionary<string, HashSet<string>>();
			_commandPermissions = new Dictionary<string, HashSet<string>>();
			_roles = new HashSet<string>();
		}
	}

	public static void SetCommandPermission(string command, HashSet<string> roleIds)
	{
		_commandPermissions[command] = roleIds;
		SaveData();
	}

	public static void SetUserRoles(string userId, HashSet<string> roleIds)
	{
		_userRoles[userId] = roleIds;
		SaveData();
	}

	public static HashSet<string> GetCommandPermission(string command)
	{
		LoadData();
		return _commandPermissions.GetValueOrDefault(command, new HashSet<string>());
	}

	public static HashSet<string> GetUserRoles(string userId)
	{
		LoadData();
		return _userRoles.GetValueOrDefault(userId, new HashSet<string>());
	}

	public static HashSet<string> GetRoles()
	{
		LoadData();
		return _roles;
	}

	public class StoredData
	{
		public Dictionary<string, HashSet<string>> UserRoles { get; set; }
		public Dictionary<string, HashSet<string>> CommandPermissions { get; set; }
		public HashSet<string> Roles { get; set; }
	}
}


// Other comopnents of the middleware
public class RoleCommands
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
	[Command("create-role", adminOnly:true)]
	public void CreateRole(Player sender, string name)
	{
		RoleRepository.AddRole(name);
		sender.ReceiveMessage($"Added {name.Emphasize()} role".Success());
	}

	[Command("grant-command-role-access", adminOnly:true)]
	public void AllowCommand(Player sender, string role, string command)
	{
		RoleRepository.AddRoleToCommand(command, role);
		sender.ReceiveMessage($"{role.Emphasize()} role can now use {command.Emphasize()} command".Success());
	}

	[Command("revoke-command-role-access", adminOnly:true)]
	public void DenyCommand(Player sender, string role, string command)
	{
		RoleRepository.RemoveRoleFromCommand(command, role);
		sender.ReceiveMessage($"{role.Emphasize()} role can no longer use {command.Emphasize()} command".Success());
	}

	[Command("assign-role", adminOnly:true)]
	public void AssignUserToRole(Player sender, string user, string role)
	{
		RoleRepository.AddUserToRole(user, role);
		sender.ReceiveMessage($"Assigned {role.Emphasize()} role to {user.Emphasize()}".Success());
	}

	[Command("unassign-role", adminOnly:true)]
	public void UnassignUserFromRole(Player sender, string user, string role)
	{
		RoleRepository.RemoveUserFromRole(user, role);
		sender.ReceiveMessage($"Unassigned {role.Emphasize()} role to {user.Emphasize()}".Success());
	}

	[Command("list-all-roles", adminOnly:true)]
	public void ListRoles(Player sender)
	{
		sender.ReceiveMessage(("Roles: " + string.Join(", ", RoleRepository.Roles).Emphasize()).Success());
	}

	[Command("list-user-roles", adminOnly:true)]
	public void ListRoles(Player sender, string user)
	{
		sender.ReceiveMessage($"Roles: {string.Join(", ", RoleRepository.ListUserRoles(user)).Emphasize()}".Success());
	}

	[Command("list-command-roles", adminOnly:true)]
	public void ListCommands(Player sender, string command)
	{
		sender.ReceiveMessage($"Roles: {string.Join(", ", RoleRepository.ListCommandRoles(command)).Emphasize()}".Success());
	}
}
