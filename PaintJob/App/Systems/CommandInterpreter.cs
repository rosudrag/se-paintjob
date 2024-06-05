using System;
using System.Collections.Generic;
using System.Linq;
using PaintJob.App.Models;
using Sandbox.ModAPI;
using VRage.Game;

namespace PaintJob.App.Systems
{
    public class CommandInterpreter : ICommandInterpreter
    {
        private readonly Dictionary<string, Action<string[]>> _commands;

        public CommandInterpreter(IPaintJobHelpSystem helpSystem, IPaintJobStateSystem stateSystem, IPaintJob paintJob)
        {
            _commands = new Dictionary<string, Action<string[]>>
            {
                {
                    "help", args => helpSystem.DisplayHelp()
                },
                {
                    "?", args => helpSystem.DisplayHelp()
                },
                {
                    "state", args => stateSystem.ShowState()
                },
                {
                    "run", args => paintJob.Run()
                },
                {
                  "test", paintJob.RunTest  
                },
                {
                    "style", args => stateSystem.SetStyle(Enum.TryParse<Style>(args[0], out var style) ? style : Style.Rudimentary)
                },
                {
                    "save", _ => stateSystem.Save()
                },
                {
                    "load", _ => stateSystem.Load()
                },
                {
                    "reset", _ => stateSystem.Reset()
                }
            };
        }

        public void Interpret(string[] args)
        {
            if (args.First().ToLower() != "/paint")
                return;

            args = args.Skip(1).ToArray();

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