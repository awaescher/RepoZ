using System.IO;

namespace grr.Messages
{
	[System.Diagnostics.DebuggerDisplay("{GetRemoteCommand()}")]
	public abstract class DirectoryMessage : IMessage
	{
		private readonly string _argument;
		private readonly bool _argumentIsExistingDirectory;

		public DirectoryMessage(string argument)
		{
			_argument = argument;
			_argumentIsExistingDirectory = Directory.Exists(_argument);
		}

		public void Execute(Repository[] repositories)
		{
			if (_argumentIsExistingDirectory)
				ExecuteExistingDirectory(_argument);
			else
				ExecuteRepositoryQuery(repositories);
		}

		protected abstract void ExecuteExistingDirectory(string directory);

		protected abstract void ExecuteRepositoryQuery(Repository[] repositories);

		public virtual string GetRemoteCommand()
		{
			if (!HasRemoteCommand)
				return null;

			return string.IsNullOrEmpty(_argument)
				? null /* makes no sense */
				: $"list:^{_argument}$";
		}

		public virtual bool HasRemoteCommand
		{
			get
			{
				if (_argumentIsExistingDirectory)
					return false;

				return !string.IsNullOrEmpty(_argument);
			}
		}

	}
}
