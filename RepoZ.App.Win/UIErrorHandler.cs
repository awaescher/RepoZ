namespace RepoZ.App.Win
{
    using System.Windows;
    using RepoZ.Api.Common;

    public class UIErrorHandler : IErrorHandler
    {
        public void Handle(string error)
        {
            MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}