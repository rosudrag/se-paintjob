using System.Collections.Generic;
using ClientPlugin.PaintAlgorithms;
using Sandbox.ModAPI;
using VRage.Game;

namespace ClientPlugin
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
                    Style.Rudimentary, new RudimentaryPaint()
                }
            };
        }
        public void Run()
        {
            var targetGrid = GridUtilities.GetGridInFrontOfPlayer();

            if (targetGrid != null)
            {
                var currentStyle = _stateSystem.GetCurrentStyle();
                if (!_algorithms.ContainsKey(currentStyle))
                {
                    MyAPIGateway.Utilities.ShowNotification("No style found for painting.", 5000, MyFontEnum.Red);
                }

                _algorithms[currentStyle].Apply(targetGrid);

                MyAPIGateway.Utilities.ShowNotification("Grid painted with the current style.", 5000, MyFontEnum.Green);
            }
            else
            {
                MyAPIGateway.Utilities.ShowNotification("No grid found in front of the player.", 5000, MyFontEnum.Red);
            }
        }
    }

}