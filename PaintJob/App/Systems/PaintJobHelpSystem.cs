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
            sb.AppendLine("Point at a grid and paint it with creative styles!");
            sb.AppendLine();
            sb.AppendLine("BASIC COMMANDS:");
            sb.AppendLine();
            sb.AppendLine("/paint                          Open Paint Designer GUI");
            sb.AppendLine("/paint help                     Show this help");
            sb.AppendLine("/paint run                      Paint with rudimentary style");
            sb.AppendLine("/paint run rudimentary          Classic colorful paint job");
            sb.AppendLine();
            sb.AppendLine("MILITARY STYLES:");
            sb.AppendLine("/paint run military             Military green-grey camouflage");
            sb.AppendLine("/paint run military stealth     Ultra-dark for stealth ops");
            sb.AppendLine("/paint run military asteroid    Orange-brown asteroid camo");
            sb.AppendLine("/paint run military industrial  Yellow-grey with hazard marks");
            sb.AppendLine("/paint run military deep_space  Dark blue-purple for space");
            sb.AppendLine();
            sb.AppendLine("RACING STYLES:");
            sb.AppendLine("/paint run racing               Formula 1 racing livery");
            sb.AppendLine("/paint run racing formula1      Red/white/black F1 style");
            sb.AppendLine("/paint run racing rally         Blue/yellow rally colors");
            sb.AppendLine("/paint run racing street        Orange/purple street racing");
            sb.AppendLine("/paint run racing drag          Green/pink drag racing");
            sb.AppendLine("/paint run racing endurance     Teal/orange endurance racing");
            sb.AppendLine();
            sb.AppendLine("PIRATE STYLES:");
            sb.AppendLine("/paint run pirate               Weathered pirate ship");
            sb.AppendLine("/paint run pirate skull         Classic skull & crossbones");
            sb.AppendLine("/paint run pirate kraken        Deep sea kraken theme");
            sb.AppendLine("/paint run pirate blackbeard    Dark and menacing");
            sb.AppendLine("/paint run pirate corsair       Purple corsair colors");
            sb.AppendLine("/paint run pirate marauder      Orange-brown marauder");
            sb.AppendLine();
            sb.AppendLine("CORPORATE STYLES:");
            sb.AppendLine("/paint run corporate            Professional mining corp");
            sb.AppendLine("/paint run corporate mining_corp     Yellow/black mining");
            sb.AppendLine("/paint run corporate transport_co    Blue/white transport");
            sb.AppendLine("/paint run corporate security_firm   Red/black security");
            sb.AppendLine("/paint run corporate research_lab    Purple/blue research");
            sb.AppendLine("/paint run corporate construction    Orange construction");
            sb.AppendLine();
            sb.AppendLine("ALIEN STYLES:");
            sb.AppendLine("/paint run alien                Organic bio-luminescent");
            sb.AppendLine("/paint run alien organic        Green organic tissue");
            sb.AppendLine("/paint run alien crystalline    Purple crystal formation");
            sb.AppendLine("/paint run alien techno_organic Cyan tech-organic hybrid");
            sb.AppendLine("/paint run alien hive           Yellow-brown hive mind");
            sb.AppendLine("/paint run alien ancient        Teal ancient technology");
            sb.AppendLine();
            sb.AppendLine("RETRO STYLES:");
            sb.AppendLine("/paint run retro                80s neon aesthetics");
            sb.AppendLine("/paint run retro 80s_neon       Dark with bright neon");
            sb.AppendLine("/paint run retro 50s_chrome     Chrome and pastels");
            sb.AppendLine("/paint run retro 70s_disco      Psychedelic earth tones");
            sb.AppendLine("/paint run retro 90s_cyber      Matrix green on black");
            sb.AppendLine("/paint run retro art_deco       Gold and black geometric");
            sb.AppendLine();
            sb.AppendLine("Each ship gets unique colors based on its ID.");
            sb.AppendLine("All styles intelligently analyze your ship's structure!");


            // Show a popup with help text
            MyAPIGateway.Utilities.ShowMissionScreen("Paint Job Help", "", "", sb.ToString(), null, "Close");
        }
    }
}