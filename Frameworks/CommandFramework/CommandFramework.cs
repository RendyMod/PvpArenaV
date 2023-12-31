using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ProjectM;
using PvpArena.GameModes;
using PvpArena.Models;
using Unity.DebugDisplay;
using UnityEngine;
using static Engine.Console.ConsoleCommand;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;

namespace PvpArena.Frameworks.CommandFramework;
public class CommandFramework
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	public class CommandAttribute : Attribute
	{
		public string Name { get; }
		public string[] Aliases { get; }
		public string Description { get; }
		public string Usage { get; }
		public string Category { get; }
		public bool AdminOnly { get; } = true;
		public bool IncludeInHelp { get; } = false;


		public CommandAttribute(string name, string description = "", string usage = "", string[] aliases = null, bool adminOnly = true, bool includeInHelp = false, string category = "")
		{
			Name = name;
			Description = description;
			Usage = usage;
			Aliases = aliases ?? Array.Empty<string>();
			AdminOnly = adminOnly;
			IncludeInHelp = includeInHelp;
			Category = category;
		}
	}

	public class CommandInfo
	{
		public MethodInfo CommandMethod { get; }
		public object CommandInstance { get; }
		public Type[] ParameterTypes { get; }
		public CommandAttribute CommandAttribute { get; }

		public CommandInfo(MethodInfo commandMethod, object commandInstance, CommandAttribute commandAttribute)
		{
			CommandMethod = commandMethod;
			CommandInstance = commandInstance;
			CommandAttribute = commandAttribute;
			ParameterTypes = commandMethod.GetParameters().Select(p => p.ParameterType).ToArray();
		}
	}


	public static class CommandHandler
	{
		private static SortedDictionary<string, CommandInfo> commandRegistry = new SortedDictionary<string, CommandInfo>();
		private static Dictionary<Type, IArgumentConverter> converters = new Dictionary<Type, IArgumentConverter>();
		public static List<IMiddleware> middlewares = new List<IMiddleware>
		{
			new GameModePermissionMiddleware(),
			new RolePermissionMiddleware(),
		};
		private static string[] categoryOrder = new string[] { "Reset CDs & Heal", "Blood Potions", "Witch & Rage Buffs", "Kits", "Jewels", "Legendaries", "Teleport", "Clan", "Misc"};

		static CommandHandler()
		{
			converters.Add(typeof(int), new IntegerConverter());
			converters.Add(typeof(bool), new BooleanConverter());
			converters.Add(typeof(float), new FloatConverter());
			converters.Add(typeof(Player), new PlayerConverter());
			converters.Add(typeof(PrefabGUID), new PrefabGUIDConverter());
			converters.Add(typeof(ItemPrefabData), new ItemPrefabDataConverter());
			converters.Add(typeof(BloodPrefabData), new BloodPrefabDataConverter());
			converters.Add(typeof(JewelPrefabData), new JewelPrefabDataConverter());
			var assembly = Assembly.GetCallingAssembly();

			foreach (var type in assembly.GetTypes())
			{
				foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
				{
					var commandAttribute = method.GetCustomAttribute<CommandAttribute>();
					if (commandAttribute != null)
					{
						object instance = null;
						if (!method.IsStatic)
						{
							if (type.GetConstructors().Any(ctor => ctor.GetParameters().Length == 0))
							{
								instance = Activator.CreateInstance(type);
							}
							else
							{
								// Handle the case where no parameterless constructor is available
								continue;
							}
						}

						var commandInfo = new CommandInfo(method, instance, commandAttribute);
						commandRegistry[commandAttribute.Name.ToLower()] = commandInfo;
						foreach (var alias in commandAttribute.Aliases)
						{
							commandRegistry[alias.ToLower()] = commandInfo;
						}
					}
				}
			}
		}

		public static (CommandInfo matchedCommand, string matchedText) FindMatchingCommand(string input)
		{
			CommandInfo matchedCommand = null;
			string matchedText = null;

			for (int i = input.Length; i > 0; i--)
			{
				string potentialCommand = input.Substring(0, i).ToLower();
				if (commandRegistry.TryGetValue(potentialCommand, out var commandInfo))
				{
					// Check if the command is the entire input or followed by a space
					if (i == input.Length || (i < input.Length && input[i] == ' '))
					{
						matchedCommand = commandInfo;
						matchedText = potentialCommand;
						break;
					}
				}
			}

			return (matchedCommand, matchedText);
		}

		public static bool ExecuteCommand(Player player, string input)
		{
			if (string.IsNullOrEmpty(input)) return false;
			string prefixUsed = input.Substring(0, 1);
			if (input.StartsWith(".") || input.StartsWith("ю") || input.StartsWith("Ю"))
			{
				// Remove the first character (the prefix)
				input = input.Substring(1);
			}
			else
			{
				return false;
			}
			var parts = SplitInput(input);
			string potentialCommand;
			CommandInfo matchedCommand;
			List<string> arguments;
			for (int i = 0; i < parts.Count; i++)
			{
				potentialCommand = string.Join(" ", parts.Take(parts.Count - i));
				if (commandRegistry.TryGetValue(potentialCommand.ToLower(), out matchedCommand))
				{
					arguments = parts.Skip(parts.Count - i).ToList();
					if (matchedCommand != null)
					{
						var commandMethod = matchedCommand.CommandMethod;
						var parametersInfo = commandMethod.GetParameters();
						int requiredParametersCount = parametersInfo.Count(p => !p.HasDefaultValue);
						if (parametersInfo.Length == 0)
						{
							player.ReceiveMessage("This command is not configured correctly.".Error());
							return false;
						}
						if (arguments.Count < requiredParametersCount - 1)
						{
							// Generate an error message for missing parameters
							string[] usageParts = matchedCommand.CommandAttribute.Usage.Split(new[] { ' ' }, 2);
							string usageArguments = usageParts.Length > 1 ? usageParts[1] : "";
							string usageMessage = $"Usage: {prefixUsed}{potentialCommand} {usageArguments}".Error();
							player.ReceiveMessage(usageMessage);
							return true;
						}
						var parameters = new object[parametersInfo.Length];
						parameters[0] = player;
						try
						{
							for (int j = 1; j < parametersInfo.Length; j++)
							{
								if (j - 1 < arguments.Count) //we still have arguments from them to parse
								{
									var paramType = parametersInfo[j].ParameterType;
									if (converters.TryGetValue(paramType, out var converter))
									{
										if (!converter.TryConvert(arguments[j - 1], paramType, out var convertedValue))
										{
											player.ReceiveMessage($"Invalid value for {parametersInfo[i].Name}".Error());
											return true;
										}
										parameters[j] = convertedValue;
									}
									else if (paramType == typeof(string))
									{
										parameters[j] = arguments[j - 1];
									}
									else
									{
										player.ReceiveMessage("That command is not set up correctly.".Error());
										return true;
									}
								}
								else if (parametersInfo[j].HasDefaultValue) //no longer have arguments to parse, go to defaults
								{
									parameters[j] = parametersInfo[j].DefaultValue;
								}
								else
								{	
									return true;
								}
							}

							var canExecute = true;
							foreach (var middleware in middlewares)
							{
								if (!middleware.CanExecute(player, matchedCommand.CommandAttribute, commandMethod))
								{
									canExecute = false;
									break;
								}
							}

							if (canExecute)
							{
								GameEvents.RaisePlayerChatCommand(player, matchedCommand.CommandAttribute);
								if (matchedCommand.CommandMethod.IsStatic)
								{
									matchedCommand.CommandMethod.Invoke(null, parameters);
								}
								else if (matchedCommand.CommandInstance != null && matchedCommand.CommandMethod.DeclaringType.IsInstanceOfType(matchedCommand.CommandInstance))
								{
									matchedCommand.CommandMethod.Invoke(matchedCommand.CommandInstance, parameters);
								}
								return true;
							}
							else
							{
								return true;
							}
						}
						catch (Exception ex)
						{
							Plugin.PluginLog.LogInfo($"input '{input}' resulted in {ex.ToString()}");
							player.ReceiveMessage($"An error occurred: {ex.ToString()}".Error());
							return true;
						}
					}
					break;
				}
			}
			
			return false;
		}

		private static List<string> SplitInput(string input)
		{
			var parts = new List<string>();
			var inQuotes = false;
			var currentPart = new StringBuilder();

			foreach (var c in input)
			{
				if (c == '"')
				{
					inQuotes = !inQuotes;
					continue;
				}

				if (!inQuotes && c == ' ')
				{
					if (currentPart.Length > 0)
					{
						parts.Add(currentPart.ToString());
						currentPart.Clear();
					}
				}
				else
				{
					currentPart.Append(c);
				}
			}

			if (currentPart.Length > 0)
			{
				parts.Add(currentPart.ToString());
			}

			return parts;
		}

		[Command(name: "help", description: "Lists all commands", usage: ".help", adminOnly: false)]
		public static void HelpCommand(Player player, string commandOrCategoryName = "")
		{
			var helpText = new List<string>();

			if (!string.IsNullOrEmpty(commandOrCategoryName))
			{
				commandOrCategoryName = char.ToUpper(commandOrCategoryName[0]) + commandOrCategoryName.Substring(1);
				// Check if the argument matches a category
				var isCategory = commandRegistry.Values.Any(cmd => cmd.CommandAttribute.Category?.Equals(commandOrCategoryName, StringComparison.OrdinalIgnoreCase) ?? false);

				if (isCategory)
				{
					// HashSet to keep track of commands already added
					var addedCommands = new HashSet<string>();
					helpText.Add($"{commandOrCategoryName} Commands".Colorify(ExtendedColor.LightServerColor));

					// Get all commands in the category
					var commandsInCategory = commandRegistry.Values
						.Where(cmd => cmd.CommandAttribute.Category?.Equals(commandOrCategoryName, StringComparison.OrdinalIgnoreCase) == true)
						.ToList();

					for (int i = 0; i < commandsInCategory.Count; i++)
					{
						var cmd = commandsInCategory[i];
						if (!addedCommands.Contains(cmd.CommandAttribute.Name))
						{
							var usageString = (!string.IsNullOrEmpty(cmd.CommandAttribute.Usage) ? cmd.CommandAttribute.Usage : ("." + cmd.CommandAttribute.Name));

							// Format and add the usage string
							helpText.Add($"{"Command".Colorify(ExtendedColor.ServerColor)}: {usageString.White()}");

							// Format and add the description, if available
							if (!string.IsNullOrEmpty(cmd.CommandAttribute.Description))
							{
								helpText.Add($"{"Description".Colorify(ExtendedColor.ServerColor)}: {cmd.CommandAttribute.Description.White()}");
							}

							// Add a space for visual separation between commands, except after the last command
							if (i < commandsInCategory.Count - 1)
							{
								helpText.Add(string.Empty);
							}

							// Add the command name to the HashSet to avoid repetition
							addedCommands.Add(cmd.CommandAttribute.Name);
						}
					}
				}
				else if (commandRegistry.TryGetValue(commandOrCategoryName.ToLower(), out CommandInfo commandInfo))
				{
					// Handle specific command display
					var usageString = (!string.IsNullOrEmpty(commandInfo.CommandAttribute.Usage) ? commandInfo.CommandAttribute.Usage : ("." + commandInfo.CommandAttribute.Name));
					helpText.Add($"{"Usage".Colorify(ExtendedColor.ServerColor)}: {usageString.White()}");
					if (!string.IsNullOrEmpty(commandInfo.CommandAttribute.Description))
					{
						helpText.Add($"{"Description".Colorify(ExtendedColor.ServerColor)}: {commandInfo.CommandAttribute.Description.White()}");
					}
				}
				else
				{
					// Command or category not found
					helpText.Add($"No command or category found with the name '{commandOrCategoryName}'.");
				}
			}
			else
			{
				var categories = new Dictionary<string, List<string>>();
				var ungroupedCommands = new List<string>();
				var listedCommands = new HashSet<string>();

				foreach (var cmd in commandRegistry.Values)
				{
					if (cmd.CommandAttribute.IncludeInHelp && !listedCommands.Contains(cmd.CommandAttribute.Name))
					{
						var usageString = (!string.IsNullOrEmpty(cmd.CommandAttribute.Usage) ? cmd.CommandAttribute.Usage : ("." + cmd.CommandAttribute.Name));

						if (string.IsNullOrEmpty(cmd.CommandAttribute.Category))
						{
							// Handle ungrouped commands
							ungroupedCommands.Add(usageString);
						}
						else
						{
							// Group commands by category
							if (!categories.ContainsKey(cmd.CommandAttribute.Category))
							{
								categories[cmd.CommandAttribute.Category] = new List<string>();
							}
							categories[cmd.CommandAttribute.Category].Add(usageString);
						}

						listedCommands.Add(cmd.CommandAttribute.Name);
					}
				}

				// Sort and add ungrouped commands first
				helpText.Add("User Commands".Colorify(ExtendedColor.LightServerColor));
				ungroupedCommands.Sort();
				helpText.AddRange(ungroupedCommands.Select(cmd => cmd.Colorify(ExtendedColor.ServerColor)));

				foreach (var categoryName in categoryOrder)
				{
					if (categories.TryGetValue(categoryName, out List<string> categoryCommands))
					{
						categoryCommands.Sort();
						var categoryEntry = $"{categoryName.Colorify(ExtendedColor.ServerColor)}: {string.Join(" / ", categoryCommands)}".Colorify(ExtendedColor.White);
						helpText.Add(categoryEntry);
					}
				}
			}

			// Now send the help text
			foreach (var line in helpText)
			{
				player.ReceiveMessage(line);
			}
		}

		[Command(name: "log-all-commands", description: "Logs all commands", usage: ".log-all-commands", adminOnly: true)]
		public static void LogAllCommandsCommand(Player player)
		{
			var loggedCommands = new HashSet<string>();

			foreach (var commandEntry in commandRegistry)
			{
				var cmd = commandEntry.Value;
				var commandName = cmd.CommandAttribute.Name;

				// Skip if this command name has already been logged
				if (loggedCommands.Contains(commandName))
				{
					continue;
				}

				var commandUsage = string.IsNullOrEmpty(cmd.CommandAttribute.Usage) ? commandName : cmd.CommandAttribute.Usage;
				var commandDescription = cmd.CommandAttribute.Description ?? "No description";
				var adminOnly = cmd.CommandAttribute.AdminOnly ? "Admin only" : "User";

				// Format the log string
				var logString = $"Name: {commandName}, Usage: {commandUsage}, Description: {commandDescription}, Type: {adminOnly}";

				// Log the command details
				Plugin.PluginLog.LogInfo(logString);

				// Add the command name to the logged commands set
				loggedCommands.Add(commandName);
			}
		}
	}
}

