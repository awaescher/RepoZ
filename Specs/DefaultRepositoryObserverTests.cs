using FluentAssertions;
using NUnit.Framework;
using RepoZ.Api.Common.Git;
using Specs.IO;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Specs
{
	public class DefaultRepositoryObserverTests
	{
		private RepositoryWriter _repositoryWriter;
		private DefaultRepositoryObserver _observer;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			string rootPath = @"C:\Temp\TestRepositories\";

			TryClearRootPath(rootPath);
			WaitFileOperationDelay();

			string repoPath = Path.Combine(rootPath, Guid.NewGuid().ToString());

			var reader = new DefaultRepositoryReader();
			_observer = new DefaultRepositoryObserver(reader);
			_observer.Setup(rootPath, 100);
			_observer.Observe();

			_repositoryWriter = new RepositoryWriter(Path.Combine(repoPath, "TestRepo"));
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			_observer.Stop();
			_observer.Dispose();

			WaitFileOperationDelay();

			_repositoryWriter.Cleanup();
		}

		[Test]
		[Order(0)]
		public void Detects_Repository_Creation()
		{
			int change = 0;
			int delete = 0;

			_observer.OnAddOrChange = (r) => change++;
			_observer.OnDelete = (path) => delete++;

			_repositoryWriter.Init();

			WaitObserverDelay();

			change.Should().Be(0); // TODO should detect
			delete.Should().Be(0);
		}

		[Test]
		[Order(1)]
		public void Detects_File_Creation()
		{
			int change = 0;
			int delete = 0;

			_observer.OnAddOrChange = (r) => change++;
			_observer.OnDelete = (path) => delete++;

			_repositoryWriter.CreateFile("Repo.Z", "Repository Monitor");

			WaitObserverDelay();

			change.Should().Be(0); // TODO should detect
			delete.Should().Be(0);
		}

		[Test]
		[Order(2)]
		public void Detects_File_Staging()
		{
			int change = 0;
			int delete = 0;

			_observer.OnAddOrChange = (r) => change++;
			_observer.OnDelete = (path) => delete++;

			_repositoryWriter.Stage("Repo.Z");

			WaitObserverDelay();

			change.Should().Be(0); // TODO should detect
			delete.Should().Be(0);
		}

		[Test]
		[Order(3)]
		public void Detects_Commits()
		{
			int change = 0;
			int delete = 0;

			_observer.OnAddOrChange = (r) => change++;
			_observer.OnDelete = (path) => delete++;

			_repositoryWriter.Commit();

			WaitObserverDelay();

			change.Should().BeGreaterOrEqualTo(1);
			delete.Should().Be(0);
		}

		private void WaitFileOperationDelay()
		{
			Thread.Sleep(100);
		}

		private void WaitObserverDelay()
		{
			Thread.Sleep(500);
		}

		private static void TryClearRootPath(string rootPath)
		{
			if (Directory.Exists(rootPath))
			{
				foreach (var dir in new DirectoryInfo(rootPath).GetDirectories())
				{
					try
					{
						dir.Delete(true);
					}
					catch (UnauthorizedAccessException)
					{
						// we cannot do nothing about it here
						Debug.WriteLine(nameof(UnauthorizedAccessException) + ": Could not clear test root path: " + dir.FullName);
					}
				}
			}
			else
			{
				Directory.CreateDirectory(rootPath);
			}
		}
	}
}
