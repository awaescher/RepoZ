namespace RepoZ.Api.Git
{
    public class StatusCharacterMap
    {
        public virtual string IdenticalSign => "\u2261";

        public virtual string NoUpstreamSign => "\u2302";

        public virtual string ArrowUpSign => "\u2191";

        public virtual string ArrowDownSign => "\u2193";

        public virtual string EllipsesSign => "\u2026";

        public virtual string StashSign => "\u205E";
    }
}