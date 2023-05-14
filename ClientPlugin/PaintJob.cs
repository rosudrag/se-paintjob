using System.Reflection;
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
        public void Initialize()
        {
            if (!MySession.Static.ChatSystem.CommandSystem.ChatCommands.ContainsKey("/paint"))
            {
                MySession.Static.ChatSystem.CommandSystem.ScanAssemblyForCommands(Assembly.GetExecutingAssembly());
                MyLog.Default.Info("Chewy's paint plugin. Chat command added: /paint");
            }
        }

        [ChatCommand("/paint", "paints grids", "")]
        public void PaintCommand(string[] args)
        {
            _interpreter.Interpret(args);
        }

        public void Run()
        {
            var targetGrid = GridUtilities.GetGridInFrontOfPlayer();

            if (targetGrid != null)
            {
                var paintAlgorithm = new PaintAlgorithm();
                paintAlgorithm.ApplyRudimentary(targetGrid);

                MyAPIGateway.Utilities.ShowNotification("Grid painted with the first color in the state.", 5000, MyFontEnum.Green);
            }
            else
            {
                MyAPIGateway.Utilities.ShowNotification("No grid found in front of the player.", 5000, MyFontEnum.Red);
            }
        }
    }
}