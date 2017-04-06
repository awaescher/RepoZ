using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepoZ.Api.Git;

namespace RepoZ.Api.Win.Git
{
	public class DefaultRepositoryObserverFactory : IRepositoryObserverFactory
	{
		private IRepositoryReader _repositoryReader;

		public DefaultRepositoryObserverFactory(IRepositoryReader repositoryReader)
		{
			_repositoryReader = repositoryReader;
		}

		public IRepositoryObserver Create() => new DefaultRepositoryObserver(_repositoryReader);
	}
}
