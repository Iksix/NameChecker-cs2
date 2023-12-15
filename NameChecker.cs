using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;

namespace NameChecker;

public class NameChecker : BasePlugin
{
    public override string ModuleName => "Name Checker";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "iks";
    private List<string> names = new List<string>();

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
            names.Add(line);
        }
        
        Console.WriteLine("[NameChecker] Forbidden names:");
        foreach (string name in names)
        {
            Console.WriteLine(name);
        }
        reader.Close();
    }

    [ConsoleCommand("css_namechecker_reload")]
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
        
        string playerName = @event.Userid.PlayerName;
        if (names.Any(playerName.Contains))
        {
            NativeAPI.IssueServerCommand($"kickid {@event.Userid.UserId} Your nickname is banned! Change it!");
            Console.WriteLine($"[NameChecker] Player with name < {@event.Userid.PlayerName} > was kicked");
        }
        
        
        return HookResult.Continue;
    }
}
