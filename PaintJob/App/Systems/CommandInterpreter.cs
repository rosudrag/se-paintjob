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

        public CommandInterpreter(IPaintJobHelpSystem helpSystem, IPaintJob paintJob)
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
                    "run", args => paintJob.Run(args)
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