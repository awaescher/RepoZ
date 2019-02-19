using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepoZ.Api.Git;

namespace RepoZ.Api.Common.Git
{
	public class DefaultRepositoryDetectorFactory : IRepositoryDetectorFactory
	{
		private readonly IRepositoryReader _repositoryReader;

		public DefaultRepositoryDetectorFactory(IRepositoryReader repositoryReader)
		{
			_repositoryReader = repositoryReader;
		}

		public IRepositoryDetector Create() => new DefaultRepositoryDetector(_repositoryReader);
	}
}
