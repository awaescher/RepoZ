using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using RepoZ.Api.Git;
using Tests.Helper;

namespace Tests.Git
{
	public class RepositoryTests
	{
		protected RepositoryBuilder _builder1;
		protected RepositoryBuilder _builder2;

		[SetUp]
		public void Setup()
		{
			_builder1 = new RepositoryBuilder();
			_builder2 = new RepositoryBuilder();
		}

		public class EqualsMethod : RepositoryTests
		{
			[Test]
			public void Is_Not_Case_Sensitive()
			{
				var r1 = _builder1.WithPath(@"C:\Develop\RepoZ\RepoZ\").Build();
				var r2 = _builder2.WithPath(@"c:\develop\repoz\rEPOz\").Build();

				r1.Equals(r2).Should().BeTrue();
			}

			[Test]
			public void Ignores_Ending_Slash()
			{
				var r1 = _builder1.WithPath(@"/c/develop/repoz/repoz").Build();
				var r2 = _builder2.WithPath(@"/c/develop/repoz/repoz/").Build();

				r1.Equals(r2).Should().BeTrue();
			}

			[Test]
			public void Ignores_Ending_Slashes()
			{
				var r1 = _builder1.WithPath(@"/c/develop/repoz/repoz/").Build();
				var r2 = _builder2.WithPath(@"/c/develop/repoz/repoz///").Build();

				r1.Equals(r2).Should().BeTrue();
			}

			[Test]
			public void Ignores_Ending_Backslash()
			{
				var r1 = _builder1.WithPath(@"C:\Develop\RepoZ\RepoZ").Build();
				var r2 = _builder2.WithPath(@"C:\Develop\RepoZ\RepoZ\").Build();

				r1.Equals(r2).Should().BeTrue();
			}

			[Test]
			public void Ignores_Ending_Backslashes()
			{
				var r1 = _builder1.WithPath(@"C:\Develop\RepoZ\RepoZ\").Build();
				var r2 = _builder2.WithPath(@"C:\Develop\RepoZ\RepoZ\\\").Build();

				r1.Equals(r2).Should().BeTrue();
			}

			[Test]
			public void Can_Use_Either_Slashes_Or_Backslashes()
			{
				var r1 = _builder1.WithPath(@"C:\Develop\RepoZ\RepoZ\").Build();
				var r2 = _builder2.WithPath(@"C:/Develop/RepoZ/RepoZ/").Build();

				r1.Equals(r2).Should().BeTrue();
			}

			[Test]
			public void Accepts_Leading_Whitespaces()
			{
				var r1 = _builder1.WithPath(@"C:\Develop\RepoZ\RepoZ").Build();
				var r2 = _builder2.WithPath(@"   C:\Develop\RepoZ\RepoZ").Build();

				r1.Equals(r2).Should().BeTrue();
			}

			[Test]
			public void Accepts_Empty_Strings()
			{
				var r1 = _builder1.WithPath("").Build();
				var r2 = _builder2.WithPath("").Build();

				r1.Equals(r2).Should().BeTrue();
			}

			[Test]
			public void Can_Handle_Null()
			{
				var r1 = _builder1.WithPath(@"C:\Develop\RepoZ\RepoZ\").Build();

				r1.Equals(null).Should().BeFalse();
			}
		}
	}
}
