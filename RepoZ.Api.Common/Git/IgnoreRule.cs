using System;

namespace RepoZ.Api.Common.Git
{
	public class IgnoreRule
	{
		private readonly Func<string, bool> _comparer;

		public IgnoreRule(string pattern)
		{
			var wildcardStart = pattern.StartsWith("*");
			var wildcardEnd = pattern.EndsWith("*");

			if (wildcardStart || wildcardEnd)
			{
				if (wildcardStart)
					pattern = pattern.Substring(1);
				if (wildcardEnd)
					pattern = pattern.Substring(0, pattern.Length - 1);

				if (wildcardStart && wildcardEnd)
				{
					_comparer = path => path.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;
				}
				else
				{
					if (wildcardStart)
						_comparer = path => path.EndsWith(pattern, StringComparison.OrdinalIgnoreCase);
					else
						_comparer = path => path.StartsWith(pattern, StringComparison.OrdinalIgnoreCase);
				}
			}
			else
			{
				_comparer = path => string.Equals(path, pattern, StringComparison.OrdinalIgnoreCase);
			}
		}

		public bool IsIgnored(string path) => _comparer(path);
	}


}
