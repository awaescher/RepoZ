using System;
using System.Linq;
using LibGit2Sharp;
using RepoZ.Api.Git;
using System.Collections.Generic;

namespace RepoZ.Api.Common.Git
{
	public class DefaultRepositoryWriter : IRepositoryWriter
	{
		public bool Checkout(Api.Git.Repository repository, string branchName)
		{
			using (var repo = new LibGit2Sharp.Repository(repository.Path))
			{
				var branch = Commands.Checkout(repo, branchName);
				return branch.FriendlyName == branchName;
			}
		}

		public void Fetch(Api.Git.Repository repository)
		{
			using (var repo = new LibGit2Sharp.Repository(repository.Path))
			{
				//var remote = repo.Network.Remotes.Single();
				//var refs = remote.FetchRefSpecs.Select(r => r.Specification).ToArray();
				//Commands.Fetch(repo, remote.Name, refs, new FetchOptions() { CertificateCheck = fsd, CredentialsProvider = fds }, "");

				var options = new FetchOptions() { CertificateCheck = CheckCertificate, CredentialsProvider = CheckCredentials };

				foreach (Remote remote in repo.Network.Remotes)
				{
					IEnumerable<string> refSpecs = remote.FetchRefSpecs.Select(r => r.Specification);
					Commands.Fetch(repo, remote.Name, refSpecs, options, "");
				}
			}
		}

		private bool CheckCertificate(Certificate certificate, bool valid, string host)
		{
			return true;
		}

		private Credentials CheckCredentials(string url, string usernameFromUrl, SupportedCredentialTypes types)
		{
			return new DefaultCredentials();
		}

		public void Pull(Api.Git.Repository repository)
		{
			using (var repo = new LibGit2Sharp.Repository(repository.Path))
			{
				var remote = repo.Network.Remotes.Single();
				Commands.Pull(repo, new Signature("RepoZ", "RepoZ", DateTimeOffset.Now), new PullOptions());
			}
		}

		public void Push(Api.Git.Repository repository)
		{
			using (var repo = new LibGit2Sharp.Repository(repository.Path))
			{
				var remote = repo.Network.Remotes.Single();
				repo.Network.Push(remote, repo.Head.CanonicalName, new PushOptions());
			}
		}
	}
}
