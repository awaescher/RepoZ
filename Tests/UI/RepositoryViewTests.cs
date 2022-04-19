namespace Tests.UI
{
    using System;
    using FluentAssertions;
    using NUnit.Framework;
    using RepoZ.Api.Git;
    using Tests.Helper;

    public class RepositoryViewTests
    {
        private Repository _repo;
        private RepositoryView _view;
        private StatusCharacterMap _statusCharacterMap;

        [SetUp]
        public void Setup()
        {
            _repo = new RepositoryBuilder().BuildFullFeatured();
            _view = new RepositoryView(_repo);
            _statusCharacterMap = new StatusCharacterMap();
        }

        public class CtorMethod : RepositoryViewTests
        {
            [Test]
            public void Throws_If_Null_Is_Passed_As_Argument()
            {
                Action act = () => _ = new RepositoryView(null);
                act.Should().Throw<ArgumentNullException>();
            }
        }

        public class AheadByProperty : RepositoryViewTests
        {
            [Test]
            public void Returns_The_Repository_Value_As_String()
            {
                _view.AheadBy.Should().Be(_repo.AheadBy.ToString());
            }

            [Test]
            public void Returns_An_Empty_String_For_Null()
            {
                _repo.AheadBy = null;
                _view.AheadBy.Should().BeEmpty();
            }
        }

        public class BehindByProperty : RepositoryViewTests
        {
            [Test]
            public void Returns_The_Repository_Value_As_String()
            {
                _view.BehindBy.Should().Be(_repo.BehindBy.ToString());
            }

            [Test]
            public void Returns_An_Empty_String_For_Null()
            {
                _repo.BehindBy = null;
                _view.BehindBy.Should().BeEmpty();
            }
        }

        public class BranchesProperty : RepositoryViewTests
        {
            [Test]
            public void Returns_The_Repository_Value_As_String()
            {
                _view.Branches.Should().ContainInOrder("master", "feature-one", "feature-two");
            }

            [Test]
            public void Returns_An_Empty_Array_For_Null()
            {
                _repo.Branches = null;
                _view.Branches.Length.Should().Be(0);
            }
        }

        public class CurrentBranchProperty : RepositoryViewTests
        {
            [Test]
            public void Returns_The_Repository_Value()
            {
                _view.CurrentBranch.Should().Be(_repo.CurrentBranch);
            }

            [Test]
            public void Returns_An_Empty_String_For_Null()
            {
                _repo.CurrentBranch = null;
                _view.CurrentBranch.Should().BeEmpty();
            }
        }

        public class LocalAddedProperty : RepositoryViewTests
        {
            [Test]
            public void Returns_The_Repository_Value_As_String()
            {
                _view.LocalAdded.Should().Be(_repo.LocalAdded.ToString());
            }

            [Test]
            public void Returns_An_Empty_String_For_Null()
            {
                _repo.LocalAdded = null;
                _view.LocalAdded.Should().BeEmpty();
            }
        }

        public class LocalIgnoredProperty : RepositoryViewTests
        {
            [Test]
            public void Returns_The_Repository_Value_As_String()
            {
                _view.LocalIgnored.Should().Be(_repo.LocalIgnored.ToString());
            }

            [Test]
            public void Returns_An_Empty_String_For_Null()
            {
                _repo.LocalIgnored = null;
                _view.LocalIgnored.Should().BeEmpty();
            }
        }

        public class StashCountProperty : RepositoryViewTests
        {
            [Test]
            public void Returns_The_Repository_Value_As_String()
            {
                _view.StashCount.Should().Be(_repo.StashCount.ToString());
            }

            [Test]
            public void Returns_An_Empty_String_For_Null()
            {
                _repo.StashCount = null;
                _view.StashCount.Should().BeEmpty();
            }
        }

        public class LocalMissingProperty : RepositoryViewTests
        {
            [Test]
            public void Returns_The_Repository_Value_As_String()
            {
                _view.LocalMissing.Should().Be(_repo.LocalMissing.ToString());
            }

            [Test]
            public void Returns_An_Empty_String_For_Null()
            {
                _repo.LocalMissing = null;
                _view.LocalMissing.Should().BeEmpty();
            }
        }

        public class LocalModifiedProperty : RepositoryViewTests
        {
            [Test]
            public void Returns_The_Repository_Value_As_String()
            {
                _view.LocalModified.Should().Be(_repo.LocalModified.ToString());
            }

            [Test]
            public void Returns_An_Empty_String_For_Null()
            {
                _repo.LocalModified = null;
                _view.LocalModified.Should().BeEmpty();
            }
        }

        public class LocalRemovedProperty : RepositoryViewTests
        {
            [Test]
            public void Returns_The_Repository_Value_As_String()
            {
                _view.LocalRemoved.Should().Be(_repo.LocalRemoved.ToString());
            }

            [Test]
            public void Returns_An_Empty_String_For_Null()
            {
                _repo.LocalRemoved = null;
                _view.LocalRemoved.Should().BeEmpty();
            }
        }

        public class LocalStagedProperty : RepositoryViewTests
        {
            [Test]
            public void Returns_The_Repository_Value_As_String()
            {
                _view.LocalStaged.Should().Be(_repo.LocalStaged.ToString());
            }

            [Test]
            public void Returns_An_Empty_String_For_Null()
            {
                _repo.LocalStaged = null;
                _view.LocalStaged.Should().BeEmpty();
            }
        }

        public class LocalUntrackedProperty : RepositoryViewTests
        {
            [Test]
            public void Returns_The_Repository_Value_As_String()
            {
                _view.LocalUntracked.Should().Be(_repo.LocalUntracked.ToString());
            }

            [Test]
            public void Returns_An_Empty_String_For_Null()
            {
                _repo.LocalUntracked = null;
                _view.LocalUntracked.Should().BeEmpty();
            }
        }

        public class NameProperty : RepositoryViewTests
        {
            [Test]
            public void Returns_The_Repository_Value()
            {
                _view.Name.Should().Be(_repo.Name);
            }

            [Test]
            public void Returns_An_Empty_String_For_Null()
            {
                _repo.Name = null;
                _view.Name.Should().BeEmpty();
            }
        }

        public class PathProperty : RepositoryViewTests
        {
            [Test]
            public void Returns_The_Repository_Value()
            {
                _view.Path.Should().Be(_repo.Path);
            }

            [Test]
            public void Returns_An_Empty_String_For_Null()
            {
                _repo.Path = null;
                _view.Path.Should().BeEmpty();
            }
        }

        public class StatusProperty : RepositoryViewTests
        {
            [Test]
            public void Returns_The_Compressed_String_From_The_StatusCompressor_Helper_Class()
            {
                var expected = new StatusCompressor(_statusCharacterMap).Compress(_repo);
                _view.Status.Should().Be(expected);
            }
        }

        public class MatchesFilterMethod : RepositoryViewTests
        {
            [Test]
            public void Returns_True_If_Filter_Is_Empty()
            {
                _view.MatchesFilter("").Should().Be(true);
            }

            [Test]
            public void Can_Filter_By_Name_Implicit()
            {
                _repo.Name = "Hello World";
                _view.MatchesFilter("lo wo").Should().Be(true);
            }

            [Test]
            public void Can_Filter_By_Name_Explicit()
            {
                _repo.Name = "Hello World";
                _view.MatchesFilter("n lo wo").Should().Be(true);
            }

            [Test]
            public void Can_Filter_By_Branch()
            {
                _repo.Name = "No Match Here";
                _repo.CurrentBranch = "feature/Test";
                _view.MatchesFilter("b feat").Should().Be(true);
            }

            [Test]
            public void Can_Filter_By_Path()
            {
                _repo.Name = "No Match Here";
                _repo.Path = @"C:\Test\Path";
                _view.MatchesFilter(@"p C:\").Should().Be(true);
            }

            [Test]
            public void Returns_True_If_Filter_Is_Empty_Except_Prefix()
            {
                // "n ", "b ", "p " can be used to filter for name, branch and path
                _view.MatchesFilter("b ").Should().Be(true);
            }

            [Test]
            public void Returns_False_If_Prefix_Misses_Space()
            {
                // should be interpreted as "b" search term without prefix
                _repo.Name = "xyz";
                _view.MatchesFilter("b").Should().Be(false);
            }

            [Test]
            public void Returns_False_If_Prefix_Comes_With_Two_Spaces()
            {
                // trimming "b " leads to " master" which is not trimmed by design
                _repo.CurrentBranch = "master";
                _view.MatchesFilter("b  master").Should().Be(false);
            }

            [Test]
            public void Returns_True_For_ToDo_Filter_With_UnpushedChanges()
            {
                _repo.StashCount = 1;
                _repo.HasUnpushedChanges.Should().Be(true);
                _view.MatchesFilter("todo").Should().Be(true);
            }

            [Test]
            public void Returns_False_For_ToDo_Filter_Without_UnpushedChanges()
            {
                _repo = new Repository();
                _repo.HasUnpushedChanges.Should().Be(false);
                new RepositoryView(_repo).MatchesFilter("todo").Should().Be(false);
            }
        }

        public class WasFoundProperty : RepositoryViewTests
        {
            [Test]
            public void Returns_The_Repository_Value()
            {
                _view.WasFound.Should().Be(_repo.WasFound);
            }

            [Test]
            public void Returns_False_If_Path_Is_Empty()
            {
                _repo.Path = "";
                _view.WasFound.Should().BeFalse();
            }

            [Test]
            public void Returns_False_If_Path_Is_Null()
            {
                _repo.Path = null;
                _view.WasFound.Should().BeFalse();
            }
        }

        public class GetHashCodeMethod : RepositoryViewTests
        {
            [Test]
            public void Returns_The_Repository_Value()
            {
                _view.GetHashCode().Should().Be(_repo.GetHashCode());
            }
        }
    }
}