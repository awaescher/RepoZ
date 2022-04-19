namespace RepoZ.Api.Common
{
    public interface IErrorHandler
    {
        void Handle(string error);
    }
}