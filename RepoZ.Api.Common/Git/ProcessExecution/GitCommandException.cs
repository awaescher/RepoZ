using System;
using System.Diagnostics;

namespace RepoZ.Api.Common.Git.ProcessExecution
{
	public class GitCommandException : Exception
	{
		public GitCommandException(string message, Process process) : base(message)
		{
			Process = process;
		}

		public GitCommandException() : base()
		{
		}

		public GitCommandException(string message) : base(message)
		{
		}

		public GitCommandException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected GitCommandException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}

		public Process Process { get; set; }
	}
}