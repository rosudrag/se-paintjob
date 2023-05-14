using System.Collections.Generic;

namespace ClientPlugin
{
    public interface IPaintJobHelpSystem
    {
        void DisplayHelp();
        void WithColors(IEnumerable<string> colors);
    }
}