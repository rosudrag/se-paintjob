using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.ModAPI;

namespace ClientPlugin
{
    public class PaintJobHelpSystem : IPaintJobHelpSystem
    {
        public void DisplayHelp()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Grid painter plugin");
            sb.AppendLine("--- --- --- --- --- ---");
            sb.AppendLine(); 
            sb.AppendLine();
            sb.AppendLine("Value commands:");
            sb.AppendLine();
            sb.AppendLine("/paint help      --- commands");
            sb.AppendLine("/paint ?         --- commands");
            sb.AppendLine("/paint add       --- add color to palette");
            sb.AppendLine("/paint list      --- list current palette");
            sb.AppendLine("/paint remove    --- remove color from palette");
            sb.AppendLine("/paint execute   --- run paint job, point towards a grid");
            sb.AppendLine("/paint exec      --- run paint job, point towards a grid");
            sb.AppendLine("/paint run       --- run paint job, point towards a grid");
            sb.AppendLine("/paint style     --- select style of painting");
            sb.AppendLine("--- --- --- ---");
                       
            // Show a popup with help text
            MyAPIGateway.Utilities.ShowMissionScreen("Paint Job Help", "", "", sb.ToString(), null, "Close");
        }
        public void ShowValidValues(string title, IEnumerable<string> values)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Valid values:");
            var valuesArray = values.ToArray();
            for (var i = 0; i < valuesArray.Length; i++)
            {
                sb.AppendLine($"{i}.{valuesArray[i]}");
            }
            MyAPIGateway.Utilities.ShowMissionScreen(title, "", "", sb.ToString(), null, "Close");
        }
    }
}