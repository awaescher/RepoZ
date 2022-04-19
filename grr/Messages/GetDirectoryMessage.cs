namespace grr.Messages
{
    using System;
    using RepoZ.Ipc;

    [System.Diagnostics.DebuggerDisplay("{GetRemoteCommand()}")]
    public class GetDirectoryMessage : DirectoryMessage
    {
        public GetDirectoryMessage(RepositoryFilterOptions filter)
            : base(filter)
        {
        }

        protected override void ExecuteExistingDirectory(string directory)
        {
            directory = $"\"{directory}\"";

            TextCopy.ClipboardService.SetText(directory);
            Console.WriteLine(directory);
        }

        protected override void ExecuteRepositoryQuery(Repository[] repositories)
        {
            if (repositories?.Length > 1)
            {
                // only use the first repository when multiple repositories came in
                // cd makes no sense with multiple repositories
                System.Console.WriteLine("");
                System.Console.WriteLine($"Found multiple repositories, using {repositories[0].Name}.");
                System.Console.WriteLine("You can get the others by index now, like:\n  grr gd :2");
                base.ExecuteRepositoryQuery(new Repository[] { repositories[0] });
            }
            else
            {
                base.ExecuteRepositoryQuery(repositories);
            }
        }

        public override bool ShouldWriteRepositories(Repository[] repositories)
        {
            return (repositories?.Length ?? 0) > 1;
        }
    }
}