using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grr.Messages.Filters
{
	public interface IMessageFilter
	{
		void Filter(RepositoryFilterOptions filter);
	}
}
