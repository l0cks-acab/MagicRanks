None of my Rust Plugins are updated, please feel free to take over the project, all plugins are MIT open source.

### MagicRanks
MagicRanks is a Rust plugin that tracks player playtime and assigns them to specific Oxide groups after reaching certain playtime thresholds. This plugin helps in rewarding players based on their commitment and time spent on the server.
**Version:** 1.0.6
**Author:** herbs.acab

### Features
- Tracks player playtime in hours.
- Automatically assigns players to specific Oxide groups based on their playtime.

### Customization
#### Changing Rank Names and Time Needed
Admins can change the rank names and the time needed to achieve each rank by editing the rewardRanks dictionary in the plugin's source code. Here is the relevant section of the code:
```c# // Define reward ranks and thresholds directly in the code
private Dictionary<string, double> rewardRanks = new Dictionary<string, double>
{
    {"vip1", 5.0},
    {"vip2", 15.0},
    {"vip3", 25.0}
};
```

To change the rank names or time needed, simply modify the dictionary entries. For example, to add a new rank "vip4" that requires 50 hours of playtime, update the dictionary as follows:
```c# // Define reward ranks and thresholds directly in the code
private Dictionary<string, double> rewardRanks = new Dictionary<string, double>
{
    {"vip1", 5.0},
    {"vip2", 15.0},
    {"vip3", 25.0},
    {"vip4", 50.0}
};
```

### Data Storage
Player playtime data is stored in a JSON file located in the oxide/data directory. 

`File Location: oxide/data/MagicRanks.json`

- The file contains a dictionary where the keys are player IDs (ulong) and the values are their total playtimes (double) in hours.

### Permissions
Ensure that the groups specified in the plugin (vip1, vip2, legend, novice, ect) exist in your Oxide permissions system and have the desired permissions set up.
