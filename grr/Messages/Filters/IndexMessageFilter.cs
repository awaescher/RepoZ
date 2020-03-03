using grr.History;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grr.Messages.Filters
{
	public class IndexMessageFilter : IMessageFilter
	{
		private readonly IHistoryRepository _historyRepository;

		public IndexMessageFilter(IHistoryRepository historyRepository)
		{
			_historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
		}

		public void Filter(RepositoryFilterOptions filter)
		{
			if (filter?.RepositoryFilter == null)
				return;

			if (filter.RepositoryFilter.StartsWith(":"))
			{
				string rest = filter.RepositoryFilter.Substring(1);
				if (int.TryParse(rest, out int index))
				{
					index--; // the index visible to the user are 1-based, not 0-based
					var state = _historyRepository.Load();
					if (index >= 0 && state.LastRepositories.Length > index)
						filter.RepositoryFilter = state.LastRepositories[index]?.Name ?? filter.RepositoryFilter;
				}
			}
		}
	}
}
