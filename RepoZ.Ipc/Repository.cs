namespace RepoZ.Ipc
{
    [System.Diagnostics.DebuggerDisplay("{Name}")]
    public class Repository
    {
        public static Repository FromString(string value)
        {
            var parts = value?.Split(new string[] { "::" }, System.StringSplitOptions.None);
            if (parts?.Length != 3)
                return null;

            return new Repository()
            {
                Name = parts[0],
                BranchWithStatus = parts[1],
                Path = parts[2],
            };
        }

		public override string ToString()
		{
			if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(BranchWithStatus) || string.IsNullOrEmpty(Path))
				return "";

			return $"{Name}::{BranchWithStatus}::{Path}";
		}

		public string Name { get; set; }

        public string BranchWithStatus { get; set; }

        public string Path { get; set; }

        public string SafePath
        {
            // use '/' for linux systems and bash command line (will work on cmd and powershell as well)
            get => Path?.Replace(@"\", "/") ?? "";
        }
	}
}
