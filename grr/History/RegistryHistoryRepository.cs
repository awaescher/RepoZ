using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grr.History
{
	public class RegistryHistoryRepository
	{
		public static string RegistryPath { get; } = @"Software\RepoZ\grr\";

		public void Save(State state)
		{
			var key = Registry.CurrentUser.OpenSubKey(RegistryPath, true);
			if (key == null)
				key = Registry.CurrentUser.CreateSubKey(RegistryPath, RegistryKeyPermissionCheck.ReadWriteSubTree);

			key.SetValue("LastLocation", state.LastLocation, RegistryValueKind.String);
			key.SetValue("LastRepositories", Serialize(state.LastRepositories), RegistryValueKind.String);
			key.Close();
		}

		public State Load()
		{
			var key = Registry.CurrentUser.OpenSubKey(RegistryPath);
			if (key == null)
				return null;

			string location = (string)key.GetValue("LastLocation");
			string repositories = (string)key.GetValue("LastRepositories");
			key.Close();

			return new State() {
				LastLocation = location,
				LastRepositories = Deserialize(repositories)
			};
		}

		private string Serialize(IEnumerable<Repository> repositories)
		{
			if (repositories?.Any() ?? false)
				return "";

			var names = repositories
					.Select(r => r.Name)
					.ToArray();

			return string.Join("|", names);
		}

		private Repository[] Deserialize(string repositoryString)
		{
			if (string.IsNullOrEmpty(repositoryString))
				return new Repository[0];

			return repositoryString.Split(new string[] { "|" }, StringSplitOptions.None)
				.Select(s => new Repository())
				.ToArray();
		}
	}
}
