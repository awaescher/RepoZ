using NUnit.Framework;
using RepoZ.Ipc;
using FluentAssertions;

namespace Tests.Ipc
{
	public class RepositoryTests
	{
		public class FromStringMethod : RepositoryTests
		{
			[Test]
			public void Splits()
			{
				var r = Repository.FromString("Name::Branch::Path");
				r.Name.Should().Be("Name");
				r.BranchWithStatus.Should().Be("Branch");
				r.Path.Should().Be("Path");
			}

			[Test]
			public void Returns_Null_For_Less_Than_Three_Arguments()
			{
				var r = Repository.FromString("Name::Branch");
				r.Should().BeNull();
			}

			[Test]
			public void Returns_Null_For_More_Than_Three_Arguments()
			{
				var r = Repository.FromString("Name::Branch::Path::Mode");
				r.Should().BeNull();
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
