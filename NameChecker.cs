using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Config;
using CounterStrikeSharp.API.Modules.Timers;
using Microsoft.VisualBasic;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace NameChecker;

public class NameCheckerConfig : BasePluginConfig
{
    [JsonPropertyName("PluginMode")] public int C_PluginMode { get; set; } = 0;
    [JsonPropertyName("KickTime")] public int C_KickTime { get; set; } = 10;
    [JsonPropertyName("SiteReplace")] public string C_SiteReplace { get; set; } = "example.com";
    
    [JsonPropertyName("pNameBanned")] public string C_pNameBanned { get; set; } = "Your nickname is banned! Change it!";
    [JsonPropertyName("pNameBannedWillChanged")] public string C_pNameBannedWillChanged { get; set; } = "Your nickname is banned! It will be changed on";
    [JsonPropertyName("pCheckConsole")] public string C_pCheckConsole { get; set; } = "Check console to see banned nickname list!";
    [JsonPropertyName("pKickTroughSeconds")] public string C_pKickTroughSeconds { get; set; } = "You will be kicked trough {seconds} seconds!";
}

public class NameChecker : BasePlugin, IPluginConfig<NameCheckerConfig>
{
    public NameCheckerConfig Config { get; set; }

    // ================================ COLORS
    public static char Default = '\x01';
    public static char White = '\x01';
    public static char Darkred = '\x02';
    public static char Green = '\x04';
    public static char LightYellow = '\x03';
    public static char LightBlue = '\x03';
    public static char Olive = '\x05';
    public static char Lime = '\x06';
    public static char Red = '\x07';
    public static char Purple = '\x03';
    public static char Grey = '\x08';
    public static char Yellow = '\x09';
    public static char Gold = '\x10';
    public static char Silver = '\x0A';
    public static char Blue = '\x0B';
    public static char DarkBlue = '\x0C';
    public static char BlueGrey = '\x0D';
    public static char Magenta = '\x0E';
    public static char LightRed = '\x0F';
    // ================================

    // ================================ INTERFACE
    public override string ModuleName => "Name Checker";
    public override string ModuleVersion => "1.1.0";
    public override string ModuleAuthor => "iks";
    // ================================

    // ================================ SETTINGS
    public int PluginMode = 0; // 0 - Default(Kick Players); 1 - Remove Nickname and set random; 2 - Replace name xxx.com from other server to aaa.com
    public int kickTime = 10; // Seconds
    public string siteReplace = "aaa.com";  // Site replace if mode 2;
    // ================================
    
    // ================================ LISTES
    private List<string> names = new List<string>();
    private List<string> namesToReplace = new List<string>();
    private List<string> whitelist = new List<string>(); // for mode 2
    private List<Timer> _timers = new ();
    private List<CCSPlayerController> playersProccesed = new List<CCSPlayerController>();
    private List<string> phrases = new List<string>(); // phrases
    // ================================


    public void OnConfigParsed(NameCheckerConfig config)
    {
        config = ConfigManager.Load<NameCheckerConfig>("NameChecker");
        PluginMode = config.C_PluginMode;
        kickTime = config.C_KickTime;
        siteReplace = config.C_SiteReplace;
        Console.WriteLine($"[NameChecker] config.C_PluginMode {config.C_PluginMode}");
        Console.WriteLine($"[NameChecker] config.C_KickTime {config.C_KickTime}");
        Console.WriteLine($"[NameChecker] config.C_SiteReplace {config.C_SiteReplace}");
        phrases = new List<string>();
        phrases.Add(config.C_pNameBanned);
        phrases.Add(config.C_pNameBannedWillChanged);
        phrases.Add(config.C_pCheckConsole);
        phrases.Add(config.C_pKickTroughSeconds);
        
        Config = config;
        Console.WriteLine("[NameChecker] Config Parsed");
        
    }

    public override void Load(bool hotReload)
    {
        NamesReload();
    }

