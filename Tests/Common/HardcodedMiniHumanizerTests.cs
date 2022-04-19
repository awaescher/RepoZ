namespace Tests.Common
{
    using FluentAssertions;
    using NUnit.Framework;
    using RepoZ.Api.Common;
    using System;

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

            [TestCase(0, "Just now")]
            [TestCase(-20, "Just now")]
            [TestCase(20, "Just now")]
            [TestCase(-30, "30 seconds ago")]
            [TestCase(30, "In 30 seconds")]
            [TestCase(-30.4123123, "30 seconds ago")]
            [TestCase(30.4123123, "In 30 seconds")]
            [TestCase(-45, "45 seconds ago")]
            [TestCase(45, "In 45 seconds")]
            [TestCase(-55, "A minute ago")]
            [TestCase(55, "In a minute")]
            [TestCase(-60, "A minute ago")]
            [TestCase(60, "In a minute")]
            [TestCase(-89, "Nearly two minutes ago")]
            [TestCase(89, "In nearly two minutes")]
            [TestCase(-90, "Nearly two minutes ago")]
            [TestCase(90, "In nearly two minutes")]
            [TestCase(-101, "2 minutes ago")]
            [TestCase(101, "In 2 minutes")]
            [TestCase(-300, "5 minutes ago")]
            [TestCase(300, "In 5 minutes")]
            public void Seconds(double seconds, string expected)
            {
                var value = _humanizer.HumanizeTimestamp(_clock.Now.AddSeconds(seconds));
                value.Should().Be(expected);
            }

            [TestCase(-15, "15 minutes ago")]
            [TestCase(15, "In 15 minutes")]
            [TestCase(-30.4123123, "30 minutes ago")]
            [TestCase(30.4123123, "In 30 minutes")]
            [TestCase(-45, "45 minutes ago")]
            [TestCase(45, "In 45 minutes")]
            [TestCase(-57, "An hour ago")]
            [TestCase(57, "In an hour")]
            [TestCase(-60, "An hour ago")]
            [TestCase(60, "In an hour")]
            [TestCase(-71, "An hour ago")]
            [TestCase(71, "In an hour")]
            [TestCase(-89, "One and a half hour ago")]
            [TestCase(89, "In one and a half hour")]
            [TestCase(-90, "One and a half hour ago")]
            [TestCase(90, "In one and a half hour")]
            [TestCase(-110, "2 hours ago")]
            [TestCase(110, "In 2 hours")]
            [TestCase(-120, "2 hours ago")]
            [TestCase(120, "In 2 hours")]
            [TestCase(-130, "2 hours ago")]
            [TestCase(130, "In 2 hours")]
            [TestCase(-300, "5 hours ago")]
            [TestCase(300, "In 5 hours")]
            public void Minutes(double minutes, string expected)
            {
                var value = _humanizer.HumanizeTimestamp(_clock.Now.AddMinutes(minutes));
                value.Should().Be(expected);
            }

            [TestCase(-0, "Just now")]
            [TestCase(-12, "12 hours ago")]
            [TestCase(12, "In 12 hours")]
            [TestCase(-12.4123123, "12 hours ago")]
            [TestCase(12.4123123, "In 12 hours")]
            [TestCase(-22, "22 hours ago")]
            [TestCase(22, "In 22 hours")]
            [TestCase(-23, "A day ago")]
            [TestCase(23, "In a day")]
            [TestCase(-24, "A day ago")]
            [TestCase(24, "In a day")]
            [TestCase(-30, "A day ago")]
            [TestCase(30, "In a day")]
            [TestCase(-40, "2 days ago")]
            [TestCase(40, "In 2 days")]
            [TestCase(-50, "2 days ago")]
            [TestCase(50, "In 2 days")]
            [TestCase(-60, "2 days ago")]
            [TestCase(60, "In 2 days")]
            [TestCase(-62, "3 days ago")]
            [TestCase(62, "In 3 days")]
            public void Hours(double hours, string expected)
            {
                var value = _humanizer.HumanizeTimestamp(_clock.Now.AddHours(hours));
                value.Should().Be(expected);
            }

            [Test]
            public void Date_Fallback()
            {
                var value = _humanizer.HumanizeTimestamp(_clock.Now.AddHours(-300));
                value.Should().Be(new DateTime(2019, 12, 19, 22, 00, 00).ToString("g"));

                value = _humanizer.HumanizeTimestamp(_clock.Now.AddHours(300));
                value.Should().Be(new DateTime(2020, 01, 13, 22, 00, 00).ToString("g"));
            }
        }
    }
}