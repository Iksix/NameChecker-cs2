using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace NameChecker;

public class NameCheckerConfig : BasePluginConfig
{
    [JsonPropertyName("PluginTag")] public string PluginTag { get; set; } = "[NameChecker]";
    [JsonPropertyName("PluginTagColor")] public string PluginTagColor { get; set; } = "Gold";
    [JsonPropertyName("PluginMode")] public int PluginMode { get; set; } = 0;
    [JsonPropertyName("KickTime")] public int KickTime { get; set; } = 10;
    [JsonPropertyName("PluginSiteReplace")] public string PluginSiteReplace { get; set; } = "example.com";
    [JsonPropertyName("Domains")] public string[] Domains { get; set; } = {".ru", ".com", ".net", ".org"};
}