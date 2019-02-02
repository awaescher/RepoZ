using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Api.Common
{
	public class HardcodededMiniHumanizer : IHumanizer
	{
		public HardcodededMiniHumanizer()
			: this(new SystemClock())
		{
		}

		public HardcodededMiniHumanizer(IClock clock)
		{
			Clock = clock ?? throw new ArgumentNullException(nameof(clock));
		}

		public string HumanizeTimestamp(DateTime value)
		{
			var diff = Clock.Now - value;

			var absoluteSeconds = Math.Abs(diff.TotalSeconds);
			var absoluteMinutes = Math.Abs(diff.TotalMinutes);
			var absoluteHours = Math.Abs(diff.TotalHours);
			var absoluteDays = Math.Abs(diff.TotalDays);

			// specials
			if (absoluteSeconds < 25)
				return "Just now";

			if (absoluteSeconds >= 55 && absoluteSeconds <= 80)
				return PastOrFuture("a minute", diff);

			if (absoluteSeconds > 80 && absoluteSeconds <= 100)
				return PastOrFuture("nearly two minutes", diff);

			if (absoluteMinutes >= 55 && absoluteMinutes <= 75)
				return PastOrFuture("an hour", diff);

			if (absoluteMinutes > 75 && absoluteMinutes <= 100)
				return PastOrFuture("one and a half hour", diff);

			if (absoluteHours >= 23 && absoluteHours <= 30)
				return PastOrFuture("a day", diff);

			// generic
			if (absoluteSeconds < 60)
				return PastOrFuture($"{Math.Round(absoluteSeconds)} seconds", diff);

			if (absoluteMinutes < 60)
				return PastOrFuture($"{Math.Round(absoluteMinutes)} minutes", diff);

			if (absoluteHours < 24)
				return PastOrFuture($"{Math.Round(absoluteHours)} hours", diff);

			if (absoluteDays >= 1.5 && absoluteDays < 5)
				return PastOrFuture($"{Math.Round(absoluteDays)} days", diff);

			// fallback
			return value.ToString("g");
		}

		private string PastOrFuture(string result, TimeSpan diff)
		{
			var value = diff.TotalMilliseconds > 0 ? result + " ago" : "in " + result;
			return value.Substring(0, 1).ToUpper() + value.Substring(1);
		}

		public IClock Clock { get; }
	}
}
