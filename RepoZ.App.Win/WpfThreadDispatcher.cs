namespace RepoZ.App.Win
{
    using System;
    using RepoZ.Api.Common;
    using System.Windows;

    internal class WpfThreadDispatcher : IThreadDispatcher
    {
        public void Invoke(Action act)
        {
            Application.Current.Dispatcher.Invoke(act);
        }
    }
}