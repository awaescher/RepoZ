using System;
using RepoZ.Api.Common;
using System.Windows;

namespace RepoZ.UI.Win.Wpf
{
	internal class WpfThreadDispatcher : IThreadDispatcher
	{
		public void Invoke(Action act)
		{
			Application.Current.Dispatcher.Invoke(act);
		}
	}
}