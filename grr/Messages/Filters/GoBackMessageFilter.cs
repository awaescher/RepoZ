using grr.History;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grr.Messages.Filters
{
	public class GoBackMessageFilter : IMessageFilter
	{
		private readonly IHistoryRepository _historyRepository;

		public GoBackMessageFilter(IHistoryRepository historyRepository)
		{
			_historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
		}

		public string Filter(string value)
		{
			if ("-" == value)
			{
				var state = _historyRepository.Load();
				value = state.LastLocation ?? value;
			}

			return value;
		}
	}
}
