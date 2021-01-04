using System.Text;

namespace RepoZ.Api.Git
{
	public class StatusCompressor
	{
		private const int COMMIT_SHA_DISPLAY_CHARS = 7;

		private readonly StatusCharacterMap _statusCharacterMap;

		public StatusCompressor(StatusCharacterMap statusCharacterMap)
		{
			_statusCharacterMap = statusCharacterMap;
		}

		public string Compress(Repository repository)
		{
			if (repository == null)
				return string.Empty;

			if (string.IsNullOrEmpty(repository.CurrentBranch))
				return string.Empty;

			var printASR = (repository.LocalAdded ?? 0) + (repository.LocalStaged ?? 0) + (repository.LocalRemoved ?? 0) > 0;
			var printUMM = (repository.LocalUntracked ?? 0) + (repository.LocalModified ?? 0) + (repository.LocalMissing ?? 0) > 0;
			var printStashCount = (repository.StashCount ?? 0) > 0;

			var builder = new StringBuilder();

			var isAhead = (repository.AheadBy ?? 0) > 0;
			var isBehind = (repository.BehindBy ?? 0) > 0;
			var isOnCommitLevel = !isAhead && !isBehind;

			if (repository.CurrentBranchHasUpstream)
			{
				if (isOnCommitLevel)
				{
					builder.Append(_statusCharacterMap.IdenticalSign);
				}
				else
				{
					if (isBehind)
						builder.Append($"{_statusCharacterMap.ArrowDownSign}{repository.BehindBy.Value}");

					if (isAhead)
					{
						if (isBehind)
							builder.Append(" ");

						builder.Append($"{_statusCharacterMap.ArrowUpSign}{repository.AheadBy.Value}");
					}
				}
			}
			else
			{
				builder.Append(_statusCharacterMap.NoUpstreamSign);
			}

			if (printASR)
			{
				if (builder.Length > 0)
					builder.Append(" ");

				builder.AppendFormat("+{0} ~{1} -{2}", repository.LocalAdded ?? 0, repository.LocalStaged ?? 0, repository.LocalRemoved ?? 0);
			}

			if (printUMM)
			{
				if (builder.Length > 0)
					builder.Append(" ");

				if (printASR)
					builder.Append("| ");

				builder.AppendFormat("+{0} ~{1} -{2}", repository.LocalUntracked ?? 0, repository.LocalModified ?? 0, repository.LocalMissing ?? 0);
			}

			if (printStashCount)
			{
				if (builder.Length > 0)
					builder.Append(" ");

				builder.Append(_statusCharacterMap.StashSign + repository.StashCount.ToString());
			}

			return builder.ToString();
		}

		public string CompressWithBranch(Repository repository)
		{
			string branch = repository.CurrentBranch;

			if (repository.CurrentBranchIsOnTag)
			{
				// put tabs in parenthesis ()
				branch = $"({branch})";
			}
			else
			{
				// put commit shas in parenthesis (), shorten them and show ellipses afterwards
				if (repository.CurrentBranchIsDetached && branch.Length > COMMIT_SHA_DISPLAY_CHARS)
					branch = $"({branch.Substring(0, COMMIT_SHA_DISPLAY_CHARS)}{_statusCharacterMap.EllipsesSign})";
			}

			return branch + " " + Compress(repository);
		}
	}
}
