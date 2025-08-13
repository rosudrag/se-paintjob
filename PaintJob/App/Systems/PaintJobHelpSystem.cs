using System.Text;
using Sandbox.ModAPI;
using PaintJob.App.Constants;

namespace PaintJob.App.Systems
{
    public class PaintJobHelpSystem : IPaintJobHelpSystem
    {
        public void DisplayHelp()
        {
            var sb = new StringBuilder();
            sb.AppendLine(PaintJobConstants.HELP_HEADER);
            sb.AppendLine();
            sb.AppendLine("Apply creative paint jobs to your grids!");
            sb.AppendLine();
            sb.AppendLine("COMMANDS:");
            sb.AppendLine("  /paint        - Open the Paint Designer GUI");
            sb.AppendLine("  /paint help   - Show this help");
            sb.AppendLine();
            sb.AppendLine("AVAILABLE STYLES:");
            sb.AppendLine("  • Rudimentary - Classic colorful paint job");
            sb.AppendLine("  • Military    - Tactical camouflage (5 variants)");
            sb.AppendLine("  • Racing      - High-speed liveries (5 variants)");
            sb.AppendLine("  • Pirate      - Weathered battle-worn (5 variants)");
            sb.AppendLine("  • Corporate   - Professional branding (5 variants)");
            sb.AppendLine("  • Alien       - Bio-luminescent organic (5 variants)");
            sb.AppendLine("  • Retro       - Era-specific designs (5 variants)");
            sb.AppendLine();
            sb.AppendLine("The GUI allows you to:");
            sb.AppendLine("  • Select any grid within 500m that you own");
            sb.AppendLine("  • Choose from 7 paint styles with 30+ variants");
            sb.AppendLine("  • Preview and apply paint jobs instantly");
            sb.AppendLine();
            sb.AppendLine("Each grid gets unique colors based on its ID!");

            MyAPIGateway.Utilities.ShowMissionScreen(PaintJobConstants.HELP_TITLE, "", "", sb.ToString(), null, "Close");
        }
    }
}