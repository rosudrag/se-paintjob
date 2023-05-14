using VRage.Game.ModAPI;

namespace ClientPlugin.PaintAlgorithms
{
    public abstract class PaintAlgorithm
    {
        public abstract void Apply(IMyCubeGrid grid);
    }
}