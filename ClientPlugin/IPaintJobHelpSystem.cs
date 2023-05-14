using System.Collections.Generic;

namespace ClientPlugin
{
    public interface IPaintJobHelpSystem
    {
        void DisplayHelp();
        void ShowValidValues(string title, IEnumerable<string> values);
    }
}