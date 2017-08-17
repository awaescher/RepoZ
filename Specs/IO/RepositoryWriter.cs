using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Specs.IO
{
    public class RepositoryWriter
    {
		private readonly string _path;

		public RepositoryWriter(string path)
		{
			_path = path;
		}

		public void Init()
		{
			Repository.Init(_path);
		}

		public void CreateFile(string nameWithExtension, string content)
		{
			File.WriteAllText(Path.Combine(_path, nameWithExtension), content);
		}

		public void Stage(string nameWithExtension)
		{
			using (var repo = new Repository(_path))
			{
				Commands.Stage(repo, Path.Combine(_path, nameWithExtension));
			}
		}

		public void Commit()
		{
			using (var repo = new Repository(_path))
			{
				var signature = new Signature("testuser", "testuser@anywhe.re", DateTimeOffset.Now);
				repo.Commit("A message", signature, signature);
			}
		}

		public void Cleanup()
		{
			Directory.Delete(_path, true);
		}
    }
}
