namespace RepoZ.Api.Common
{
    using System;

    public class SystemClock : IClock
    {
        public DateTime Now => DateTime.Now;
    }
}