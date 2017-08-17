using FluentAssertions;
using NUnit.Framework;
using RepoZ.Api.Common.Git;
using Specs.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Specs
{
	public class DefaultRepositoryObserverTests
	{
		private RepositoryWriter _repositoryWriter;
		private DefaultRepositoryObserver _observer;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			string path = @"C:\Temp\Test";

			if (Directory.Exists(path))
				Directory.Delete(path, true);
			Directory.CreateDirectory(path);

			var reader = new DefaultRepositoryReader();
			_observer = new DefaultRepositoryObserver(reader);
			_observer.Setup(path, 100);
			_observer.Observe();

			_repositoryWriter = new RepositoryWriter(Path.Combine(path, "TestRepo"));
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			_observer.Stop();
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

			change.Should().Be(1);
			delete.Should().Be(0);
		}

		private void WaitObserverDelay()
		{
			System.Threading.Thread.Sleep(500);
		}
	}
}
