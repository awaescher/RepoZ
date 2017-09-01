using FluentAssertions;
using RepoZ.Api.Common.Git;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Specs
{
	public static class AssertionHelper
	{
		public static void Expect(this DefaultRepositoryObserver observer, Action act, int changes, int deletes)
		{
			ExpectInternal(observer, act, out int actualChanges, out int actualDeletes);

			actualChanges.Should().Be(changes);
			actualDeletes.Should().Be(deletes);
		}

		public static void Expect(
			this DefaultRepositoryObserver observer,
			Action act,
			Func<int, bool> changesAssertion,
			Func<int, bool> deletesAssertion)
		{
			ExpectInternal(observer, act, out int actualChanges, out int actualDeletes);

			changesAssertion(actualChanges).Should().BeTrue();
			deletesAssertion(actualDeletes).Should().BeTrue();
		}

		private static void ExpectInternal(
			this DefaultRepositoryObserver observer,
			Action act,
			out int actualChanges,
			out int actualDeletes)
		{
			int trackedChanges = 0;
			int trackedDeletes = 0;

			try
			{
				observer.OnAddOrChange = (s) => trackedChanges++;
				observer.OnDelete = (s) => trackedDeletes++;

				observer.Observe();

				act();

				// let's be generous
				var delay = 3 * observer.DetectionToAlertDelayMilliseconds;
				Thread.Sleep(delay);
			}
			finally
			{
				observer.Stop();

				observer.OnAddOrChange = null;
				observer.OnDelete = null;
			}

			actualChanges = trackedChanges;
			actualDeletes = trackedDeletes;
		}
	}
}
