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

			if (Math.Abs(diff.TotalSeconds) < 25)
				return "Just now";

			if (diff.TotalMinutes >= -0.8)
				return $"{Math.Abs(Math.Round(diff.TotalSeconds))} seconds ago";

			if (diff.TotalMinutes < -0.8 && diff.TotalMinutes > -1.5)
				return "A minute ago";

			if (diff.TotalHours > -0.8)
				return $"{Math.Abs(Math.Round(diff.TotalMinutes))} minutes ago";

			if (diff.TotalHours < -0.8 && diff.TotalHours > -1.2)
				return "An hour ago";

			if (diff.TotalHours <= -1.2 && diff.TotalHours > -1.8)
				return $"{Math.Abs(Math.Round(diff.TotalMinutes))} minutes ago";

			if (diff.TotalHours <= -1.8 && diff.TotalDays > -0.95)
				return $"{Math.Abs(Math.Round(diff.TotalHours))} hours ago";

			if (diff.TotalDays < -0.95 && diff.TotalDays > -1.5)
				return "A day ago";

			if (diff.TotalDays <= -1.5 && diff.TotalDays > -5)
				return $"{Math.Abs(Math.Round(diff.TotalDays))} days ago";

			return value.ToString("g");
		}

		public IClock Clock { get; }
	}
}