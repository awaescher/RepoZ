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
		public RepositoryWriter(string path)
		{
			Path = path;
		}

		public void InitBare()
		{
			var path = Repository.Init(Path, isBare: true);
		}

		public void Clone(string sourcePath)
		{

			Repository.Clone(sourcePath, Path);
		}

		public void Branch(string name)
		{
			using (var repo = new Repository(Path))
			{
				repo.CreateBranch(name);
			}
		}

		public void CreateFile(string nameWithExtension, string content)
		{
			File.WriteAllText(System.IO.Path.Combine(Path, nameWithExtension), content);
		}

		public void Stage(string nameWithExtension)
		{
			using (var repo = new Repository(Path))
			{
				Commands.Stage(repo, System.IO.Path.Combine(Path, nameWithExtension));
			}
		}

		public void Commit()
		{
			using (var repo = new Repository(Path))
			{
				repo.Commit("A message", Signature, Signature);
			}
		}

		public string Fetch()
		{
			using (var repo = new Repository(Path))
			{
				string logMessage = "";

				var remote = repo.Network.Remotes.Single();
				var refs = remote.FetchRefSpecs.Select(r => r.Specification).ToArray();
				Commands.Fetch(repo, remote.Name, refs, new FetchOptions(), logMessage);

				return logMessage;
			}
		}

		public void Pull()
		{
			using (var repo = new Repository(Path))
			{
				var remote = repo.Network.Remotes.Single();
				Commands.Pull(repo, Signature, new PullOptions());
			}
		}

		public void Merge()
		{
			using (var repo = new Repository(Path))
			{
				repo.Merge(repo.Head, Signature);
			}
		}

		public void Rebase(string ontoBranchName)
		{
			using (var repo = new Repository(Path))
			{
				var branch = repo.Head;
				var upstream = branch.Commits.First();
				var onto = repo.Branches[ontoBranchName];
				repo.Rebase.Start(branch, branch, onto, Identity, new RebaseOptions());
			}
		}

		public void Push()
		{
			using (var repo = new Repository(Path))
			{
				var remote = repo.Network.Remotes.Single();
				var refSpec = remote.RefSpecs.Single();
				repo.Network.Push(remote, @"refs/heads/master", new PushOptions());
			}
		}

		public void Cleanup()
		{
			Directory.Delete(Path, true);
		}

		internal void Checkout(string branch)
		{
			using (var repo = new Repository(Path))
			{
				Commands.Checkout(repo, branch);
			}
		}

		public string Path { get; }

		protected Identity Identity => new Identity("John Doe", "johndoe@anywhe.re");

		protected Signature Signature => new Signature(Identity, DateTimeOffset.Now);

		public string CurrentBranch
		{
			get
			{
				using (var repo = new Repository(Path))
				{
					return repo.Head.FriendlyName;
				}
			}
		}

		public string HeadTip
		{
			get
			{
				using (var repo = new Repository(Path))
				{
					return repo.Head.Tip.Sha;
				}
			}
		}
	}
}
