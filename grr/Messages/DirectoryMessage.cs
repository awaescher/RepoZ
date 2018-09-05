using System.IO;
using System.Linq;

namespace grr.Messages
{
	[System.Diagnostics.DebuggerDisplay("{GetRemoteCommand()}")]
	public abstract class DirectoryMessage : IMessage
	{
		private readonly bool _argumentIsExistingDirectory;

		public DirectoryMessage(RepositoryFilterOptions filter)
		{
			Filter = filter;
			_argumentIsExistingDirectory = Directory.Exists(Filter.RepositoryFilter);
		}

		public void Execute(Repository[] repositories)
		{
			if (_argumentIsExistingDirectory)
				ExecuteExistingDirectory(Filter.RepositoryFilter);
			else
				ExecuteRepositoryQuery(repositories);
		}

		protected abstract void ExecuteExistingDirectory(string directory);

		protected virtual void ExecuteRepositoryQuery(Repository[] repositories)
		{
			if (repositories == null || repositories.Length <= 0)
				return;

			foreach (var repository in repositories)
			{
				var directory = repository.Path;

				if (string.IsNullOrWhiteSpace(directory))
				{
					System.Console.WriteLine("Repository path is empty. Aborting.");
					return;
				}

				if (Directory.Exists(directory))
					ExecuteExistingDirectory(directory);
				else
					System.Console.WriteLine("Repository path does not exist:\n" + directory);
			}
		}

		public virtual string GetRemoteCommand()
		{
			if (!HasRemoteCommand)
				return null;

			return string.IsNullOrEmpty(Filter?.RepositoryFilter)
				? null /* makes no sense */
				: $"list:.*{Filter.RepositoryFilter}.*";
		}

		public virtual bool HasRemoteCommand
		{
			get
			{
				if (_argumentIsExistingDirectory)
					return false;

				return !string.IsNullOrEmpty(Filter?.RepositoryFilter);
			}
		}

		public RepositoryFilterOptions Filter { get; }
	}
}
