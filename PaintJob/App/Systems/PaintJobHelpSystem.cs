using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientPlugin.App.Models;
using Sandbox.ModAPI;

namespace ClientPlugin.App
{
    public class PaintJobHelpSystem : IPaintJobHelpSystem
    {
        private string _colorsAsList;
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
            sb.AppendLine("/paint remove    --- remove color from palette");
            sb.AppendLine("/paint state     --- display current state");
            sb.AppendLine("/paint execute   --- run paint job, point towards a grid");
            sb.AppendLine("/paint exec      --- run paint job, point towards a grid");
            sb.AppendLine("/paint run       --- run paint job, point towards a grid");
            sb.AppendLine("/paint style     --- select style of painting");
            sb.AppendLine("/paint save      --- save state");
            sb.AppendLine("/paint load      --- load state");
            sb.AppendLine("--- --- --- ---");
            sb.AppendLine("Valid styles:");
            foreach (var style in Enum.GetNames(typeof(Style)))
            {
                sb.AppendLine($"* {style}");
            }
            sb.AppendLine("--- --- --- ---");
            sb.AppendLine();
            sb.AppendLine("Valid colors:");
            sb.AppendLine(_colorsAsList);
            sb.AppendLine("--- --- --- ---");


            // Show a popup with help text
            MyAPIGateway.Utilities.ShowMissionScreen("Paint Job Help", "", "", sb.ToString(), null, "Close");
        }
        public void WithColors(IEnumerable<string> colors)
        {
            var sb = new StringBuilder();
            var valuesArray = colors.ToArray();
            for (var i = 0; i < valuesArray.Length; i++)
            {
                sb.AppendLine($"{i}.{valuesArray[i]}");
            }
            _colorsAsList = sb.ToString();
        }
    }
}