namespace RepoZ.Api.Common.IO
{
    using RepoZ.Api.IO;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    // http://stackoverflow.com/questions/2106877/is-there-a-faster-way-than-this-to-find-all-the-files-in-a-directory-and-all-sub

    public class GravellGitRepositoryFinder : IGitRepositoryFinder
    {
        private readonly IPathSkipper _pathSkipper;

        public GravellGitRepositoryFinder(IPathSkipper pathSkipper)
        {
            _pathSkipper = pathSkipper;
        }

        public List<string> Find(string root, Action<string> onFoundAction)
        {
            return FindInternal(root, "HEAD", onFoundAction).ToList();
        }

        private IEnumerable<string> FindInternal(string root, string searchPattern, Action<string> onFound)
        {
            var pending = new Queue<string>();
            pending.Enqueue(root);
            while (pending.Count > 0)
            {
                root = pending.Dequeue();

                if (_pathSkipper.ShouldSkip(root))
                {
                    continue;
                }

                string[] tmp;
                try
                {
                    tmp = Directory.GetFiles(root, searchPattern);
                }
                catch (Exception)
                {
                    continue;
                }

                for (var i = 0; i < tmp.Length; i++)
                {
                    onFound?.Invoke(tmp[i]);
                    yield return tmp[i];
                }

                tmp = Directory.GetDirectories(root);
                for (int i = 0; i < tmp.Length; i++)
                {
                    pending.Enqueue(tmp[i]);
                }
            }
        }
    }
}