# MagicRanks

MagicRanks is a Rust plugin that tracks player playtime and assigns them to specific Oxide groups after reaching configurable playtime thresholds.

## Features

- Track player playtime in hours.
- Assign players to specific groups based on their playtime.
- Configurable reward ranks with custom playtime thresholds.

## Installation

1. Download the `MagicRanks.cs` file and place it in your `oxide/plugins` directory.
2. The plugin will automatically create a default configuration file on first run.

## Configuration

The configuration file (`oxide/config/MagicRanks.json`) allows you to define reward ranks and their corresponding playtime thresholds.

### Default Configuration

```json
{
  "RewardRanks": {
    "rank1": 10.0,
    "rank2": 20.0,
    "rank3": 30.0
  }
}
```

### Example Custom Configuration
```json
{
  "RewardRanks": {
    "Novice": 5.0,
    "Experienced": 15.0,
    "Veteran": 25.0
  }
}
```
- The keys (Novice, Experienced, Veteran) are the names of the Oxide groups.
- The values (5.0, 15.0, 25.0) represent the playtime thresholds in hours.
- You can add or modify ranks as needed.

### Data Storage
The plugin saves player playtime data to a file (oxide/data/MagicRanks.json) to persist playtime information across server restarts.

### License
This plugin is released under the MagicServices License.
