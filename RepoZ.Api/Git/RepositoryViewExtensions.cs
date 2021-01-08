using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace RepoZ.Api.Git
{
	public static class RepositoryViewExtensions
	{
		public static bool MatchesRegexFilter(this IRepositoryView repositoryView, string pattern) => MatchesFilter(repositoryView, pattern, useRegex: true);

		public static bool MatchesFilter(this IRepositoryView repositoryView, string filter) => MatchesFilter(repositoryView, filter, useRegex: false);

		private static bool MatchesFilter(IRepositoryView repositoryView, string filter, bool useRegex)
		{
			if (string.IsNullOrEmpty(filter))
				return true;

			if (filter.Replace(".*", "").Equals("todo", StringComparison.OrdinalIgnoreCase))
				return repositoryView.HasUnpushedChanges;

			string filterProperty = null;
			string[] lfilterProperty = null;

			// note, these are used in grr.RegexFilter as well
			if (filter.StartsWith("n ", StringComparison.OrdinalIgnoreCase))
				filterProperty = repositoryView.Name;
			else if (filter.StartsWith("b ", StringComparison.OrdinalIgnoreCase))
				filterProperty = repositoryView.CurrentBranch;
			else if (filter.StartsWith("p ", StringComparison.OrdinalIgnoreCase))
				filterProperty = repositoryView.Path;
			else if (filter.StartsWith("a ", StringComparison.OrdinalIgnoreCase))
				lfilterProperty = repositoryView.AllBranches;
			if (filterProperty == null && lfilterProperty == null)
				filterProperty = repositoryView.Name;
			else
				filter = filter.Substring(2);

			if (string.IsNullOrEmpty(filter))
				return true;

			if (lfilterProperty is string[])
			{
				bool matchFound = false;
				foreach (string branchName in lfilterProperty)
				{
					if (useRegex)
						matchFound = Regex.IsMatch(branchName, filter, RegexOptions.IgnoreCase);
					else
						matchFound = branchName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1;

					if (matchFound) return true;
				}
				return false;
			}
			else
			{
				if (useRegex)
					return Regex.IsMatch(filterProperty, filter, RegexOptions.IgnoreCase);
				return filterProperty.IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1;
			}
		}
	}
}
