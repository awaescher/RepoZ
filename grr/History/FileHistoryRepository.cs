using Microsoft.Win32;
using RepoZ.Ipc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grr.History
{
	public class FileHistoryRepository : IHistoryRepository
	{
		public void Save(State state)
		{
			// if multiple repositories were found the last time we ran grr,
			// these were written to the last state.
			// if the user selects one with an index like "grr cd :2", we want
			// to keep the last repositories to enable him to choose another one
			// with the same indexes as before.
			// so we have to get the old repositories - load and copy them if required
			if (!state.OverwriteRepositories)
			{
				var oldState = Load();
				if (oldState?.LastRepositories?.Length > 0)
					state.LastRepositories = oldState.LastRepositories;
			}

			var lines = new string[] { state?.LastLocation ?? "", Serialize(state?.LastRepositories ?? new Repository[0]) };

			try
			{
				File.WriteAllLines(GetFileName(), lines, Encoding.Default);
			}
			catch (Exception) { /* safely ignore this, saving the state is optional */ }
		}

		public State Load()
		{
			string[] lines = null;

			try
			{
				lines = File.ReadAllLines(GetFileName(), Encoding.Default);
			}
			catch { /* safely ignore this, reading the state is optional */ }

			if (lines?.Length != 2)
				return new State() { LastLocation = "", LastRepositories = new Repository[0] };

			return new State()
			{
				LastLocation = lines[0],
				LastRepositories = Deserialize(lines[1])
			};
		}

		private string GetFileName() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RepoZ", "state.grr");

		private string Serialize(IEnumerable<Repository> repositories)
		{
			if (repositories == null || !repositories.Any())
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
				.Select(s => new Repository() { Name = s })
				.ToArray();
		}
	}
}
