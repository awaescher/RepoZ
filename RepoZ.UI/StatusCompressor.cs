using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepoZ.Api.Git;

namespace RepoZ.UI
{
	public static class StatusCompressor
	{
		const string SIGN_IDENTICAL = "\u2261";
		const string SIGN_ARROW_UP = "\u2191";
		const string SIGN_ARROW_DOWN = "\u2193";

		public static string Compress(Repository repository)
		{
			if (repository == null)
				return string.Empty;

			var printASR = (repository.LocalAdded ?? 0) + (repository.LocalStaged ?? 0) + (repository.LocalRemoved ?? 0) > 0;
			var printUMM = (repository.LocalUntracked ?? 0) + (repository.LocalModified ?? 0) + (repository.LocalMissing ?? 0) > 0;

			var builder = new StringBuilder();

			var isAhead = (repository.AheadBy ?? 0) > 0;
			var isBehind = (repository.BehindBy ?? 0) > 0;
			var isOnCommitLevel = !isAhead && !isBehind;

			if (isOnCommitLevel)
			{
				builder.Append(SIGN_IDENTICAL);
			}
			else
			{
				if (isAhead)
					builder.Append($"{SIGN_ARROW_UP}{repository.AheadBy.Value}");

				if (isBehind)
				{
					if (isAhead)
						builder.Append(" ");
					builder.Append($"{SIGN_ARROW_DOWN}{repository.BehindBy.Value}");
				}
			}

			if (printASR)
			{
				builder.AppendFormat(" +{0} ~{1} -{2}", repository.LocalAdded ?? 0, repository.LocalStaged ?? 0, repository.LocalRemoved ?? 0);
			}

			if (printUMM)
			{
				if (printASR)
					builder.Append(" |");

				builder.AppendFormat(" +{0} ~{1} -{2}", repository.LocalUntracked ?? 0, repository.LocalModified ?? 0, repository.LocalMissing ?? 0);
			}

			return builder.ToString();
		}
	}
}
