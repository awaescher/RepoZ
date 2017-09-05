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
		private readonly string _path;

		public GivenPathProvider(string path)
		{
			_path = path;
		}

		public string[] GetPaths() => new string[] { _path };
	}
}
