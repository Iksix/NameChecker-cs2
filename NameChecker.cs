using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace NameChecker;

public class NameCheckerConfig : BasePluginConfig
{
    [JsonPropertyName("PluginMode")] public int C_PluginMode { get; set; } = 0;
    [JsonPropertyName("KickTime")] public int C_KickTime { get; set; } = 10;
    [JsonPropertyName("SiteReplace")] public string C_SiteReplace { get; set; } = "example.com";
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
    public int PluginMode = 2; // 0 - Default(Kick Players); 1 - Remove Nickname and set random; 2 - Replace name xxx.com from other server to aaa.com
    public int kickTime = 10; // Seconds
    public string siteReplace = "aaa.com";  // Site replace if mode 2;
    // ================================
    
    // ================================ LISTES
    private List<string> names = new List<string>();
    private List<string> namesToReplace = new List<string>();
    private List<Timer> _timers = new List<Timer>();
    // ================================


    public void OnConfigParsed(NameCheckerConfig config)
    {
        if (config.C_PluginMode > 2 || config.C_PluginMode < 0)
        {
            Console.WriteLine("[NameChecker] PluginMode must be value between [0-2] || 0 as default");
            PluginMode = 0;
        }
        else PluginMode = config.C_PluginMode;
        kickTime = config.C_KickTime;
        siteReplace = config.C_SiteReplace;
        
        Config = config;
    }

    public override void Load(bool hotReload)
    {
        NamesReload();
    }

    public void NamesReload()
    {
        OnConfigParsed(Config);
        names = new List<string>();
        StreamReader reader = new StreamReader($"{ModuleDirectory}/names.txt");
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            names.Add(line);
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
        Console.WriteLine($"[NameChecker] Mode: {PluginMode}");
        Console.WriteLine($"[NameChecker] KickTime: {kickTime}");
        Console.WriteLine($"[NameChecker] ReplaceSite: {siteReplace}");

    }

    [ConsoleCommand("css_nc_reload")]
    public void onReloadCommand(CCSPlayerController? controller, CommandInfo eventInfo)
    {
        NamesReload();
        if (controller != null)
        {
            controller.PrintToChat("[NameChecker] Names reloaded");
        }
    }
    
    [GameEventHandler]
    public HookResult onPlayerConnect(EventPlayerConnectFull @event, GameEventInfo gameEventInfo)
    {
        checkName(@event.Userid);
        return HookResult.Continue;
    }
    [GameEventHandler]
    public HookResult onPlayerConnect(EventRoundStart @event, GameEventInfo gameEventInfo)
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
        
        if (PluginMode == 0) // Kick player
        {
            if (names.Any(playerName.Contains))
            {
                controller.PrintToConsole("Your nickname is banned! Change it!");
                controller.PrintToConsole("Banned nickname list:");
                foreach (var name in names)
                {
                    controller.PrintToConsole(name);
                }

                int timeCounter = 0;
                controller.PrintToChat($"{Green}[NameChecker]{Red} Your nickname is banned! Change it!");
                controller.PrintToChat($"{Green}[NameChecker]{Olive} Check console to see banned nickname list!");
                int timerIndex = _timers.Count;
                Console.WriteLine(timerIndex);
                
                _timers.Add(AddTimer(1, () =>
                {
                    controller.PrintToChat($"{Green}[NameChecker]{Red} You will be kicked trough {kickTime - timeCounter} seconds!");
                    timeCounter++;
                    if (timeCounter == kickTime)
                    {
                        NativeAPI.IssueServerCommand($"kickid {controller.UserId} Your nickname is banned! Change it!");
                        Console.WriteLine($"[NameChecker] Player with name < {controller.PlayerName} > was kicked");
                        _timers[timerIndex].Kill();
                        _timers.RemoveAt(timerIndex);
                    }
                }, TimerFlags.REPEAT));
            }
        }
        else if (PluginMode == 1)
        {
            if (names.Any(playerName.Contains))
            {
                Random rnd = new Random();
                string newName = namesToReplace[rnd.Next(0, namesToReplace.Count)];

                PlayerName<CBasePlayerController> _playerName = new PlayerName<CBasePlayerController>(controller, "m_iszPlayerName");
                _playerName.Set(newName);
                
                controller.PrintToChat($"{Green}[NameChecker] {Red} Your nickname is banned! It will be changed on {newName}");
                controller.PrintToChat($"{Green}[NameChecker]{Olive} Check console to see banned nickname list!");
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