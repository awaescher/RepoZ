using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Api.Git
{
	public interface IRepositoryObserverFactory
	{
		IRepositoryObserver Create();
	}
}
