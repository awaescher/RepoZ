namespace RepoZ.Ipc
{
	[System.Diagnostics.DebuggerDisplay("{Name}")]
	public class Repository
	{
		public static Repository FromString(string value)
		{
			var parts = value?.Split('|');
			if (parts?.Length != 3)
				return null;

			return new Repository()
			{
				Name = parts[0],
				BranchWithStatus = parts[1],
				Path = parts[2],
			};
		}

		public string Name { get; set; }

		public string BranchWithStatus { get; set; }

		public string Path { get; set; }
	}
}
