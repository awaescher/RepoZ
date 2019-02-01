using RepoZ.Api.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Common
{
	public class FakeClock : IClock
	{
		public FakeClock(DateTime fakeValue)
		{
			FakeValue = fakeValue;
		}

		public DateTime Now => FakeValue;

		public DateTime FakeValue { get; }
	}
}
