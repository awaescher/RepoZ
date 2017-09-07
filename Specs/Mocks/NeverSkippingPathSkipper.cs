using RepoZ.Api.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Specs.Mocks
{
	internal class NeverSkippingPathSkipper : IPathSkipper
	{
		public bool ShouldSkip(string path) => false;
	}
}
