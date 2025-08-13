using System;
using System.Collections.Generic;
using System.Linq;
using PaintJob.App.Models;
using PaintJob.App.Constants;
using PaintJob.GUI;
using Sandbox.ModAPI;
using VRage.Game;

namespace PaintJob.App.Systems
{
    public class CommandInterpreter : ICommandInterpreter
    {
        private readonly Dictionary<string, Action<string[]>> _commands;

        public CommandInterpreter(IPaintJobHelpSystem helpSystem, IPaintJob paintJob)
        {
            _commands = new Dictionary<string, Action<string[]>>
            {
                {
                    PaintJobConstants.COMMAND_HELP, args => helpSystem.DisplayHelp()
                },
                {
                    "", args => OpenPaintGui(paintJob)
                }
            };
        }

        public void Interpret(string[] args)
        {
            if (args.First().ToLower() != PaintJobConstants.COMMAND_PREFIX)
                return;

            args = args.Skip(1).ToArray();

            var command = args.Length > 0 ? args[0].ToLower() : "";
            var remainingArgs = args.Skip(1).ToArray();

            if (_commands.TryGetValue(command, out var action))
            {
                action(remainingArgs);
            }
            else
            {
                MyAPIGateway.Utilities.ShowNotification(PaintJobConstants.NOTIFICATION_INVALID_COMMAND, 5000, MyFontEnum.Red);
            }
        }

        
        private void OpenPaintGui(IPaintJob paintJob)
        {
            Sandbox.MySandboxGame.Static.Invoke(() => 
            {
                Sandbox.Graphics.GUI.MyGuiSandbox.AddScreen(new GUI.PaintJobGui(paintJob));
            }, "PaintJobGui");
        }
    }
}