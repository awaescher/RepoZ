using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grr.Messages
{
	public class ListMessage
	{
		private readonly string _repositoryFilter;

		public ListMessage(string repositoryFilter)
		{
			_repositoryFilter = repositoryFilter;
		}

		public override string ToString() => string.IsNullOrEmpty(_repositoryFilter) 
			? $"list:*"
			: $"list:{_repositoryFilter}";
	}
}
