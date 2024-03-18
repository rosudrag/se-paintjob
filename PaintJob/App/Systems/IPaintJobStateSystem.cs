using PaintJob.App.Models;

namespace PaintJob.App.Systems
{
    public interface IPaintJobStateSystem
    {
        void ShowState();
        void SetStyle(Style style);
        Style GetCurrentStyle();
        void Load();
        void Save();
        void Reset();
    }
}