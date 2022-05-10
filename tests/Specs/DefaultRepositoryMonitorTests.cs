namespace Specs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Threading;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using RepoZ.Api.Common.Common;
    using RepoZ.Api.Common.Git;
    using RepoZ.Api.Common.Git.AutoFetch;
    using RepoZ.Api.Common.IO;
    using RepoZ.Api.Git;
    using RepoZ.Api.IO;
    using Specs.IO;
    using Specs.Mocks;

    public class DefaultRepositoryMonitorTests
    {
        private readonly IFileSystem _fileSystem = new FileSystem();
        private RepositoryWriter _origin;
        private RepositoryWriter _cloneA;
        private RepositoryWriter _cloneB;
        private DefaultRepositoryMonitor _monitor;
        private string _rootPath;

        private string _defaultBranch = "master";

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var fileSystem = new FileSystem();
            _rootPath = Path.Combine(Path.GetTempPath(), "RepoZ_Test_Repositories");

            TryCreateRootPath(_rootPath);

            var repoPath = Path.Combine(_rootPath, Guid.NewGuid().ToString());
            _fileSystem.Directory.CreateDirectory(repoPath);

            var appSettingsService = new Mock<IAppSettingsService>();
            appSettingsService.Setup(x => x.EnabledSearchProviders).Returns(new List<string>(0));

            var defaultRepositoryTagsResolver = new DefaultRepositoryTagsResolver(new Mock<IRepositoryActionConfigurationStore>().Object, fileSystem);
            _monitor = new DefaultRepositoryMonitor(
                new GivenPathProvider(new string[] { repoPath, }),
                new DefaultRepositoryReader(defaultRepositoryTagsResolver),
                new DefaultRepositoryDetectorFactory(new DefaultRepositoryReader(defaultRepositoryTagsResolver)),
                new DefaultRepositoryObserverFactory(),
                new GitRepositoryFinderFactory(appSettingsService.Object, new List<ISingleGitRepositoryFinderFactory>() { new GravellGitRepositoryFinderFactory(new NeverSkippingPathSkipper(), _fileSystem) }),
                new UselessRepositoryStore(),
                new DefaultRepositoryInformationAggregator(
                    new StatusCompressor(new StatusCharacterMap()),
                    new DirectThreadDispatcher()),
                new Mock<IAutoFetchHandler>().Object,
                new Mock<IRepositoryIgnoreStore>().Object,
                _fileSystem)
                {
                    DelayGitRepositoryStatusAfterCreationMilliseconds = 100,
                    DelayGitStatusAfterFileOperationMilliseconds = 100,
                };

            
            _origin = new RepositoryWriter(Path.Combine(repoPath, "BareOrigin"), fileSystem);
            _cloneA = new RepositoryWriter(Path.Combine(repoPath, "CloneA"), fileSystem);
            _cloneB = new RepositoryWriter(Path.Combine(repoPath, "CloneB"), fileSystem);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _monitor.Stop();
            //_monitor.Dispose();

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
            Monitor.Expect(() =>
                    {
                        _origin.InitBare();
                    },
                changes => changes == 0,
                deletes => deletes == 0);
        }

        [Test]
        [Order(1)]
        public void T1A_Detects_Repository_Clone()
        {
            Monitor.Expect(() =>
                    {
                        _cloneA.Clone(_origin.Path);
                        _cloneB.Clone(_origin.Path);
                    },
                changes => changes >= 0, /* TODO */
                deletes => deletes == 0);

            _defaultBranch = _cloneA.CurrentBranch;
            Assert.That(_cloneB.CurrentBranch, Is.EqualTo(_defaultBranch));
        }

        [Test]
        [Order(2)]
        public void T2B_Detects_File_Creation()
        {
            Monitor.Expect(() =>
                    {
                        _cloneA.CreateFile("First.A", "First file on clone A");
                    },
                changes => changes >= 0, /* TODO */
                deletes => deletes == 0);
        }

        [Test]
        [Order(3)]
        public void T2C_Detects_File_Staging()
        {
            Monitor.Expect(() =>
                    {
                        _cloneA.Stage("First.A");
                    },
                changes => changes >= 0, /* TODO */
                deletes => deletes == 0);
        }

        [Test]
        [Order(4)]
        public void T2D_Detects_Repository_Commits()
        {
            Monitor.Expect(() =>
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
            Monitor.Expect(() =>
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
            Monitor.Expect(() =>
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
            Monitor.Expect(() =>
                    {
                        
                        _cloneB.CurrentBranch.Should().Be(_defaultBranch);
                        _cloneB.Branch("develop");
                        _cloneB.Checkout("develop");
                        _cloneB.CurrentBranch.Should().Be("develop");
                    },
                changes => changes >= 1,
                deletes => deletes == 0);
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
            _cloneB.Checkout(_defaultBranch);
            _cloneB.CurrentBranch.Should().Be(_defaultBranch);
        }

        [Test]
        [Order(11)]
        public void T5B_Detects_Repository_Fetch()
        {
            Monitor.Expect(() =>
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
            Monitor.Expect(() =>
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
            _cloneB.CurrentBranch.Should().Be(_defaultBranch);
            _cloneB.Checkout("develop");
            _cloneB.CurrentBranch.Should().Be("develop");
        }

        [Test]
        [Order(14)]
        public void T6B_Detects_Repository_Rebase()
        {
            Monitor.Expect(() =>
                    {
                        var steps = _cloneB.Rebase(_defaultBranch);
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
            _cloneB.Checkout(_defaultBranch);
            _cloneB.CurrentBranch.Should().Be(_defaultBranch);
        }

        [Test]
        [Order(16)]
        public void T7B_Detects_Repository_Merge_With_Other_Branch()
        {
            Monitor.Expect(() => _cloneB.Merge("develop"),
                changes => changes >= 1,
                deletes => deletes == 0);
        }

        [Test]
        [Order(17)]
        public void T8A_Detects_Repository_Push_With_Upstream()
        {
            Monitor.Expect(
                () =>
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

            Monitor.Expect(
                () => _fileSystem.Directory.Delete(_cloneA.Path, true),
                changes: 0,
                deletes: 1);
        }

        private void TryDeleteRootPath(string rootPath)
        {
            if (!_fileSystem.Directory.Exists(rootPath))
            {
                return;
            }

            WaitFileOperationDelay();

            try
            {
                NormalizeReadOnlyFiles(rootPath);

                _fileSystem.Directory.Delete(rootPath, true);
            }
            catch (UnauthorizedAccessException)
            {
                // we cannot do nothing about it here
                Debug.WriteLine(nameof(UnauthorizedAccessException) + ": Could not clear test root path: " + rootPath);
            }

            WaitFileOperationDelay();
        }

        private void NormalizeReadOnlyFiles(string rootPath)
        {
            // set readonly git files to "normal" 
            // otherwise we get UnauthorizedAccessExceptions
            var readOnlyFiles = _fileSystem.Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories)
                                         .Where(f => File.GetAttributes(f).HasFlag(FileAttributes.ReadOnly));

            foreach (var file in readOnlyFiles)
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }
        }

        private void TryCreateRootPath(string rootPath)
        {
            TryDeleteRootPath(rootPath);

            if (_fileSystem.Directory.Exists(rootPath))
            {
                return;
            }

            _fileSystem.Directory.CreateDirectory(rootPath);
        }

        private static void WaitFileOperationDelay()
        {
            Thread.Sleep(500);
        }

        protected DefaultRepositoryMonitor Monitor => _monitor;
    }
}