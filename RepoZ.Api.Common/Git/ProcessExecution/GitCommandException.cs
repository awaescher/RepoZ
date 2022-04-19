namespace RepoZ.Api.Common.Git.ProcessExecution
{
    using System;

    public class GitCommandException : Exception
    {
        public GitCommandException() : base() { }

        public GitCommandException(string message) : base(message) { }

        public GitCommandException(string message, Exception innerException) : base(message, innerException) { }

        protected GitCommandException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}