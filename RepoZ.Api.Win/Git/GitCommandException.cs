namespace RepoZ.Api.Win.IO
{
    using System;
    using System.Diagnostics;

    public class GitCommandException : Exception
    {
        public Process Process { get; set; }

        public GitCommandException(string message, Process process)
            : base(message)
        {
            Process = process;
        }

        public GitCommandException() :
            base()
        {
        }

        public GitCommandException(string message)
            : base(message)
        {
        }

        public GitCommandException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected GitCommandException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}