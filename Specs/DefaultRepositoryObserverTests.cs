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
		private RepositoryWriter _origin;
		private RepositoryWriter _cloneA;
		private RepositoryWriter _cloneB;
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

			_origin = new RepositoryWriter(Path.Combine(repoPath, "TestRepo"));
			_cloneA = new RepositoryWriter(Path.Combine(repoPath, "TestRepoClone1"));
			_cloneB = new RepositoryWriter(Path.Combine(repoPath, "TestRepoClone2"));

		}

		[OneTimeTearDown]
		public void TearDown()
		{
			_observer.Stop();
			_observer.Dispose();

			WaitFileOperationDelay();

			_origin.Cleanup();
			_cloneA.Cleanup();
			_cloneB.Cleanup();
		}

		/*

			        [[1]]                          [[1]]

  cloneA   <-----------------+   origin  +----------------->  cloneB
     +                                 +                        +
     |                           ^  ^  |                        |
     v       [[2]]               |  |  |         [[3]]          v
 add file                        |  |  +----------------------> +--+    [[4]]
     +                           |  |  |        pull            |  |
     |                           |  |  |                        |  |
     v                           |  |  |                        |  +---->  branch "develop"   
 stage file                      |  |  |fetch                   |              |
     +                           |  |  |                  master|              v
     |                           |  |  +--> +                   |          create file
     v                  push to  |  |       |                   |              |
commit file             master   |  |       |                   |              v
      +--------------------------^  |       |                   |          stage file
                                    |       |                   |              |
                                    |       |                   |              v
                                    |       |     [[5]]         |          commit file
                                    |       |            merge  v              |
                                    |       +-----------------> @              |
                                    |                                          |
                                    |                           |   rebase     |
                                    |                           +^-------------+
                                    |                           |   [[6]]
                                    |                           ^
                                    |    push                  +++
                                    +--------------------------+ |
                                             [[7]]             +-+



		 */

		[Test]
		[Order(0)]
		public void T0A_Detects_Repository_Creation()
		{
			_origin.InitBare();

			//_originWriter.Checkout("master");

			WaitObserverDelay();
		}

		[Test]
		[Order(1)]
		public void T1A_Detects_Repository_Clone()
		{
			_cloneA.Clone(_origin.Path);
			_cloneB.Clone(_origin.Path);

			WaitObserverDelay();
		}

		[Test]
		[Order(2)]
		public void T2B_Detects_File_Creation()
		{
			_cloneA.CreateFile("Repo.Z", "Repository Monitor");

			WaitObserverDelay();
		}

		[Test]
		[Order(3)]
		public void T2C_Detects_File_Staging()
		{
			_cloneA.Stage("Repo.Z");

			WaitObserverDelay();
		}

		[Test]
		[Order(4)]
		public void T2D_Detects_Repository_Commits()
		{
			_cloneA.Commit();

			WaitObserverDelay();
		}

		[Test]
		[Order(5)]
		public void T2E_Detects_Repository_Pushes()
		{
			_cloneA.Push();
			_origin.HeadTip.Should().Be(_cloneA.HeadTip);

			WaitObserverDelay();
		}

		[Test]
		[Order(6)]
		public void T3A_Detects_Repository_Pull()
		{
			_cloneB.Pull();
			_cloneB.HeadTip.Should().Be(_cloneA.HeadTip);

			WaitObserverDelay();
		}
		
		[Test]
		[Order(7)]
		public void T4A_Detects_Repository_Branch_And_Checkout()
		{
			_cloneB.CurrentBranch.Should().Be("master");
			_cloneB.Branch("develop");
			_cloneB.Checkout("develop");
			_cloneB.CurrentBranch.Should().Be("develop");

			WaitObserverDelay();
		}

		[Test]
		[Order(8)]
		public void T4B_Detects_Repository_Branch_And_Checkout()
		{
			_cloneB.CreateFile("CloneB.dmp", "Clone B was here");
			_cloneB.Stage("CloneB.dmp");
			_cloneB.Commit();

			WaitObserverDelay();
		}

		[Test]
		[Order(9)]
		public void T5A_Checkout_Master()
		{
			_cloneB.CurrentBranch.Should().Be("develop");
			_cloneB.Checkout("master");
			_cloneB.CurrentBranch.Should().Be("master");

			WaitObserverDelay();
		}

		[Test]
		[Order(10)]
		public void T5B_Detects_Repository_Fetch()
		{
			_cloneB.Fetch();

			WaitObserverDelay();
		}

		[Test]
		[Order(11)]
		public void T5C_Detects_Repository_Merge()
		{
			_cloneB.Merge();

			WaitObserverDelay();
		}

		[Test]
		[Order(12)]
		public void T6A_Checkout_Develop()
		{
			_cloneB.CurrentBranch.Should().Be("master");
			_cloneB.Checkout("develop");
			_cloneB.CurrentBranch.Should().Be("develop");

			WaitObserverDelay();
		}

		[Test]
		[Order(13)]
		public void T6B_Detects_Repository_Rebase()
		{
			_cloneB.Rebase("master");

			WaitObserverDelay();
		}

		[Test]
		[Order(14)]
		public void T7A_Detects_Repository_Push_With_Upstream()
		{
			_cloneB.Push();

			WaitObserverDelay();
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
