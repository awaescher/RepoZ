using RepoZ.Ipc;
using System;

namespace grrui.Model
{
	public class RepositoryView
	{
		public RepositoryView(Repository repository)
		{
			Repository = repository ?? throw new ArgumentNullException(nameof(repository));
		}

		public override string ToString() => DisplayText ?? "";

		public Repository Repository { get; }

		public string DisplayText { get; set; }
	}
}