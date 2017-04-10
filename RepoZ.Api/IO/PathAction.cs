using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Api.IO
{
	public class PathAction
	{
		public string Name { get; set; }
		public Action Action { get; set; }
		public bool IsDefault { get; set; } = false;
	}
}
