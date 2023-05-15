using System.Collections.Generic;

namespace ClientPlugin.App
{
    public interface IPaintJobHelpSystem
    {
        void DisplayHelp();
        void WithColors(IEnumerable<string> colors);
    }
}