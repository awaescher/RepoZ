using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepoZ.Api.Git;

namespace RepoZ.Api.Win.Git
{
	public class WindowsRepositoryObserverFactory : IRepositoryObserverFactory
	{
		private IRepositoryReader _repositoryReader;

		public WindowsRepositoryObserverFactory(IRepositoryReader repositoryReader)
		{
			_repositoryReader = repositoryReader;
		}

		public IRepositoryObserver Create() => new WindowsRepositoryObserver(_repositoryReader);
	}
}
