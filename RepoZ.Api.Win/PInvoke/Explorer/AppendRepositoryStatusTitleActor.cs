using RepoZ.Api.Git;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Api.Win.PInvoke.Explorer
{
	public class AppendRepositoryStatusTitleActor : ExplorerWindowActor
	{
		private readonly IRepositoryInformationAggregator _repositoryInfoAggregator;

		public AppendRepositoryStatusTitleActor(IRepositoryInformationAggregator repositoryInfoAggregator)
		{
			_repositoryInfoAggregator = repositoryInfoAggregator;
		}

		protected override void Act(IntPtr hwnd, string explorerLocationUrl)
		{
			if (!string.IsNullOrEmpty(explorerLocationUrl))
			{
				string path = new Uri(explorerLocationUrl).LocalPath;

				string status = _repositoryInfoAggregator.GetStatusByPath(path);

				if (!string.IsNullOrEmpty(status))
				{
					string separator = "  [";
					WindowHelper.AppendWindowText(hwnd, separator, status + "]");
				}
			}
		}
	}
}
