using System.Runtime.CompilerServices;
using System.Text;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Memory;
namespace NameChecker;

public class PlayerName<SchemaClass> : NativeObject where SchemaClass : NativeObject
{
    public PlayerName(SchemaClass instance, string member) : base(Schema.GetSchemaValue<nint>(instance.Handle, typeof(SchemaClass).Name!, member))
    { }

    public unsafe void Set(string str)
    {
        byte[] bytes = this.GetStringBytes(str);
        foreach (var b in bytes)
        {
            Server.PrintToChatAll(b.ToString());
        }

        for (int i = 0; i < bytes.Length; i++)
        {
            Unsafe.Write((void*)(this.Handle.ToInt64() + i), bytes[i]);
        }

        Unsafe.Write((void*)(this.Handle.ToInt64() + bytes.Length), 0);
    }

    private byte[] GetStringBytes(string str)
    {
        return Encoding.UTF8.GetBytes(str);
    }
}