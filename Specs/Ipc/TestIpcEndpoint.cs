using RepoZ.Ipc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Specs.Ipc
{
	class TestIpcEndpoint : IIpcEndpoint
	{
		public string Address => "tcp://localhost:18182";
	}
}