    public void NamesReload()
    {
        names = new List<string>();
        StreamReader reader = new StreamReader($"{ModuleDirectory}/names.txt");
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            names.Add(line.ToLower());
        }
        Console.WriteLine("[NameChecker] Forbidden names:");
        foreach (string name in names)
        {
            Console.WriteLine(name);
        }
        reader.Close();
        namesToReplace = new List<string>();
        reader = new StreamReader($"{ModuleDirectory}/namesToReplace.txt");
        line = "";
        while ((line = reader.ReadLine()) != null)
        {
            namesToReplace.Add(line);
        }
        Console.WriteLine("[NameChecker] Names to replace:");
        foreach (string name in namesToReplace)
        {
            Console.WriteLine(name);
        }
        reader.Close();
        reader = new StreamReader($"{ModuleDirectory}/whitelist.txt");
        line = "";
        while ((line = reader.ReadLine()) != null)
        {
            whitelist.Add(line.ToLower());
        }
        Console.WriteLine("[NameChecker] Names to replace:");
        foreach (string name in whitelist)
        {
            Console.WriteLine(whitelist);
        }
        reader.Close();
        Console.WriteLine($"[NameChecker] Mode: {PluginMode}");
        Console.WriteLine($"[NameChecker] KickTime: {kickTime}");
        Console.WriteLine($"[NameChecker] ReplaceSite: {siteReplace}");

    }

    [ConsoleCommand("css_nc_reload")]
    public void onReloadCommand(CCSPlayerController? controller, CommandInfo eventInfo)
    {
        OnConfigParsed(Config);
        Console.WriteLine(Config.C_PluginMode);
        NamesReload();
        foreach (var timer in _timers)
        {
            timer.Kill();
        }
        _timers.Clear();
        if (controller != null)
        {
            controller.PrintToChat("[NameChecker] Names reloaded");
            foreach (var name in names)
            {
                controller.PrintToChat(name);
            }
        }

        
    }
    
    [GameEventHandler]
    public HookResult onPlayerConnect(EventPlayerConnectFull @event, GameEventInfo gameEventInfo)
    {
        checkName(@event.Userid);
        return HookResult.Continue;
    }
    [GameEventHandler]
    public HookResult onRoundStart(EventRoundStart @event, GameEventInfo gameEventInfo)
    {
        List<CCSPlayerController> players = Utilities.GetPlayers();
        foreach (var player in players)
        {
            checkName(player);
        }
        return HookResult.Continue;
    }


    public void checkName(CCSPlayerController controller)
    {
        string playerName = controller.PlayerName;
        foreach (var player in playersProccesed)
        {
            if (controller == player)
            {
                return;
            }
        }
        if (PluginMode == 0) // Kick player
        {
            if (names.Any(playerName.ToLower().Contains))
            {
                playersProccesed.Add(controller);
                controller.PrintToConsole("Your nickname is banned! Change it!");
                controller.PrintToConsole("Banned nickname list:");
                foreach (var name in names)
                {
                    controller.PrintToConsole(name);
                }

                int timeCounter = 0;
                controller.PrintToChat($"{Green}[NameChecker]{Red} {phrases[0]}");
                controller.PrintToChat($"{Green}[NameChecker]{Olive} {phrases[2]}");
                int timerIndex = _timers.Count;
                

                _timers.Add(AddTimer(1, () =>
                {
                    int seconds = kickTime - timeCounter;
                    
                    controller.PrintToChat( $"{Green}[NameChecker]{Red} " + String.Format(phrases[3], seconds));
                    timeCounter++;
                    if (timeCounter == kickTime)
                    {
                        NativeAPI.IssueServerCommand($"kickid {controller.UserId} Your nickname is banned! Change it!");
                        Console.WriteLine($"[NameChecker] Player with name < {controller.PlayerName} > was kicked");
                        for (int i = 0; i < playersProccesed.Count; i++)
                        {
                            if (playersProccesed[i] == controller)
                            {
                                playersProccesed.RemoveAt(i);
                            }
                        }
                        _timers[timerIndex].Kill();
                        _timers.RemoveAt(timerIndex);
                        Console.WriteLine($"Timer count = {_timers.Count}");

                    }
                }, TimerFlags.REPEAT));
            }
        }
        else if (PluginMode == 1)
        {
            if (names.Any(playerName.ToLower().Contains))
            {
                Random rnd = new Random();
                string newName = namesToReplace[rnd.Next(0, namesToReplace.Count)];

                PlayerName<CBasePlayerController> _playerName = new PlayerName<CBasePlayerController>(controller, "m_iszPlayerName");
                _playerName.Set(newName);
                
                controller.PrintToChat($"{Green}[NameChecker] {Red} {phrases[0]} {newName}");
                controller.PrintToChat($"{Green}[NameChecker]{Olive} {phrases[2]}");
                controller.PrintToConsole("Banned nickname list:");
                foreach (var name in names)
                {
                    controller.PrintToConsole(name);
                }
            }
        }
        else if (PluginMode == 2)
        {
            var playerNameWords = playerName.Split(" ");
            string newPlayerName = playerName;
            foreach (var word in playerNameWords)
            {
                bool wordInWhiteList = false;
                foreach (var name in whitelist)
                {
                    if (word.ToLower().Contains(name.ToLower()))
                    {
                        wordInWhiteList = true;
                    }
                }

                if (wordInWhiteList)
                {
                    continue;
                }
                if (word.Contains(".com") || word.Contains(".net") || word.Contains(".ru") || word.Contains(".org"))
                {
                    newPlayerName = newPlayerName.Replace(word, siteReplace);
                }
            }
            PlayerName<CBasePlayerController> _playerName = new PlayerName<CBasePlayerController>(controller, "m_iszPlayerName");
            _playerName.Set(newPlayerName);
        }
    }
}