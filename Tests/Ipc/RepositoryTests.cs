namespace Tests.Ipc
{
    using NUnit.Framework;
    using RepoZ.Ipc;
    using FluentAssertions;

    public class RepositoryTests
    {
        public class FromStringMethod : RepositoryTests
        {
            [Test]
            public void Deserializes_With_Three_Arguments()
            {
                var r = Repository.FromString("Name::Branch::Path");
                r.Name.Should().Be("Name");
                r.BranchWithStatus.Should().Be("Branch");
                r.Path.Should().Be("Path");
                r.HasUnpushedChanges.Should().BeFalse();
            }

            [Test]
            public void Deserializes_With_Four_Arguments()
            {
                var r = Repository.FromString("Name::Branch::Path::1");
                r.Name.Should().Be("Name");
                r.BranchWithStatus.Should().Be("Branch");
                r.Path.Should().Be("Path");
                r.HasUnpushedChanges.Should().BeTrue();
            }

            [Test]
            public void Returns_Null_For_Less_Than_Three_Arguments()
            {
                var r = Repository.FromString("Name::Branch");
                r.Should().BeNull();
            }

            [Test]
            public void Returns_Null_For_Too_Much_Arguments()
            {
                var r = Repository.FromString("Name::Branch::Path::1::Mode");
                r.Should().BeNull();
            }
        }

        public class ToStringMethod : RepositoryTests
        {
            [Test]
            public void Serializes_With_Four_Arguments()
            {
                var r = new Repository
                    {
                        Name = "N",
                        BranchWithStatus = "B",
                        Path = "P"
                    };
                r.ToString().Should().Be("N::B::P::0"); // 0 is "HasUnpushedChanges"
            }

            [Test]
            public void Returns_Null_For_Less_Than_Mandatory_Arguments()
            {
                var r = new Repository
                    {
                        Name = "N",
                        BranchWithStatus = "B"
                    };
                r.ToString().Should().Be("");
            }
        }

        public class SafePathProperty : RepositoryTests
        {
            [Test]
            public void Replaces_Backslashes_With_Slashes()
            {
                var r = Repository.FromString("Name::Branch::C:\\Users\\doe");
                r.SafePath.Should().Be("C:/Users/doe");
            }

            [Test]
            public void Returns_Empty_String_For_Null_Path()
            {
                var r = Repository.FromString("Name::Branch::C:\\Users\\doe");
                r.Path = null;
                r.SafePath.Should().Be("");
            }
        }
    }
}