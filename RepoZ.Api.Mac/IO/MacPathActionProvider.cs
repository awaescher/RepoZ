using System;
using System.Collections.Generic;
using RepoZ.Api.IO;

namespace RepoZ.Api.Mac
{
	public class MacPathActionProvider : IPathActionProvider
	{
		public IEnumerable<PathAction> GetFor(string path)
		{
			yield return createPathAction("Open", "nil");
		}

		private PathAction createPathAction(string name, string command)
		{
			return new PathAction()
			{
				Name = name,
				Action = (sender, args) => sender = null
			};
		}

		private PathAction createDefaultPathAction(string name, string command)
		{
			var action = createPathAction(name, command);
			action.IsDefault = true;
			return action;
		}
	}
}
