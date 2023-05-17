using PaintJob.Shared.Config;
using PaintJob.Shared.Logging;

namespace PaintJob.Shared.Plugin
{
    public interface ICommonPlugin
    {
        IPluginLogger Log { get; }
        IPluginConfig Config { get; }
        long Tick { get; }
    }
}