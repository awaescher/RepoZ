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
		public static void Expect(this DefaultRepositoryMonitor monitor, Action act, int changes, int deletes)
		{
			ExpectInternal(monitor, act, out int actualChanges, out int actualDeletes);

			actualChanges.Should().Be(changes);
			actualDeletes.Should().Be(deletes);
		}

		public static void Expect(
			this DefaultRepositoryMonitor monitor,
			Action act,
			Func<int, bool> changesAssertion,
			Func<int, bool> deletesAssertion)
		{
			ExpectInternal(monitor, act, out int actualChanges, out int actualDeletes);

			changesAssertion(actualChanges).Should().BeTrue();
			deletesAssertion(actualDeletes).Should().BeTrue();
		}

		private static void ExpectInternal(
			this DefaultRepositoryMonitor monitor,
			Action act,
			out int actualChanges,
			out int actualDeletes)
		{
			int trackedChanges = 0;
			int trackedDeletes = 0;

			try
			{
				monitor.OnChangeDetected = (r) => trackedChanges++;
				monitor.OnDeletionDetected = (s) => trackedDeletes++;

				monitor.Observe();

				act();

				// let's be generous
				var delay = 3 * monitor.DelayGitStatusAfterFileOperationMilliseconds;
				Thread.Sleep(delay);
			}
			finally
			{
				monitor.Stop();

				monitor.OnChangeDetected = null;
				monitor.OnDeletionDetected = null;
			}

			actualChanges = trackedChanges;
			actualDeletes = trackedDeletes;
		}
	}
}
