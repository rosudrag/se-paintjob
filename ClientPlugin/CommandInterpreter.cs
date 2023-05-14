using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI;
using VRage.Game;

namespace ClientPlugin
{
    public class CommandInterpreter
    {
        public static readonly CommandInterpreter Instance = new CommandInterpreter();
        private readonly Dictionary<string, Action<string[]>> _commands;
        private readonly PaintJobHelpSystem _helpSystem = PaintJobHelpSystem.Instance;
        private readonly PaintJob _paintJob = PaintJob.Instance;
        private readonly PaintJobStateSystem _stateSystem = PaintJobStateSystem.Instance;

        private CommandInterpreter()
        {
            _commands = new Dictionary<string, Action<string[]>>
            {
                {
                    "help", args => _helpSystem.DisplayHelp()
                },
                {
                    "?", args => _helpSystem.DisplayHelp()
                },
                {
                    "add", args => _stateSystem.AddColor(args)
                },
                {
                    "list", args => _stateSystem.ListColors()
                },
                {
                    "remove", args => _stateSystem.RemoveColor(args)
                },
                {
                    "execute", args => _paintJob.Run()
                },
                {
                    "exec", args => _paintJob.Run()
                },
                {
                    "run", args => _paintJob.Run()
                },
                {
                    "style", args => _stateSystem.SetStyle(Enum.TryParse<Style>(args[0], out var style) ? style : Style.Rudimentary )
                }
            };
        }

        public void Interpret(string[] args)
        {
            var command = args.Length > 0 ? args[0].ToLower() : "help";
            var remainingArgs = args.Skip(1).ToArray();

            if (_commands.TryGetValue(command, out var action))
            {
                action(remainingArgs);
            }
            else
            {
                MyAPIGateway.Utilities.ShowNotification("Invalid command. Type '/paint help' for a list of commands.", 5000, MyFontEnum.Red);
            }
        }
    }
}