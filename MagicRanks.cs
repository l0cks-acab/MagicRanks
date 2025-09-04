using Oxide.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("MagicRanks", "herbs.acab", "1.0.7")]
    [Description("Tracks player playtime and assigns them to Oxide groups after reaching playtime thresholds.")]
    public class MagicRanks : RustPlugin
    {
        private Dictionary<ulong, double> playerPlaytimes = new Dictionary<ulong, double>(); // PlayerID to total hours played
        private Dictionary<ulong, double> sessionStartTimes = new Dictionary<ulong, double>(); // PlayerID to session start time (realtimeSinceStartup)

        // Playtime thresholds to rank name
        private Dictionary<string, double> rewardRanks = new Dictionary<string, double>()
        {
            {"vip1", 5.0},
            {"vip2", 15.0},
            {"vip3", 25.0}
        };

        private void Init()
        {
            LoadPlayerPlaytimes();
        }

        // Load player playtimes from data file
        private void LoadPlayerPlaytimes()
        {
            playerPlaytimes = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<ulong, double>>("MagicRanks");
            if (playerPlaytimes == null)
                playerPlaytimes = new Dictionary<ulong, double>();
        }

        // Save player playtimes to data file
        private void SavePlayerPlaytimes()
        {
            Interface.Oxide.DataFileSystem.WriteObject("MagicRanks", playerPlaytimes);
        }

        private void OnPlayerInit(BasePlayer player)
        {
            if (!playerPlaytimes.ContainsKey(player.userID))
                playerPlaytimes[player.userID] = 0.0;

            sessionStartTimes[player.userID] = Time.realtimeSinceStartup;
        }

        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            if (sessionStartTimes.TryGetValue(player.userID, out double startTime))
            {
                double sessionDurationHours = (Time.realtimeSinceStartup - startTime) / 3600.0;
                playerPlaytimes[player.userID] = playerPlaytimes.GetValueOrDefault(player.userID, 0) + sessionDurationHours;

                sessionStartTimes.Remove(player.userID);
                CheckRewards(player);
            }
        }

        // Periodically update playtime for all connected players and save data
        private void SavePlaytimesPeriodically()
        {
            foreach (var player in BasePlayer.activePlayerList)
            {
                TrackPlaytime(player);
            }
            SavePlayerPlaytimes();
        }

        // Update playtime for a player during their current session
        private void TrackPlaytime(BasePlayer player)
        {
            if (player == null || !player.IsConnected) return;

            if (!sessionStartTimes.TryGetValue(player.userID, out double startTime))
                return; // No session start time, can't track

            double sessionDurationHours = (Time.realtimeSinceStartup - startTime) / 3600.0;
            playerPlaytimes[player.userID] = playerPlaytimes.GetValueOrDefault(player.userID, 0) + sessionDurationHours;
            sessionStartTimes[player.userID] = Time.realtimeSinceStartup; // Reset start time after adding

            CheckRewards(player);
        }

        // Check if player qualifies for a rank and assign group
        private void CheckRewards(BasePlayer player)
        {
            if (player == null) return;
            double totalHours = playerPlaytimes.GetValueOrDefault(player.userID, 0);

            foreach (var rank in rewardRanks)
            {
                if (totalHours >= rank.Value && !permission.UserHasGroup(player.UserIDString, rank.Key))
                {
                    permission.AddUserGroup(player.UserIDString, rank.Key);
                    PrintToChat(player, $"Congratulations! You have been promoted to {rank.Key} for playing {rank.Value} hours.");
                }
            }
        }

        private void OnServerSave()
        {
            SavePlayerPlaytimes();
        }

        private void Unload()
        {
            SavePlayerPlaytimes();
        }

        private void OnServerInitialized()
        {
            LoadPlayerPlaytimes();
            timer.Every(3600f, SavePlaytimesPeriodically); // Save every hour (3600 seconds)
        }
    }
}
