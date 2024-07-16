using Oxide.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("MagicRanks", "herbs.acab", "1.0.3")]
    [Description("Tracks player playtime and assigns them to specific Oxide groups after reaching certain playtime thresholds.")]
    public class MagicRanks : RustPlugin
    {
        private Dictionary<ulong, double> playerPlaytimes = new Dictionary<ulong, double>();
        private Dictionary<string, double> rewardRanks = new Dictionary<string, double>();
        private Dictionary<ulong, double> sessionStartTimes = new Dictionary<ulong, double>();

        private void Init()
        {
            LoadConfigValues();
        }

        protected override void LoadDefaultConfig()
        {
            PrintWarning("Creating a new configuration file.");

            Config["RewardRanks"] = new Dictionary<string, double>
            {
                {"rank1", 10.0},
                {"rank2", 20.0},
                {"rank3", 30.0}
            };
            SaveConfig();
        }

        private void LoadConfigValues()
        {
            rewardRanks = Config["RewardRanks"] as Dictionary<string, double>;
            if (rewardRanks == null)
            {
                PrintError("Failed to load reward ranks from config. Creating default values.");
                LoadDefaultConfig();
            }
        }

        private void OnPlayerInit(BasePlayer player)
        {
            if (!playerPlaytimes.ContainsKey(player.userID))
            {
                playerPlaytimes[player.userID] = 0.0;
            }

            sessionStartTimes[player.userID] = Time.realtimeSinceStartup;
        }

        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            if (sessionStartTimes.ContainsKey(player.userID))
            {
                double sessionTime = (Time.realtimeSinceStartup - sessionStartTimes[player.userID]) / 3600.0; // Convert seconds to hours
                playerPlaytimes[player.userID] += sessionTime;
                sessionStartTimes.Remove(player.userID);
                CheckRewards(player);
            }
        }

        private void TrackPlaytime(BasePlayer player)
        {
            if (player == null || !player.IsConnected) return;

            double sessionTime = (Time.realtimeSinceStartup - sessionStartTimes[player.userID]) / 3600.0; // Convert seconds to hours
            playerPlaytimes[player.userID] += sessionTime;
            sessionStartTimes[player.userID] = Time.realtimeSinceStartup; // Reset session start time
            CheckRewards(player);
        }

        private void CheckRewards(BasePlayer player)
        {
            if (player == null) return;

            foreach (var rank in rewardRanks)
            {
                if (playerPlaytimes[player.userID] >= rank.Value && !permission.UserHasGroup(player.UserIDString, rank.Key))
                {
                    permission.AddUserGroup(player.UserIDString, rank.Key);
                    PrintToChat(player, $"Congratulations! You have been promoted to {rank.Key} for playing {rank.Value} hours.");
                }
            }
        }

        private void OnServerSave()
        {
            Interface.Oxide.DataFileSystem.WriteObject("MagicRanks", playerPlaytimes);
        }

        private void Unload()
        {
            OnServerSave();
        }

        private void OnServerInitialized()
        {
            playerPlaytimes = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<ulong, double>>("MagicRanks");
            timer.Every(3600, () => SavePlaytimes());
        }

        private void SavePlaytimes()
        {
            foreach (var player in BasePlayer.activePlayerList)
            {
                TrackPlaytime(player);
            }
            OnServerSave();
        }
    }
}
