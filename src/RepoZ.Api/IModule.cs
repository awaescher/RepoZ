namespace RepoZ.Api
{
    using System.Threading.Tasks;

    public interface IModule
    {
        Task StartAsync();

        Task StopAsync();
    }
}