namespace grr
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static class RegexFilter
    {
        public static string Get(string value)
        {
            var usedPrefix = "";

            // respect filter targets like "n " for name, "b " for branches and "p " for paths
            var prefixes = new[] { "b ", "n ", "p ", }; // used in RepositoryView as well
            var isPrefixed = prefixes.Any(p => value.StartsWith(p, StringComparison.OrdinalIgnoreCase));

            if (isPrefixed)
            {
                usedPrefix = value.Substring(0, 2);
                value = value.Substring(2);
            }

            // square brackets [] define a RegEx to use. If they are not given, use a like search
            if (value.StartsWith("[") && value.EndsWith("]"))
            {
                return $"{usedPrefix}^{value.Substring(1, value.Length - 2)}$";
            }

            return $"{usedPrefix}.*{Regex.Escape(value)}.*";
        }
    }
}
