using FluentAssertions;
using NUnit.Framework;
using RepoZ.Api.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Common
{
	public class HardcodedMiniHumanizerTests
	{
		private FakeClock _clock;
		private IHumanizer _humanizer;

		[SetUp]
		public void Setup()
		{
			_clock = new FakeClock(new DateTime(2020, 01, 01, 10, 00, 00));
			_humanizer = new HardcodededMiniHumanizer(_clock);
		}

		public class HumanizeTimestampMethod : HardcodedMiniHumanizerTests
		{
			[Test]
			public void Returns_Speaking_Now()
			{
				var value = _humanizer.HumanizeTimestamp(_clock.Now);
				value.Should().Be("Just now");
			}

			[TestCase(15, "Just now")]
			[TestCase(20, "Just now")]
			[TestCase(30, "30 seconds ago")]
			[TestCase(30.4123123, "30 seconds ago")]
			[TestCase(45, "45 seconds ago")]
			[TestCase(50, "A minute ago")]
			[TestCase(60, "A minute ago")]
			[TestCase(89, "A minute ago")]
			[TestCase(90, "2 minutes ago")]
			[TestCase(300, "5 minutes ago")]
			public void Seconds(double seconds, string expected)
			{
				var value = _humanizer.HumanizeTimestamp(_clock.Now.AddSeconds(seconds));
				value.Should().Be(expected);
			}

			[TestCase(15, "15 minutes ago")]
			[TestCase(30.4123123, "30 minutes ago")]
			[TestCase(45, "45 minutes ago")]
			[TestCase(57, "An hour ago")]
			[TestCase(60, "An hour ago")]
			[TestCase(63, "An hour ago")]
			[TestCase(89, "89 minutes ago")]
			[TestCase(90, "90 minutes ago")]
			[TestCase(110, "2 hours ago")]
			[TestCase(120, "2 hours ago")]
			[TestCase(130, "2 hours ago")]
			[TestCase(300, "5 hours ago")]
			public void Minutes(double minutes, string expected)
			{
				var value = _humanizer.HumanizeTimestamp(_clock.Now.AddMinutes(minutes));
				value.Should().Be(expected);
			}

			[TestCase(0, "Just now")]
			[TestCase(12, "12 hours ago")]
			[TestCase(12.4123123, "12 hours ago")]
			[TestCase(22, "22 hours ago")]
			[TestCase(23, "A day ago")]
			[TestCase(24, "A day ago")]
			[TestCase(30, "A day ago")]
			[TestCase(40, "2 days ago")]
			[TestCase(50, "2 days ago")]
			[TestCase(60, "2 days ago")]
			[TestCase(62, "3 days ago")]
			[TestCase(300, "13.01.2020 22:00")]
			public void Hours(double hours, string expected)
			{
				var value = _humanizer.HumanizeTimestamp(_clock.Now.AddHours(hours));
				value.Should().Be(expected);
			}
		}
	}
}
