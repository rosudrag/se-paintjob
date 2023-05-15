using System.Collections.Generic;
using PaintJob.App.Models;
using VRageMath;

namespace PaintJob.App.Systems
{
    public interface IPaintJobStateSystem
    {
        void ShowState();
        void AddColor(string[] args);
        void RemoveColor(string[] args);
        Color[] GetColors();
        void SetStyle(Style style);
        Style GetCurrentStyle();
        void Load();
        void Save();
        void Reset();
    }
}