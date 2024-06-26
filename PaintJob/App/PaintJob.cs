﻿using System;
using System.Collections.Generic;
using PaintJob.App.Extensions;
using PaintJob.App.Models;
using PaintJob.App.PaintAlgorithms;
using PaintJob.App.Systems;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game;

namespace PaintJob.App
{
    public class PaintJob : IPaintJob
    {
        private readonly Dictionary<Style, PaintAlgorithm> _algorithms;
        private readonly IPaintJobStateSystem _stateSystem;

        public PaintJob(IPaintJobStateSystem stateSystem)
        {
            _stateSystem = stateSystem;
            _algorithms = new Dictionary<Style, PaintAlgorithm>
            {
                {
                    Style.Rudimentary, new RudimentaryPaintJob()
                }
            };
        }
        public void Run()
        {
            var currentStyle = _stateSystem.GetCurrentStyle();
            if (!_algorithms.ContainsKey(currentStyle))
            {
                MyAPIGateway.Utilities.ShowNotification("No style found for painting.", 5000, MyFontEnum.Red);
                return;
            }
            
            var paintAlgorithm = _algorithms[currentStyle];

            try
            {
                var targetGrid = GridUtilities.GetGridInFrontOfPlayer();

                if (targetGrid != null)
                {
                    paintAlgorithm.Run(targetGrid as MyCubeGrid);

                    MyAPIGateway.Utilities.ShowNotification("Grid painted with the current style.", 5000, MyFontEnum.Green);
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
        public void RunTest(string[] args)
        {
            var currentStyle = _stateSystem.GetCurrentStyle();
            if (!_algorithms.ContainsKey(currentStyle))
            {
                MyAPIGateway.Utilities.ShowNotification("No style found for painting.", 5000, MyFontEnum.Red);
                return;
            }
            
            var paintAlgorithm = _algorithms[currentStyle];

            try
            {
                var targetGrid = GridUtilities.GetGridInFrontOfPlayer();

                if (targetGrid != null)
                {
                    paintAlgorithm.RunTest(targetGrid as MyCubeGrid, args);

                    MyAPIGateway.Utilities.ShowNotification("Test OK!", 5000, MyFontEnum.Green);
                }
                else
                {
                    MyAPIGateway.Utilities.ShowNotification("No grid found in front of the player.", 5000, MyFontEnum.Red);
                }
            }
            catch (Exception e)
            {
                MyAPIGateway.Utilities.ShowMessage("Logger", $"Test BAD: {e.Message}");
            }
            finally
            {
                paintAlgorithm.Clean();
            }
        }
    }

}