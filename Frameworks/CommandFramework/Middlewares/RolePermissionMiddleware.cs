using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using PvpArena;
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
        if (roles == null)
        {
            return false;
        }
		var userRoles = FileRoleStorage.GetUserRoles(user);
        if (userRoles == null) 
        {
            return false;
        }
        return roles.Any(userRoles.Contains);
	}
}

public static class FileRoleStorage
{
	public static void Initialize()
	{
		LoadData();
	}
	public static void Dispose()
	{
		_userRoles.Clear();
		_commandPermissions.Clear();
		_roles.Clear();
	}
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
		
		return _commandPermissions.GetValueOrDefault(command, new HashSet<string>());
	}

	public static HashSet<string> GetUserRoles(string userId)
	{
		return _userRoles.GetValueOrDefault(userId, new HashSet<string>());
	}

	public static HashSet<string> GetRoles()
	{
		return _roles;
	}

	public class StoredData
	{
		public Dictionary<string, HashSet<string>> UserRoles { get; set; }
		public Dictionary<string, HashSet<string>> CommandPermissions { get; set; }
		public HashSet<string> Roles { get; set; }
	}
}
