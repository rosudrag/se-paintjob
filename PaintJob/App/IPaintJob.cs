namespace PaintJob.App
{
    public interface IPaintJob
    {
        void Run(string[] args = null);
        void Run(string[] args, Sandbox.Game.Entities.MyCubeGrid targetGrid);
    }
}