using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RepoZ.Api.IO;

namespace RepoZ.Api.Mac.IO
{
	public class MacPathSkipper : IPathSkipper
	{
        private string _userProfile;
        private List<string> _knownExclusions;

		public MacPathSkipper()
		{
			_userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

			_knownExclusions = new List<string>()
			{
				Path.Combine(_userProfile, "Library"),
				Path.Combine(_userProfile, "Movies"),
				Path.Combine(_userProfile, "Music"),
				Path.Combine(_userProfile, "Pictures")
			};
		}

		public bool ShouldSkip(string path)
		{
			// skip hidden folders in the users profile (like .config, etc.)
			if (path.IndexOf(_userProfile, StringComparison.OrdinalIgnoreCase) > -1)
			{
				var nameInUserProfile = path.ToLowerInvariant().Replace(_userProfile.ToLowerInvariant(), "");
				var isHiddenByName = nameInUserProfile.StartsWith("/.");
				if (isHiddenByName)
					return true;
			}

			return _knownExclusions.Any(ex => path.IndexOf(ex, StringComparison.OrdinalIgnoreCase) > -1);
		}
	}
}
