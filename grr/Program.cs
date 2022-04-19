namespace grr
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using CommandLine;
    using grr.Messages;
    using grr.Messages.Filters;
    using RepoZ.Ipc;

    static class Program
    {
        private const int MAX_REPO_NAME_LENGTH = 35;

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            args = PrepareArguments(args);

            if (IsHelpRequested(args))
            {
                ShowHelp();
            }
            else
            {
                IMessage message = TryParseArgumentsToMessage(args);

                if (message != null)
                {
                    IpcClient.Result result = null;

                    if (message.HasRemoteCommand)
                    {
                        var client = new IpcClient(new DefaultIpcEndpoint());
                        result = client.GetRepositories(message.GetRemoteCommand());

                        if (result.Repositories?.Length > 0)
                        {
                            if (message.ShouldWriteRepositories(result.Repositories))
                            {
                                WriteRepositories(result.Repositories);
                            }
                        }
                        else
                        {
                            Console.WriteLine(result.Answer);
                        }
                    }

                    message?.Execute(result?.Repositories);

                    WriteHistory(result?.Repositories);
                }
                else
                {
                    Console.WriteLine("Could not parse command line arguments.");
                }
            }

            if (Debugger.IsAttached)
            {
                Console.ReadKey();
            }
        }

        private static IMessage TryParseArgumentsToMessage(string[] args)
        {
            try
            {
                ParserResult<object> parseResult = CommandLine.Parser.Default.ParseArguments(args, typeof(ListOptions), typeof(ChangeDirectoryOptions), typeof(GetDirectoryOptions), typeof(OpenDirectoryOptions));

                if (parseResult.Tag == CommandLine.ParserResultType.NotParsed)
                {
                    return null;
                }

                var options = parseResult.GetType().GetProperty("Value").GetValue(parseResult) as RepositoryFilterOptions;

                // yes, that's a hack. I feel not good about it. The CommandLineParser seems not to be able to parse "cd -" since version 2.3.0 anymore
                // and here we are, hacking our way around it ...
                if (options != null)
                {
                    if (options.RepositoryFilter == null
                        && args.Length == 2
                        && ("cd".Equals(args[0], StringComparison.OrdinalIgnoreCase) || "gd".Equals(args[0], StringComparison.OrdinalIgnoreCase))
                        && args[1] == "-")
                    {
                        options.RepositoryFilter = "-";
                    }
                }

                return GetMessage(options);
            }
            catch
            {
                return null;
            }
        }

        private static void WriteHistory(Repository[] repositories)
        {
            var history = new History.State()
                {
                    LastLocation = FindCallerWorkingDirectory(),
                    LastRepositories = repositories,
                    OverwriteRepositories = (repositories?.Length > 1) /* 0 or 1 repo should not overwrite the last list */

                    // OverwriteRepositories = false?!
                    // if multiple repositories were found the last time we ran grr,
                    // these were written to the last state.
                    // if the user selects one with an index like "grr cd :2", we want
                    // to keep the last repositories to enable him to choose another one
                    // with the same indexes as before.
                    // so we have to get the old repositories - load and copy them if required
                };

            var repository = new History.FileHistoryRepository();
            repository.Save(history);
        }

        private static string FindCallerWorkingDirectory()
        {
            // do NOT use the directory of the grr-assembly
            // we need to preserve the context of the calling console
            return System.IO.Directory.GetCurrentDirectory();
        }

        private static void WriteRepositories(Repository[] repositories)
        {
            var maxRepoNameLength = Math.Min(MAX_REPO_NAME_LENGTH, repositories.Max(r => r.Name?.Length ?? 0));
            var maxIndexStringLength = repositories.Length.ToString().Length;
            var ellipsesSign = "\u2026";
            var writeIndex = repositories.Length > 1;

            for (var i = 0; i < repositories.Length; i++)
            {
                var userIndex = i + 1; // the index visible to the user are 1-based, not 0-based;

                var repoName = (repositories[i].Name.Length > MAX_REPO_NAME_LENGTH)
                    ? repositories[i].Name.Substring(0, MAX_REPO_NAME_LENGTH) + ellipsesSign
                    : repositories[i].Name;

                Console.Write(" ");
                if (writeIndex)
                {
                    Console.Write($" [{userIndex.ToString().PadLeft(maxIndexStringLength)}]  ");
                }

                Console.Write(repoName.PadRight(maxRepoNameLength + 3));
                Console.Write(repositories[i].BranchWithStatus);
                Console.WriteLine();
            }
        }

        private static string[] PrepareArguments(string[] args)
        {
            if (args?.Length == 0)
            {
                args = new string[] { CommandLineOptions.LIST_COMMAND, };
            }

            if (!CommandLineOptions.IsKnownArgument(args.First()))
            {
                var newArgs = new List<string>(args);
                newArgs.Insert(0, CommandLineOptions.LIST_COMMAND);
                args = newArgs.ToArray();
            }

            return args;
        }

        private static IMessage GetMessage(RepositoryFilterOptions options)
        {
            // default should be listing all repositories
            IMessage message = new ListRepositoriesMessage();

            ApplyMessageFilters(options);

            if (options is ListOptions)
            {
                if (options.HasFileFilter)
                {
                    message = new ListRepositoryFilesMessage(options);
                }
                else
                {
                    message = new ListRepositoriesMessage(options);
                }
            }

            if (options is ChangeDirectoryOptions)
            {
                message = new ChangeToDirectoryMessage(options);
            }

            if (options is GetDirectoryOptions)
            {
                message = new GetDirectoryMessage(options);
            }

            if (options is OpenDirectoryOptions)
            {
                if (options.HasFileFilter)
                {
                    message = new OpenFileMessage(options);
                }
                else
                {
                    message = new OpenDirectoryMessage(options);
                }
            }

            return message;
        }

        private static void ApplyMessageFilters(RepositoryFilterOptions filter)
        {
            var historyRepository = new History.FileHistoryRepository();
            var filters = new IMessageFilter[]
                {
                    new IndexMessageFilter(historyRepository),
                    new GoBackMessageFilter(historyRepository),
                };

            foreach (IMessageFilter messageFilter in filters)
            {
                messageFilter.Filter(filter);
            }
        }

        private static bool IsHelpRequested(string[] args)
        {
            if (args.Length != 1)
            {
                return false;
            }

            var arg = args[0].TrimStart('-').TrimStart('/');

            return CommandLineOptions.HELP_COMMAND.Equals(arg, StringComparison.OrdinalIgnoreCase)
                   ||
                   CommandLineOptions.HELP_COMMAND_CHAR.ToString().Equals(arg, StringComparison.OrdinalIgnoreCase);
        }

        private static void ShowHelp()
        {
            Console.WriteLine(CommandLineOptions.GetUsage());
        }
    }
}