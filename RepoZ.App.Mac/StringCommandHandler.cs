using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace RepoZ.App.Mac
{
    public class StringCommandHandler
    {
        private Dictionary<string, Action> _commands = new Dictionary<string, Action>();
        private StringBuilder _helpBuilder = new StringBuilder();

        internal bool IsCommand(string value)
        {
            return value?.StartsWith(":") == true;
        }

        internal bool Handle(string command)
        {
            if (_commands.TryGetValue(CleanCommand(command), out Action commandAction))
            {
                commandAction.Invoke();
                return true;
            }

            return false;
        }

        internal void Define(string[] commands, Action commandAction, string helpText)
        {
            foreach (var command in commands)
                _commands[CleanCommand(command)] = commandAction;

            if (_helpBuilder.Length == 0)
            {
                _helpBuilder.AppendLine("To execute a command instead of filtering the list of repositories, simply begin with a colon (:).");
                _helpBuilder.AppendLine("");
                _helpBuilder.AppendLine("Command reference:");
            }

            _helpBuilder.AppendLine("");

            _helpBuilder.AppendLine("\t:" + string.Join(" or :", commands.OrderBy(c => c)));
            _helpBuilder.AppendLine("\t\t"+ helpText);
        }

        private string CleanCommand(string command)
        {
            command = command?.Trim().ToLower() ?? "";
            return command.StartsWith(":", StringComparison.OrdinalIgnoreCase) ? command.Substring(1) : command;
        }

        internal string GetHelpText() => _helpBuilder.ToString();
    }
}