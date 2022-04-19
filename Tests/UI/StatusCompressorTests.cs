namespace Tests.UI
{
    using FluentAssertions;
    using NUnit.Framework;
    using RepoZ.Api.Git;
    using Tests.Helper;

    public class StatusCompressorTests
    {
        private RepositoryBuilder _builder;
        private StatusCharacterMap _characterMap;
        private StatusCompressor _compressor;

        [SetUp]
        public void Setup()
        {
            _builder = new RepositoryBuilder();
            _characterMap = new StatusCharacterMap();
            _compressor = new StatusCompressor(_characterMap);
        }

        protected string Compress(Repository repo)
        {
            return _compressor.Compress(repo);
        }

        protected string CompressWithBranch(Repository repo)
        {
            return _compressor.CompressWithBranch(repo);
        }

        protected string Up => _characterMap.ArrowUpSign;

        protected string Down => _characterMap.ArrowDownSign;

        protected string Eq => _characterMap.IdenticalSign;

        protected string NoUp => _characterMap.NoUpstreamSign;

        protected string Ellipses => _characterMap.EllipsesSign;

        protected string StashCount => _characterMap.StashSign;

        public class CompressMethod : StatusCompressorTests
        {
            [Test]
            public void Returns_Empty_String_For_Null()
            {
                Compress(null).Should().BeEmpty();
            }

            [Test]
            public void Returns_Empty_String_For_Empty_Repositories()
            {
                var repo = _builder.Build();
                Compress(repo).Should().BeEmpty();
            }

            [Test]
            public void Returns_Empty_String_For_Repositories_With_Just_A_Name()
            {
                var repo = _builder
                           .WithName("Repo")
                           .Build();

                Compress(repo).Should().BeEmpty();
            }

            [Test]
            public void Returns_NoUpstream_For_Repositories_Without_Upstream_And_Just_A_Branch()
            {
                var repo = _builder
                           .WithoutUpstream()
                           .WithCurrentBranch("master")
                           .Build();

                Compress(repo).Should().Be(NoUp);
            }

            [Test]
            public void Returns_IdenticalToUpstream_For_Repositories_With_Upstream_But_Just_A_Branch()
            {
                var repo = _builder
                           .WithUpstream()
                           .WithCurrentBranch("master")
                           .Build();

                Compress(repo).Should().Be(Eq);
            }

            [Test]
            public void Returns_An_ArrowUp_And_The_Count_If_AheadBy()
            {
                var repo = _builder
                           .WithCurrentBranch("master")
                           .WithUpstream()
                           .WithAheadBy(15)
                           .Build();

                Compress(repo).Should().Be($"{Up}15");
            }

            [Test]
            public void Returns_An_ArrowDown_And_The_Count_If_BehindBy()
            {
                var repo = _builder
                           .WithCurrentBranch("master")
                           .WithUpstream()
                           .WithBehindBy(7)
                           .Build();

                Compress(repo).Should().Be($"{Down}7");
            }

            [Test]
            public void Returns_An_ArrowDown_And_An_ArrowDown_And_The_Count_If_AhreadBy_And_BehindBy()
            {
                var repo = _builder
                           .WithCurrentBranch("master")
                           .WithUpstream()
                           .WithAheadBy(15)
                           .WithBehindBy(7)
                           .Build();

                Compress(repo).Should().Be($"{Down}7 {Up}15");
            }

            [Test]
            public void Returns_Just_An_NoUpstream_Sign_If_No_Upstream_Is_Set_And_AheadBy_And_BehindBy_Are_Zero()
            {
                var repo = _builder
                           .WithCurrentBranch("master")
                           .WithoutUpstream()
                           .WithAheadBy(0)
                           .WithBehindBy(0)
                           .Build();

                Compress(repo).Should().Be(NoUp);
            }

            [Test]
            public void Returns_Just_An_NoUpstream_Sign_If_No_Upstream_Is_Set_Even_If_AheadBy_And_BehindBy_Are_Not_Zero()
            {
                var repo = _builder
                           .WithCurrentBranch("master")
                           .WithoutUpstream()
                           .WithAheadBy(15)
                           .WithBehindBy(7)
                           .Build();

                Compress(repo).Should().Be(NoUp);
            }

            [Test]
            public void Returns_An_Identical_Sign_If_AheadBy_And_BehindBy_Is_Zero()
            {
                var repo = _builder
                           .WithCurrentBranch("master")
                           .WithUpstream()
                           .WithAheadBy(0)
                           .WithBehindBy(0)
                           .Build();

                Compress(repo).Should().Be(Eq);
            }

            [Test]
            public void Returns_An_Identical_Sign_If_AheadBy_And_BehindBy_Is_Not_Set()
            {
                var repo = _builder
                           .WithCurrentBranch("master")
                           .WithUpstream()
                           .Build();

                Compress(repo).Should().Be(Eq);
            }

            [Test]
            public void Returns_Added_Staged_Removed_And_NoUpstream_Sign_Without_Upstream()
            {
                var repo = _builder
                           .WithCurrentBranch("master")
                           .WithLocalAdded(1)
                           .WithLocalStaged(2)
                           .WithLocalRemoved(3)
                           .Build();

                Compress(repo).Should().Be($"{NoUp} +1 ~2 -3");
            }

            [Test]
            public void Returns_Added_Staged_Removed_And_Identical_Sign_With_Upstream()
            {
                var repo = _builder
                           .WithCurrentBranch("master")
                           .WithUpstream()
                           .WithLocalAdded(1)
                           .WithLocalStaged(2)
                           .WithLocalRemoved(3)
                           .Build();

                Compress(repo).Should().Be($"{Eq} +1 ~2 -3");
            }


            [Test]
            public void Returns_Untracked_Modified_Missing_Without_Upstream()
            {
                var repo = _builder
                           .WithCurrentBranch("master")
                           .WithoutUpstream()
                           .WithLocalUntracked(4)
                           .WithLocalModified(5)
                           .WithLocalMissing(6)
                           .Build();

                Compress(repo).Should().Be($"{NoUp} +4 ~5 -6");
            }

            [Test]
            public void Returns_Untracked_Modified_Missing_And_Identical_Sign_With_Upstream()
            {
                var repo = _builder
                           .WithCurrentBranch("master")
                           .WithUpstream()
                           .WithLocalUntracked(4)
                           .WithLocalModified(5)
                           .WithLocalMissing(6)
                           .Build();

                Compress(repo).Should().Be($"{Eq} +4 ~5 -6");
            }

            [Test]
            public void Returns_Added_Staged_Removed_Untracked_Modified_Missing_Without_Upstream()
            {
                var repo = _builder
                           .WithCurrentBranch("master")
                           .WithoutUpstream()
                           .WithLocalAdded(1)
                           .WithLocalStaged(2)
                           .WithLocalRemoved(3)
                           .WithLocalUntracked(4)
                           .WithLocalModified(5)
                           .WithLocalMissing(6)
                           .Build();

                Compress(repo).Should().Be($"{NoUp} +1 ~2 -3 | +4 ~5 -6");
            }

            [Test]
            public void Returns_Added_Staged_Removed_Untracked_Modified_Missing_And_Identical_Sign_With_Upstream()
            {
                var repo = _builder
                           .WithCurrentBranch("master")
                           .WithUpstream()
                           .WithLocalAdded(1)
                           .WithLocalStaged(2)
                           .WithLocalRemoved(3)
                           .WithLocalUntracked(4)
                           .WithLocalModified(5)
                           .WithLocalMissing(6)
                           .Build();

                Compress(repo).Should().Be($"{Eq} +1 ~2 -3 | +4 ~5 -6");
            }

            [Test]
            public void Returns_Added_Staged_Removed_Untracked_Modified_Missing_Without_AheadBy_And_BehindBy_Without_Upstream()
            {
                var repo = _builder
                           .WithCurrentBranch("master")
                           .WithoutUpstream()
                           .WithAheadBy(15)
                           .WithBehindBy(7)
                           .WithLocalAdded(1)
                           .WithLocalStaged(2)
                           .WithLocalRemoved(3)
                           .WithLocalUntracked(4)
                           .WithLocalModified(5)
                           .WithLocalMissing(6)
                           .Build();

                Compress(repo).Should().Be($"{NoUp} +1 ~2 -3 | +4 ~5 -6");
            }

            [Test]
            public void Returns_Added_Staged_Removed_Untracked_Modified_Missing_And_AheadBy_And_BehindBy_With_Upstream()
            {
                var repo = _builder
                           .WithCurrentBranch("master")
                           .WithUpstream()
                           .WithAheadBy(15)
                           .WithBehindBy(7)
                           .WithLocalAdded(1)
                           .WithLocalStaged(2)
                           .WithLocalRemoved(3)
                           .WithLocalUntracked(4)
                           .WithLocalModified(5)
                           .WithLocalMissing(6)
                           .Build();

                Compress(repo).Should().Be($"{Down}7 {Up}15 +1 ~2 -3 | +4 ~5 -6");
            }

            [Test]
            public void Returns_Stashed_Only_If_No_Other_Changes_Are_Present()
            {
                var repo = _builder
                           .WithCurrentBranch("master")
                           .WithUpstream()
                           .WithStashCount(7)
                           .Build();

                Compress(repo).Should().Be($"{Eq} {StashCount}7");
            }

            [Test]
            public void Returns_Stashed_With_Pipe_Separator_If_Other_Changes_Are_Present()
            {
                var repo = _builder
                           .WithCurrentBranch("master")
                           .WithUpstream()
                           .WithAheadBy(15)
                           .WithBehindBy(7)
                           .WithLocalAdded(1)
                           .WithLocalStaged(2)
                           .WithLocalRemoved(3)
                           .WithLocalUntracked(4)
                           .WithLocalModified(5)
                           .WithLocalMissing(6)
                           .WithStashCount(7)
                           .Build();

                Compress(repo).Should().Be($"{Down}7 {Up}15 +1 ~2 -3 | +4 ~5 -6 {StashCount}7");
            }

            [Test]
            public void Does_Not_Return_Stashes_If_Stash_Is_Empty()
            {
                var repo = _builder
                           .WithCurrentBranch("master")
                           .WithUpstream()
                           .WithStashCount(0)
                           .Build();

                Compress(repo).Should().Be(Eq);
            }
        }

        public class CompressWithBranchMethod : StatusCompressorTests
        {
            [Test]
            public void Returns_The_Name_Of_The_Current_Branch_With_Its_Status()
            {
                var repo = _builder
                           .WithUpstream()
                           .WithCurrentBranch("develop")
                           .Build();

                CompressWithBranch(repo).Should().Be($"develop {Eq}");
            }

            [Test]
            public void Returns_A_Part_Of_The_Commit_Sha_If_Head_Is_Detached()
            {
                var repo = _builder
                           .WithUpstream()
                           .WithDetachedHeadOnCommit("96728c66c9245a0ba10cddefb1f1cf621743fa5f")
                           .Build();

                CompressWithBranch(repo).Should().Be($"(96728c6{Ellipses}) {Eq}");
            }

            [Test]
            public void Returns_The_Tag_Name_If_Head_Is_Detached_On_A_Tag()
            {
                var repo = _builder
                           .WithUpstream()
                           .WithDetachedHeadOnTag("v1.01")
                           .Build();

                CompressWithBranch(repo).Should().Be($"(v1.01) {Eq}");
            }
        }
    }
}