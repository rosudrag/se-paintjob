using System.Collections.Generic;

namespace PaintJob.App.Systems
{
    public interface IPaintJobHelpSystem
    {
        void DisplayHelp();
        void WithColors(IEnumerable<string> colors);
    }
}