using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grr
{
	[System.Diagnostics.DebuggerDisplay("{Name}")]
	public class Repository
	{
		public static Repository FromString(string value)
		{
			var parts = value?.Split('|');
			if (parts?.Length != 3)
				return null;

			return new Repository()
			{
				Name = parts[0],
				BranchWithStatus = parts[1],
				Path = parts[2],
			};
		}

		public string Name { get; private set; }
		public string BranchWithStatus { get; private set; }
		public string Path { get; private set; }
	}
}
