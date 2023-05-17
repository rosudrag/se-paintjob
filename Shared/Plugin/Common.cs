using PaintJob.Shared.Config;
using PaintJob.Shared.Logging;

namespace PaintJob.Shared.Plugin
{
    public static class Common
    {
        public static ICommonPlugin Plugin { get; private set; }
        public static IPluginLogger Logger { get; private set; }
        public static IPluginConfig Config { get; private set; }


        public static void SetPlugin(ICommonPlugin plugin)
        {
            Plugin = plugin;
            Logger = plugin.Log;
            Config = plugin.Config;
        }
    }
}