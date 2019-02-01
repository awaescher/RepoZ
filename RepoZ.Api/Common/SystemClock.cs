using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Api.Common
{
	public class SystemClock : IClock
	{
		public DateTime Now => DateTime.Now;
	}
}
