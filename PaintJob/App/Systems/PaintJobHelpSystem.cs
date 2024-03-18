using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PaintJob.App.Models;
using Sandbox.ModAPI;

namespace PaintJob.App.Systems
{
    public class PaintJobHelpSystem : IPaintJobHelpSystem
    {
        public void DisplayHelp()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Grid painter plugin");
            sb.AppendLine("--- --- --- --- --- ---");
            sb.AppendLine("This plugin allows you to paint a grid with a palette of colors.");
            sb.AppendLine();
            sb.AppendLine("The plugin will use all the colors in your palette, and will use");
            sb.AppendLine("an algorithm to distribute the colors across the grid.");
            sb.AppendLine();
            sb.AppendLine("Value commands:");
            sb.AppendLine();
            sb.AppendLine("/paint help      --- commands");
            sb.AppendLine("/paint run       --- run paint job, point towards a grid");
            sb.AppendLine("--- --- --- ---");


            // Show a popup with help text
            MyAPIGateway.Utilities.ShowMissionScreen("Paint Job Help", "", "", sb.ToString(), null, "Close");
        }
    }
}