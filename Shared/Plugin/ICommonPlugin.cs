using ClientPlugin.Shared.Config;
using ClientPlugin.Shared.Logging;

namespace ClientPlugin.Shared.Plugin
{
    public interface ICommonPlugin
    {
        IPluginLogger Log { get; }
        IPluginConfig Config { get; }
        long Tick { get; }
    }
}