using Sandbox.ModAPI;

namespace ClientPlugin
{
    public class PaintJobHelpSystem
    {
        public static readonly PaintJobHelpSystem Instance = new PaintJobHelpSystem();
        public void DisplayHelp()
        {
            // Show a popup with help text
            MyAPIGateway.Utilities.ShowMissionScreen("Paint Job Help", "", "", "TODO: Add help text here", null, "Close");
        }
    }
}