using System.Diagnostics;
using System.Collections.Generic;
using RepoZ.Api.Git;

namespace RepoZ.Api.Mac
{
	public class MacRepositoryActionProvider : IRepositoryActionProvider
	{
		public IEnumerable<RepositoryAction> GetFor(Repository repository)
		{
			yield return createDefaultAction("Open in Finder", repository.Path);
		}

		private RepositoryAction createAction(string name, string command)
		{
			return new RepositoryAction()
			{
				Name = name,
				Action = (sender, args) => startProcess(command)
			};
		}

		private RepositoryAction createDefaultAction(string name, string command)
		{
			var action = createAction(name, command);
			action.IsDefault = true;
			return action;
		}

		private void startProcess(string command)
		{
			Process.Start(command);
		}
	}
}
