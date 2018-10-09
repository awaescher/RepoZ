using System.Text.RegularExpressions;

namespace grr
{
	public static class RegexFilter
	{
		public static string Get(string value)
		{
			// parentheses define a RegEx to use. If they are not given, use a like search

			if (value.StartsWith("(") && value.EndsWith(")"))
				return $"^{value.Substring(1, value.Length - 2)}$";

			return $".*{Regex.Escape(value)}.*";
		}
	}
}
