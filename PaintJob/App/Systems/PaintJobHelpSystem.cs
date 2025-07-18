using System.Text;
using Sandbox.ModAPI;

namespace PaintJob.App.Systems
{
    public class PaintJobHelpSystem : IPaintJobHelpSystem
    {
        public void DisplayHelp()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== GRID PAINTER ===");
            sb.AppendLine();
            sb.AppendLine("Point at a grid and paint it!");
            sb.AppendLine();
            sb.AppendLine("COMMANDS:");
            sb.AppendLine();
            sb.AppendLine("/paint help                     Show this help");
            sb.AppendLine("/paint run                      Paint with rudimentary style");
            sb.AppendLine("/paint run rudimentary          Classic colorful paint job");
            sb.AppendLine("/paint run military             Military green-grey camouflage");
            sb.AppendLine("/paint run military stealth     Ultra-dark for stealth ops");
            sb.AppendLine("/paint run military asteroid    Orange-brown asteroid camo");
            sb.AppendLine("/paint run military industrial  Yellow-grey with hazard marks");
            sb.AppendLine("/paint run military deep_space  Dark blue-purple for space");
            sb.AppendLine();
            sb.AppendLine("Each ship gets unique colors based on its ID.");


            // Show a popup with help text
            MyAPIGateway.Utilities.ShowMissionScreen("Paint Job Help", "", "", sb.ToString(), null, "Close");
        }
    }
}