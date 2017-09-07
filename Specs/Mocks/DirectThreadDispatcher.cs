using System;
using RepoZ.Api.Common;

namespace Specs
{
	internal class DirectThreadDispatcher : IThreadDispatcher
	{
		public void Invoke(Action act) => act();
	}
}