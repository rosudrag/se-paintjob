using System;
using System.Collections.Generic;
using System.Linq;
using PaintJob.App.Extensions;
using PaintJob.App.Models;
using PaintJob.App.PaintAlgorithms;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game;

namespace PaintJob.App
{
    public class PaintJob : IPaintJob
    {
        private readonly Dictionary<Style, PaintAlgorithm> _algorithms = new Dictionary<Style, PaintAlgorithm>
        {
            {
                Style.Rudimentary, new RudimentaryPaintJob()
            },
            {
                Style.Military, new MilitaryPaintJob()
            }
        };

        public void Run(string[] args = null)
        {
            Style style;
            string[] algorithmArgs = null;
            
            // Parse style from args if provided, otherwise use current style
            if (args != null && args.Length > 0)
            {
                if (Enum.TryParse(args[0], true, out style))
                {
                    // Style was provided in args, pass remaining args to algorithm
                    algorithmArgs = args.Length > 1 ? args.Skip(1).ToArray() : null;
                }
                else
                {
                    // First arg wasn't a valid style, show error
                    MyAPIGateway.Utilities.ShowNotification($"Unknown style '{args[0]}'. Use /paint help for options.", 5000, MyFontEnum.Red);
                    return;
                }
            }
            else
            {
                // No args provided, default to rudimentary
                style = Style.Rudimentary;
            }
            
            if (!_algorithms.TryGetValue(style, out var paintAlgorithm))
            {
                MyAPIGateway.Utilities.ShowNotification($"Style '{style}' not found.", 5000, MyFontEnum.Red);
                return;
            }

            try
            {
                var targetGrid = GridUtilities.GetGridInFrontOfPlayer();

                if (targetGrid != null)
                {
                    // Configure algorithm with args if it's military
                    if (paintAlgorithm is MilitaryPaintJob militaryPaintJob && algorithmArgs != null && algorithmArgs.Length > 0)
                    {
                        militaryPaintJob.SetVariant(algorithmArgs[0]);
                    }
                    
                    paintAlgorithm.Run(targetGrid as MyCubeGrid);

                    var notification = style == Style.Military && algorithmArgs != null && algorithmArgs.Length > 0
                        ? $"Grid painted with {style} style (variant: {algorithmArgs[0]})."
                        : $"Grid painted with {style} style.";
                    MyAPIGateway.Utilities.ShowNotification(notification, 5000, MyFontEnum.Green);
                }
                else
                {
                    MyAPIGateway.Utilities.ShowNotification("No grid found in front of the player.", 5000, MyFontEnum.Red);
                }
            }
            catch (Exception e)
            {
                MyAPIGateway.Utilities.ShowMessage("Logger", $"{e.Message}");
            }
            finally
            {
                paintAlgorithm.Clean();
            }

        }
    }

}