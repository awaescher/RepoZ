using RepoZ.Api.Git;
using RepoZ.Api.Win.PInvoke.Explorer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Api.Win.PInvoke.Explorer
{
	public class WindowsExplorerHandler
	{
		private readonly IRepositoryInformationAggregator _repositoryInfoAggregator;

		public WindowsExplorerHandler(IRepositoryInformationAggregator repositoryInfoAggregator)
		{
			_repositoryInfoAggregator = repositoryInfoAggregator;
		}

		public void UpdateTitles()
		{
			var actor = new AppendRepositoryStatusTitleActor(_repositoryInfoAggregator);
			actor.Pulse();
		}

		public void CleanTitles()
		{
			var actor = new CleanWindowTitleActor();
			actor.Pulse();
		}
	}
}
