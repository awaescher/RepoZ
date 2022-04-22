namespace RepoZ.Api.Common.IO.VoidToolsEverything
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using RepoZ.Api.IO;

    internal class EverythingGitRepositoryFinder : IGitRepositoryFinder
    {
        private readonly IPathSkipper _pathSkipper;

        public EverythingGitRepositoryFinder(IPathSkipper pathSkipper)
        {
            _pathSkipper = pathSkipper ?? throw new ArgumentNullException(nameof(pathSkipper));
        }

        public List<string> Find(string root, Action<string> onFoundAction)
        {
            const string SEARCH = "file: .git\\HEAD";

            var result = Everything64Api.Search($"\"{root}\" {SEARCH}")
                                        .Where(item => !string.IsNullOrWhiteSpace(item))
                                        .Where(item => !_pathSkipper.ShouldSkip(item))
                                        .ToList();

            if (onFoundAction != null)
            {
                foreach (var item in result)
                {
                    onFoundAction.Invoke(item);
                }
            }

            return result;
        }
    }
}