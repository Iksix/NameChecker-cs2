using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Config;
using CounterStrikeSharp.API.Modules.Timers;
using FormatWith;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace NameChecker;

class NameChecker : BasePlugin, IPluginConfig<NameCheckerConfig>
{
    #region Colors
    
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

    #endregion
    
    #region Base

    public override string ModuleName { get; } = "NameChecker";
    public override string ModuleVersion { get; } = "2.0.0";
    public override string ModuleAuthor { get; } = "iks";

    #endregion

    #region Settings

    public string PluginTag = "[NameChecker]";
    public string PluginSiteReplace = "example.com";
    public char PluginTagColor = '\x10';
    public int PluginMode = 0;
    public int KickTime = 10;
    public string[] Domains = {".ru", ".com", ".net", ".org"};

    #endregion

    #region Lists and Massives

    private string[] _bannedNames;
    private string[] _replaceWords;
    private string[] _whitelist;
    private List<Timer> _timers = new ();
    private List<CCSPlayerController> _workedPlayers = new();

    #endregion

    #region Config
    
    public NameCheckerConfig Config { get; set; }

    public void OnConfigParsed(NameCheckerConfig config)
    {
        config = ConfigManager.Load<NameCheckerConfig>("NameChecker");
        switch (config.PluginTagColor)
        {
            case "Default":
                PluginTagColor = Default;
                break;
            case "White":
                PluginTagColor = White;
                break;
            case "Darkred":
                PluginTagColor = Darkred;
                break;
            case "Green":
                PluginTagColor = Green;
                break;
            case "LightYellow":
                PluginTagColor = LightYellow;
                break;
            case "LightBlue":
                PluginTagColor = LightBlue;
                break;
            case "Olive":
                PluginTagColor = Olive;
                break;
            case "Lime":
                PluginTagColor = Lime;
                break;
            case "Red":
                PluginTagColor = Red;
                break;
            case "Purple":
                PluginTagColor = Purple;
                break;
            case "Grey":
                PluginTagColor = Grey;
                break;
            case "Yellow":
                PluginTagColor = Yellow;
                break;
            case "Gold":
                PluginTagColor = Gold;
                break;
            case "Silver":
                PluginTagColor = Silver;
                break;
            case "Blue":
                PluginTagColor = Blue;
                break;
            case "DarkBlue":
                PluginTagColor = DarkBlue;
                break;
            case "BlueGrey":
                PluginTagColor = BlueGrey;
                break;
            case "Magenta":
                PluginTagColor = Magenta;
                break;
            case "LightRed":
                PluginTagColor = LightRed;
                break;
            default:
                // Handle the case where the color code is not recognized.
                // You might want to set a default color in this case.
                PluginTagColor = Default;
                break;
        }

        PluginTag = config.PluginTag;
        PluginMode = config.PluginMode;
        PluginSiteReplace = config.PluginSiteReplace;
        Domains = config.Domains;
        KickTime = config.KickTime;

        if (!File.Exists($"{ModuleDirectory}/names.txt"))
        {
            File.Create($"{ModuleDirectory}/names.txt");
        }
        if (!File.Exists($"{ModuleDirectory}/namesToReplace.txt"))
        {
            File.Create($"{ModuleDirectory}/namesToReplace.txt");
        }
        if (!File.Exists($"{ModuleDirectory}/whitelist.txt"))
        {
            File.Create($"{ModuleDirectory}/whitelist.txt");
        }
        
        string[] file = File.ReadAllLines($"{ModuleDirectory}/names.txt");
        _bannedNames = file;
        
        file = File.ReadAllLines($"{ModuleDirectory}/namesToReplace.txt");
        _replaceWords = file;

        file = File.ReadAllLines($"{ModuleDirectory}/whitelist.txt");
        _whitelist = file;
        
        Config = config;
    }
    
    #endregion

    [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_nc_reload")]
    public void OnReloadCommand(CCSPlayerController? controller, CommandInfo info)
    {
        OnConfigParsed(Config);
        Console.WriteLine($"{PluginTag} Plugin was reloaded!");
        if (controller != null)
        {
            controller.PrintToChat($" {PluginTagColor}{PluginTag} {Green}Plugin was reloaded!");
        }

        foreach (var timer in _timers)
        {
            timer.Kill();
        }
        _timers.Clear();

        foreach (var player in Utilities.GetPlayers())
        {
            checkNames(player);
        }
        
    }
    

    [GameEventHandler]
    public HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        checkNames(@event.Userid);
        
        return HookResult.Continue;
    }
    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        foreach (var player in Utilities.GetPlayers())
        {
            checkNames(player);
        }
        
