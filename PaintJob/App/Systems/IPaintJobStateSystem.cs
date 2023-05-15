using System.Collections.Generic;
using ClientPlugin.App.Models;
using VRageMath;

namespace ClientPlugin.App
{
    public interface IPaintJobStateSystem
    {
        void ShowState();
        void AddColor(string[] args);
        void RemoveColor(string[] args);
        IEnumerable<Color> GetColors();
        void SetStyle(Style style);
        Style GetCurrentStyle();
        void Load();
        void Save();
        void Reset();
    }
}