using System.Diagnostics;
using System.Collections.Generic;
using RepoZ.Api.IO;

namespace RepoZ.Api.Mac
{
	public class MacPathActionProvider : IPathActionProvider
	{
		public IEnumerable<PathAction> GetFor(string path)
		{
			yield return createDefaultPathAction("Open in Finder", path);
		}

		private PathAction createPathAction(string name, string command)
		{
			return new PathAction()
			{
				Name = name,
				Action = (sender, args) => startProcess(command)
			};
		}

		private PathAction createDefaultPathAction(string name, string command)
		{
			var action = createPathAction(name, command);
			action.IsDefault = true;
			return action;
		}

		private void startProcess(string command)
		{
			Process.Start(command);
		}
	}
}
