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

		public string Filter(string value)
		{
			if (value?.StartsWith("@") ?? false)
			{
				string rest = value.Substring(1);
				if (int.TryParse(rest, out int index))
				{
					var state = _historyRepository.Load();
					if (state.LastRepositories.Length > index)
						value = state.LastRepositories[index]?.Name ?? value;
				}
			}

			return value;
		}
	}
}
