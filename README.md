## NameChecker
A plugin for automatically checking player nicknames for forbidden words.

## Features
- Kick a player if there are forbidden words in his nickname
- Replacement of the player's nickname if there are forbidden words in his nickname
- Replacing the link in the player's name with your own (By domain)
- The translation file

## Configuration
- `"PluginMode": 0` - 0 - kick mode | 1 - replace the name with a random word from namesToReplace.txt | 2 - Link replacement (Works by domain) | 3 - Modes 0 and 2 together
- `"KickTime": 10` - Time to kick the player in seconds for `"PluginMode"` 0 and 3
- `"PluginSiteReplace": "example.com"` - The link to which the link in the player's nickname will be replaced for `"PluginMode"` 2 and 3
- `names.txt` - Banned names. For `"PluginMode"` 0, 1, 3
- `namesToReplace.txt` - The words that the name will be replaced with. For `"PluginMode"` 1
- `whitelist.txt` - exceptions for `"PluginMode"` 2 and 3

## Requirements
<a href="https://github.com/roflmuffin/CounterStrikeSharp">CounterStrikeSharp</a> Tested on v140
