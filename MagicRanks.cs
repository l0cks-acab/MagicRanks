using Oxide.Core;
using Oxide.Core.Plugins;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("MagicRanks", "herbs.acab", "1.0.3")]
    [Description("Awards players by adding them to a group after a specified amount of playtime.")]
    public class MagicRanks : RustPlugin
    {
        private Dictionary<string, double> groupTimes;

        private const string permAdmin = "magicranks.admin";

        private void Init()
        {
            permission.RegisterPermission(permAdmin, this);
            LoadConfigValues();
        }

        protected override void LoadDefaultConfig()
        {
            PrintWarning("Creating a new configuration file.");
            Config.Clear();
            Config["GroupTimes"] = new Dictionary<string, object>
            {
                { "Group1", 5.0 },
                { "Group2", 10.0 },
                { "Group3", 20.0 }
            };
            SaveConfig();
        }

        private void LoadConfigValues()
        {
            groupTimes = new Dictionary<string, double>();

            var configData = Config["GroupTimes"] as Dictionary<string, object>;
            if (configData == null)
            {
                PrintWarning("Invalid configuration format. Loading default values.");
                LoadDefaultConfig();
                configData = Config["GroupTimes"] as Dictionary<string, object>;
            }

            foreach (var entry in configData)
            {
                if (entry.Value is double hours)
                {
                    groupTimes[entry.Key] = hours;
                }
                else
                {
                    PrintWarning($"Invalid value for group {entry.Key}. Expected a double.");
                }
            }
        }

        [ChatCommand("magicranks_add")]
        private void CmdMagicRanksAdd(BasePlayer player, string command, string[] args)
        {
            if (!IsAdmin(player)) return;
            if (args.Length != 2) 
            {
                player.ChatMessage("Usage: /magicranks_add [group] [hours]");
                return;
            }

            string group = args[0];
            if (!double.TryParse(args[1], out double hours))
            {
                player.ChatMessage("Invalid hours value. Please enter a valid number.");
                return;
            }

            groupTimes[group] = hours;
            SaveConfig();
            player.ChatMessage($"Group {group} set to {hours} hours.");
        }

        [ChatCommand("magicranks_edit")]
        private void CmdMagicRanksEdit(BasePlayer player, string command, string[] args)
        {
            if (!IsAdmin(player)) return;
            if (args.Length != 2) 
            {
                player.ChatMessage("Usage: /magicranks_edit [group] [hours]");
                return;
            }

            string group = args[0];
            if (!double.TryParse(args[1], out double hours))
            {
                player.ChatMessage("Invalid hours value. Please enter a valid number.");
                return;
            }

            if (!groupTimes.ContainsKey(group))
            {
                player.ChatMessage($"Group {group} does not exist.");
                return;
            }

            groupTimes[group] = hours;
            SaveConfig();
            player.ChatMessage($"Group {group} updated to {hours} hours.");
        }

        [ChatCommand("magicranks_remove")]
        private void CmdMagicRanksRemove(BasePlayer player, string command, string[] args)
        {
            if (!IsAdmin(player)) return;
            if (args.Length != 1)
            {
                player.ChatMessage("Usage: /magicranks_remove [group]");
                return;
            }

            string group = args[0];

            if (!groupTimes.ContainsKey(group))
            {
                player.ChatMessage($"Group {group} does not exist.");
                return;
            }

            groupTimes.Remove(group);
            SaveConfig();
            player.ChatMessage($"Group {group} removed.");
        }

        [ChatCommand("magicranks_list")]
        private void CmdMagicRanksList(BasePlayer player, string command, string[] args)
        {
            if (!IsAdmin(player)) return;

            player.ChatMessage("Current groups and required hours:");
            foreach (var entry in groupTimes)
            {
                player.ChatMessage($"{entry.Key}: {entry.Value} hours");
            }
        }

        private bool IsAdmin(BasePlayer player)
        {
            if (player == null || !permission.UserHasPermission(player.UserIDString, permAdmin))
            {
                player.ChatMessage("You do not have permission to use this command.");
                return false;
            }
            return true;
        }

        private void SaveConfig()
        {
            Config["GroupTimes"] = groupTimes;
            Config.Save();
        }
    }
}
