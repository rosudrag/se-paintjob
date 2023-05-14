using System.Collections.Generic;
using VRageMath;

namespace ClientPlugin
{
    public interface IPaintJobStateSystem
    {
        void ListColors();
        void AddColor(string[] args);
        void RemoveColor(string[] args);
        IEnumerable<Color> GetColors();
        void SetStyle(Style style);
        Style GetCurrentStyle();
    }
}