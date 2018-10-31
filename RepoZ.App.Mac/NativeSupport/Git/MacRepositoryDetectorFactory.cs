using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepoZ.Api.Git;

namespace RepoZ.App.Mac.NativeSupport.Git
{
	public class MacRepositoryDetectorFactory : IRepositoryDetectorFactory
	{
		private IRepositoryReader _repositoryReader;

		public MacRepositoryDetectorFactory(IRepositoryReader repositoryReader)
		{
			_repositoryReader = repositoryReader;
		}

		public IRepositoryDetector Create() => new MacRepositoryDetector(_repositoryReader);
	}
}
