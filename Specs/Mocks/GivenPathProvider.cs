using RepoZ.Api.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Specs.Mocks
{
	internal class GivenPathProvider : IPathProvider
	{
		public GivenPathProvider(string[] paths)
		{
			Paths = paths;
		}

		public string[] Paths { get; set; }

		public string[] GetPaths() => Paths;
	}
}
