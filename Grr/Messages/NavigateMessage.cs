using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grr.Messages
{
	public class NavigateMessage
	{
		private readonly string _repositoryFilter;

		public NavigateMessage(string repositoryFilter)
		{
			_repositoryFilter = repositoryFilter;
		}

		public override string ToString() => string.IsNullOrEmpty(_repositoryFilter) 
			? null /* makes no sense */
			: $"list:{_repositoryFilter}";
	}
}
