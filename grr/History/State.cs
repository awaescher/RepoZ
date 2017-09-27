using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grr.History
{
	public class State
	{
		public Repository[] LastRepositories { get; set; }
		public string LastLocation { get; set; }
	}
}