        return HookResult.Continue;
    }
    [GameEventHandler]
    public HookResult OnMapShutDown(EventMapShutdown @event, GameEventInfo info)
    {
        OnConfigParsed(Config);
        Console.WriteLine($"{PluginTag} Plugin was reloaded!");

        foreach (var timer in _timers)
        {
            timer.Kill();
        }
        _timers.Clear();

        foreach (var player in Utilities.GetPlayers())
        {
            checkNames(player);
        }
        return HookResult.Continue;
    }

    public void checkNames(CCSPlayerController controller)
    {
        if (controller.IsBot)
        {
            return;
        }
        switch (PluginMode)
        {
            case 0:
                KickMode(controller);
                break;
            case 1:
                RenamePlayerOnRandomWord(controller);
                break;
            case 2:
                RenameSiteMode(controller);
                break;
            case 3:
                RenameSiteAndKickMode(controller);
                break;
        }
    }

    public void PrintBannedNamesInConsole(CCSPlayerController controller)
    {
        controller.PrintToConsole($"{PluginTag} " + Localizer["main.bannedListInConsole"]);
        foreach (var word in _bannedNames)
        {
            controller.PrintToConsole(word);
        }
    }

    public void KickMode(CCSPlayerController controller)
    {
        foreach (var player in _workedPlayers)
        {
            if (controller == player)
            {
                return;
            }
        }
        if (!_bannedNames.Any(controller.PlayerName.Contains))
        {
            return;
        }
        _workedPlayers.Add(controller);
        controller.PrintToChat($" {PluginTagColor}{PluginTag} " + Localizer["main.bannedName"]);
        controller.PrintToChat($" {PluginTagColor}{PluginTag} " + Localizer["main.bannedNamesInConsole"]);
        int counter = KickTime;

        int timerIndex = _timers.Count;
        
        _timers.Add(AddTimer(1, () =>
        {
            controller.PrintToChat($" {PluginTagColor}{PluginTag} " + Localizer["main.beforeKickMessage"].ToString().FormatWith(new {seconds = counter}));
            PrintBannedNamesInConsole(controller);
            counter--;
            if (counter <= 0)
            {
                for (int i = 0; i < _workedPlayers.Count; i++)
                {
                    if (controller == _workedPlayers[i])
                    {
                        _workedPlayers.RemoveAt(i);
                    }
                }
                
                NativeAPI.IssueServerCommand($"kickid {controller.UserId}");
                _timers[timerIndex].Kill();
                _timers.RemoveAt(timerIndex);
            }
        }, TimerFlags.REPEAT));
    }

    public void RenameSiteMode(CCSPlayerController controller)
    {
        string[] wordsInPlayerName = controller.PlayerName.Split(" ");

        string newPlayerName = controller.PlayerName;

        foreach (var word in wordsInPlayerName)
        {
            bool wordContainsWhiteListWord = false;
            foreach (var whitelistWord in _whitelist)
            {
                wordContainsWhiteListWord = word.Contains(whitelistWord);
            }
            if (wordContainsWhiteListWord) continue;
            foreach (string domain in Domains)
            {
                if (word.Contains(domain))
                {
                    newPlayerName = newPlayerName.Replace(word, PluginSiteReplace);
                }
            }
            PlayerName<CBasePlayerController> _playerName = new PlayerName<CBasePlayerController>(controller, "m_iszPlayerName");
            _playerName.Set(newPlayerName);
        }
    }

    public void RenamePlayerOnRandomWord(CCSPlayerController controller)
    {
        if (_bannedNames.Any(controller.PlayerName.Contains))
        {
            Random rnd = new Random();
            PlayerName<CBasePlayerController> _playerName = new PlayerName<CBasePlayerController>(controller, "m_iszPlayerName");
            string randomWord = _replaceWords[rnd.Next(0, _replaceWords.Length)];
            _playerName.Set(randomWord);
            controller.PrintToChat($" {PluginTagColor}{PluginTag} " + Localizer["main.nameReplaceOnRandom"].ToString().FormatWith(new {name = randomWord}));
        }
    }

    public void RenameSiteAndKickMode(CCSPlayerController controller)
    {
        RenameSiteMode(controller);
        KickMode(controller);
    }
}