using Oxide.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("MagicRanks", "herbs.acab", "1.0.6")]
    [Description("Tracks player playtime and assigns them to specific Oxide groups after reaching certain playtime thresholds.")]
    public class MagicRanks : RustPlugin
    {
        private Dictionary<ulong, double> playerPlaytimes = new Dictionary<ulong, double>(); // Stores player IDs and their total playtimes
        private Dictionary<ulong, double> sessionStartTimes = new Dictionary<ulong, double>(); // Stores session start times for currently connected players
        
        private Dictionary<string, double> rewardRanks = new Dictionary<string, double>
        
         // Define reward ranks and thresholds here, you can add/remove ranks too, follow the format.
        {
            {"vip1", 5.0}, 
            {"vip2", 15.0},
            {"vip3", 25.0}
        };
         // I'm too lazy to add a config, and I wanted it as lightweight as possible. If you need help ping me on discord: herbs.acab

        private void Init()
        {
            LoadPlayerPlaytimes();
        }

        // Load player playtimes from the JSON file
        private void LoadPlayerPlaytimes()
        {
            playerPlaytimes = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<ulong, double>>("MagicRanks");
            if (playerPlaytimes == null)
            {
                playerPlaytimes = new Dictionary<ulong, double>();
            }
        }

        private void OnPlayerInit(BasePlayer player)
        {
            if (!playerPlaytimes.ContainsKey(player.userID))
            {
                playerPlaytimes[player.userID] = 0.0;
            }

            sessionStartTimes[player.userID] = Time.realtimeSinceStartup; // Record the session start time
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

        // Track playtime and update the dictionary
        private void TrackPlaytime(BasePlayer player)
        {
            if (player == null || !player.IsConnected) return;

            double sessionTime = (Time.realtimeSinceStartup - sessionStartTimes[player.userID]) / 3600.0; // Convert seconds to hours
            playerPlaytimes[player.userID] += sessionTime;
            sessionStartTimes[player.userID] = Time.realtimeSinceStartup; // Reset session start time
            CheckRewards(player);
        }

        // Check if the player qualifies for any rewards based on playtime
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

        // Save player playtimes to the JSON file
        private void OnServerSave()
        {
            Interface.Oxide.DataFileSystem.WriteObject("MagicRanks", playerPlaytimes);
        }

        private void Unload()
        {
            OnServerSave();
        }

        // Load player playtimes when the server initializes and set up periodic saving
        private void OnServerInitialized()
        {
            LoadPlayerPlaytimes();
            timer.Every(3600, () => SavePlaytimes());
        }

        // Save playtimes periodically
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
