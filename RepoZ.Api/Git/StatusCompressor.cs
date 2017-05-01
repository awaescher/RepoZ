using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepoZ.Api.Git;

namespace RepoZ.Api.Git
{
	public class StatusCompressor
	{
		private StatusCharacterMap _statusCharakterMap;

		public StatusCompressor(StatusCharacterMap statusCharacterMap)
		{
			_statusCharakterMap = statusCharacterMap;
		}

		public string Compress(Repository repository)
		{
			if (repository == null)
				return string.Empty;

			if (string.IsNullOrEmpty(repository.CurrentBranch))
				return string.Empty;

			var printASR = (repository.LocalAdded ?? 0) + (repository.LocalStaged ?? 0) + (repository.LocalRemoved ?? 0) > 0;
			var printUMM = (repository.LocalUntracked ?? 0) + (repository.LocalModified ?? 0) + (repository.LocalMissing ?? 0) > 0;

			var builder = new StringBuilder();

			var isAhead = (repository.AheadBy ?? 0) > 0;
			var isBehind = (repository.BehindBy ?? 0) > 0;
			var isOnCommitLevel = !isAhead && !isBehind;

			if (repository.CurrentBranchHasUpstream)
			{
				if (isOnCommitLevel)
				{
					builder.Append(_statusCharakterMap.IdenticalSign);
				}
				else
				{
					if (isAhead)
						builder.Append($"{_statusCharakterMap.ArrowUpSign}{repository.AheadBy.Value}");

					if (isBehind)
					{
						if (isAhead)
							builder.Append(" ");
						builder.Append($"{_statusCharakterMap.ArrowDownSign}{repository.BehindBy.Value}");
					}
				}
			}
			else
			{
				builder.Append(_statusCharakterMap.NoUpstreamSign);
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

			return builder.ToString();
		}
	}
}
