namespace grr.History
{
    using RepoZ.Ipc;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Text;

    public class FileHistoryRepository : IHistoryRepository
    {
        private readonly IFileSystem _fileSystem;

        public FileHistoryRepository(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        public void Save(State state)
        {
            // if multiple repositories were found the last time we ran grr,
            // these were written to the last state.
            // if the user selects one with an index like "grr cd :2", we want
            // to keep the last repositories to enable him to choose another one
            // with the same indexes as before.
            // so we have to get the old repositories - load and copy them if required
            if (!state.OverwriteRepositories)
            {
                State oldState = Load();
                if (oldState?.LastRepositories?.Length > 0)
                {
                    state.LastRepositories = oldState.LastRepositories;
                }
            }

            var lines = new string[] { state?.LastLocation ?? "", Serialize(state?.LastRepositories ?? Array.Empty<Repository>()) };

            try
            {
                _fileSystem.File.WriteAllLines(GetFileName(), lines, Encoding.Default);
            }
            catch (Exception)
            {
                /* safely ignore this, saving the state is optional */
            }
        }

        public State Load()
        {
            string[] lines = null;

            try
            {
                lines = _fileSystem.File.ReadAllLines(GetFileName(), Encoding.Default);
            }
            catch
            {
                /* safely ignore this, reading the state is optional */
            }

            if (lines?.Length != 2)
            {
                return new State()
                    {
                        LastLocation = string.Empty,
                        LastRepositories = Array.Empty<Repository>()
                    };
            }

            return new State()
                {
                    LastLocation = lines[0],
                    LastRepositories = Deserialize(lines[1])
                };
        }

        private static string GetFileName()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RepoZ", "state.grr");
        }

        private static string Serialize(IEnumerable<Repository> repositories)
        {
            if (repositories == null)
            {
                return string.Empty;
            }

            var names = repositories
                        .Select(r => r.Name)
                        .ToArray();

            if (!names.Any())
            {
                return string.Empty;
            }

            return string.Join("|", names);
        }

        private static Repository[] Deserialize(string repositoryString)
        {
            if (string.IsNullOrEmpty(repositoryString))
            {
                return Array.Empty<Repository>();
            }

            return repositoryString
                   .Split(new string[] { "|", }, StringSplitOptions.None)
                   .Select(s => new Repository() { Name = s, })
                   .ToArray();
        }
    }
}