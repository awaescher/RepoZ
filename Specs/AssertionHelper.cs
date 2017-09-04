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
		public static void Expect(this DefaultRepositoryDetector detector, Action act, int changes, int deletes)
		{
			ExpectInternal(detector, act, out int actualChanges, out int actualDeletes);

			actualChanges.Should().Be(changes);
			actualDeletes.Should().Be(deletes);
		}

		public static void Expect(
			this DefaultRepositoryDetector detector,
			Action act,
			Func<int, bool> changesAssertion,
			Func<int, bool> deletesAssertion)
		{
			ExpectInternal(detector, act, out int actualChanges, out int actualDeletes);

			changesAssertion(actualChanges).Should().BeTrue();
			deletesAssertion(actualDeletes).Should().BeTrue();
		}

		private static void ExpectInternal(
			this DefaultRepositoryDetector detector,
			Action act,
			out int actualChanges,
			out int actualDeletes)
		{
			int trackedChanges = 0;
			int trackedDeletes = 0;

			try
			{
				detector.OnAddOrChange = (s) => trackedChanges++;
				detector.OnDelete = (s) => trackedDeletes++;

				detector.Start();

				act();

				// let's be generous
				var delay = 3 * detector.DetectionToAlertDelayMilliseconds;
				Thread.Sleep(delay);
			}
			finally
			{
				detector.Stop();

				detector.OnAddOrChange = null;
				detector.OnDelete = null;
			}

			actualChanges = trackedChanges;
			actualDeletes = trackedDeletes;
		}
	}
}
