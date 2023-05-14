using System.Collections.Generic;
using System.Reflection;
using ClientPlugin.PaintAlgorithms;
using Sandbox.Game.GameSystems.Chat;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Utils;

namespace ClientPlugin
{
    public class PaintJob
    {
        public static readonly PaintJob Instance = new PaintJob();
        private readonly CommandInterpreter _interpreter = CommandInterpreter.Instance;
        private readonly PaintJobStateSystem _state = PaintJobStateSystem.Instance;
        private readonly Dictionary<Style, PaintAlgorithm> _algorithms;

        private PaintJob()
        {
            _algorithms = new Dictionary<Style, PaintAlgorithm>
            {
                { Style.Rudimentary, RudimentaryPaint.Instance }
            };
        }

        public void Initialize()
        {
            if (!MySession.Static.ChatSystem.CommandSystem.ChatCommands.ContainsKey("/paint"))
            {
                MySession.Static.ChatSystem.CommandSystem.ScanAssemblyForCommands(Assembly.GetExecutingAssembly());
                MyLog.Default.Info("Chewy's paint plugin. Chat command added: /paint");
            }
        }

        [ChatCommand("/paint", "paints grids", "")]
        // ReSharper disable once UnusedMember.Global
        public void PaintCommand(string[] args)
        {
            _interpreter.Interpret(args);
        }

        public void Run()
        {
            var targetGrid = GridUtilities.GetGridInFrontOfPlayer();

            if (targetGrid != null)
            {
                var currentStyle = _state.GetCurrentStyle();
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