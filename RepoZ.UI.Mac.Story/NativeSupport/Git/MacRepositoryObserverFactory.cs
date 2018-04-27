using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepoZ.Api.Git;

namespace RepoZ.UI.Mac.Story.NativeSupport.Git
{
	public class MacRepositoryObserverFactory : IRepositoryObserverFactory
	{
		public IRepositoryObserver Create() => new MacRepositoryObserver();
	}
}
