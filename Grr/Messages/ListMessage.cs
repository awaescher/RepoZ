using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grr.Messages
{
	public class ListMessage : IMessage
	{
		private readonly string _repositoryFilter;

		public ListMessage(string repositoryFilter)
		{
			_repositoryFilter = repositoryFilter;
		}

		public void Execute(Repository[] repositories)
		{
			// nothing to do
		}

		public string GetRemoteCommand() => ToString();

		public override string ToString() => string.IsNullOrEmpty(_repositoryFilter) 
			? $"list:*"
			: $"list:{_repositoryFilter}";
	}
}
