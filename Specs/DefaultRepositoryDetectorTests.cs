using FluentAssertions;
using NUnit.Framework;
using RepoZ.Api.Common.Git;
using Specs.IO;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Specs
{
	public class DefaultRepositoryDetectorTests
	{
		private RepositoryWriter _origin;
		private RepositoryWriter _cloneA;
		private RepositoryWriter _cloneB;
		private DefaultRepositoryDetector _detector;
		private string _rootPath;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_rootPath = Path.Combine(Path.GetTempPath(), "RepoZ_Test_Repositories");

			TryCreateRootPath(_rootPath);

			string repoPath = Path.Combine(_rootPath, Guid.NewGuid().ToString());

			var reader = new DefaultRepositoryReader();
			_detector = new DefaultRepositoryDetector(reader);
			_detector.Setup(_rootPath, 100);

			_origin = new RepositoryWriter(Path.Combine(repoPath, "BareOrigin"));
			_cloneA = new RepositoryWriter(Path.Combine(repoPath, "CloneA"));
			_cloneB = new RepositoryWriter(Path.Combine(repoPath, "CloneB"));
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			_detector.Stop();
			_detector.Dispose();

			WaitFileOperationDelay();

			TryDeleteRootPath(_rootPath);
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
     +---------------------------^  |       |                   |          stage file
                                    |       |                   |              |
                                    |       |                   |              v
                                    |       |     [[5]]         |          commit file
                                    |       |            merge  v   [[6]]      |
          [[9]]                     |       +-----------------> @------------->+
      delete cloneA                 |                           |    rebase    |
                                    |                           |              |
                                    |                           +<-------------+
                                    |                           |   merge
                                    |                           |   [[7]]
                                    |    push                   |
                                    +---------------------------+
                                             [[8]]

		 */

		[Test]
		[Order(0)]
		public void T0A_Detects_Repository_Creation()
		{
			Detector.Expect(() =>
			{
				_origin.InitBare();
			},
			changes => changes >= 1,
			deletes => deletes == 0);
		}

		[Test]
		[Order(1)]
		public void T1A_Detects_Repository_Clone()
		{
			Detector.Expect(() =>
			{
				_cloneA.Clone(_origin.Path);
				_cloneB.Clone(_origin.Path);
			},
			changes => changes >= 1,
			deletes => deletes == 0);
		}

		[Test]
		[Order(2)]
		public void T2B_Detects_File_Creation()
		{
			Detector.Expect(() =>
			{
				_cloneA.CreateFile("First.A", "First file on clone A");
			},
			changes => changes >= 1,
			deletes => deletes == 0);
		}

		[Test]
		[Order(3)]
		public void T2C_Detects_File_Staging()
		{
			Detector.Expect(() =>
			{
				_cloneA.Stage("First.A");
			},
			changes => changes >= 1,
			deletes => deletes == 0);
		}

		[Test]
		[Order(4)]
		public void T2D_Detects_Repository_Commits()
		{
			Detector.Expect(() =>
			{
				_cloneA.Commit("Commit #1 on A");
			},
			changes => changes >= 1,
			deletes => deletes == 0);
		}

		[Test]
		[Order(5)]
		public void T2E_Detects_Repository_Pushes()
		{
			Detector.Expect(() =>
			{
				_cloneA.Push();
				_origin.HeadTip.Should().Be(_cloneA.HeadTip);
			},
			changes => changes >= 1,
			deletes => deletes == 0);
		}

		[Test]
		[Order(6)]
		public void T3A_Detects_Repository_Pull()
		{
			Detector.Expect(() =>
			{
				_cloneB.Pull();
				_cloneB.HeadTip.Should().Be(_cloneA.HeadTip);
			},
			changes => changes >= 1,
			deletes => deletes == 0);
		}

		[Test]
		[Order(7)]
		public void T4A_Detects_Repository_Branch_And_Checkout()
		{
			Detector.Expect(() =>
			{
				_cloneB.CurrentBranch.Should().Be("master");
				_cloneB.Branch("develop");
				_cloneB.Checkout("develop");
				_cloneB.CurrentBranch.Should().Be("develop");
			},
			changes: 1,
			deletes: 0);
		}

		[Test]
		[Order(8)]
		public void T4B_Preparation_Add_Changes_To_A_And_Push()
		{
			_cloneA.CreateFile("Second.A", "Second file on clone A");
			_cloneA.Stage("Second.A");
			_cloneA.Commit("Commit #2 on A");
			_cloneA.Push();
		}

		[Test]
		[Order(9)]
		public void T4C_Preparation_Add_Changes_To_B()
		{
			_cloneB.CreateFile("First.B", "First file on clone B");
			_cloneB.Stage("First.B");
			_cloneB.Commit("Commit #1 on B");
		}

		[Test]
		[Order(10)]
		public void T5A_Preparation_Checkout_Master()
		{
			_cloneB.CurrentBranch.Should().Be("develop");
			_cloneB.Checkout("master");
			_cloneB.CurrentBranch.Should().Be("master");
		}

		[Test]
		[Order(11)]
		public void T5B_Detects_Repository_Fetch()
		{
			Detector.Expect(() =>
			{
				_cloneB.Fetch();
			},
			changes => changes >= 1,
			deletes => deletes == 0);
		}

		[Test]
		[Order(12)]
		public void T5C_Detects_Repository_Merge_Tracked_Branch()
		{
			Detector.Expect(() =>
			{
				_cloneB.MergeWithTracked();
			},
			changes => changes >= 1,
			deletes => deletes == 0);
		}

		[Test]
		[Order(13)]
		public void T6A_Preparation_Checkout_Develop()
		{
			_cloneB.CurrentBranch.Should().Be("master");
			_cloneB.Checkout("develop");
			_cloneB.CurrentBranch.Should().Be("develop");
		}

		[Test]
		[Order(14)]
		public void T6B_Detects_Repository_Rebase()
		{
			Detector.Expect(() =>
			{
				int steps = _cloneB.Rebase("master");
				steps.Should().Be(1);
			},
			changes => changes >= 1,
			deletes => deletes == 0);
		}

		[Test]
		[Order(15)]
		public void T7A_Preparation_Checkout_Master()
		{
			_cloneB.CurrentBranch.Should().Be("develop");
			_cloneB.Checkout("master");
			_cloneB.CurrentBranch.Should().Be("master");
		}

		[Test]
		[Order(16)]
		public void T7B_Detects_Repository_Merge_With_Other_Branch()
		{
			Detector.Expect(() =>
			{
				_cloneB.Merge("develop");
			},
			changes: 1,
			deletes: 0);
		}

		[Test]
		[Order(17)]
		public void T8A_Detects_Repository_Push_With_Upstream()
		{
			Detector.Expect(() =>
			{
				_origin.HeadTip.Should().NotBe(_cloneB.HeadTip);

				_cloneB.Push();

				_origin.HeadTip.Should().Be(_cloneB.HeadTip);
			},
			changes => changes >= 1,
			deletes => deletes == 0);
		}

		[Test]
		[Order(18)]
		public void T9A_Detects_Repository_Deletion()
		{
			NormalizeReadOnlyFiles(_cloneA.Path);

			Detector.Expect(() =>
			{
				Directory.Delete(_cloneA.Path, true);
			},
			changes: 0,
			deletes: 1);
		}

		private static void TryDeleteRootPath(string rootPath)
		{
			if (!Directory.Exists(rootPath))
				return;

			WaitFileOperationDelay();

			try
			{
				NormalizeReadOnlyFiles(rootPath);

				Directory.Delete(rootPath, true);
			}
			catch (UnauthorizedAccessException)
			{
				// we cannot do nothing about it here
				Debug.WriteLine(nameof(UnauthorizedAccessException) + ": Could not clear test root path: " + rootPath);
			}

			WaitFileOperationDelay();
		}

		private static void NormalizeReadOnlyFiles(string rootPath)
		{
			// set readonly git files to "normal" 
			// otherwise we get UnauthorizedAccessExceptions
			var readOnlyFiles = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories)
				.Where(f => File.GetAttributes(f).HasFlag(FileAttributes.ReadOnly));

			foreach (var file in readOnlyFiles)
				File.SetAttributes(file, FileAttributes.Normal);
		}

		private static void TryCreateRootPath(string rootPath)
		{
			TryDeleteRootPath(rootPath);

			if (Directory.Exists(rootPath))
				return;

			Directory.CreateDirectory(rootPath);
		}

		private static void WaitFileOperationDelay()
		{
			Thread.Sleep(500);
		}

		protected DefaultRepositoryDetector Detector => _detector;
	}
}
