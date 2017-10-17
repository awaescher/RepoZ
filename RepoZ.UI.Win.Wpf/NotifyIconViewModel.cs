using System;
using System.Windows;
using System.Windows.Input;

namespace RepoZ.UI.Win.Wpf
{
	/// <summary>
	/// Provides bindable properties and commands for the NotifyIcon. In this sample, the
	/// view model is assigned to the NotifyIcon in XAML. Alternatively, the startup routing
	/// in App.xaml.cs could have created this view model, and assigned it to the NotifyIcon.
	/// </summary>
	public class NotifyIconViewModel
	{
		/// <summary>
		/// Shows a window, if none is already open.
		/// </summary>
		public ICommand OpenCommand
		{
			get
			{
				return new DelegateCommand
				{
					CanExecuteFunc = () => Application.Current.MainWindow == null,
					CommandAction = () =>
					{
						var window = TinyIoC.TinyIoCContainer.Current.Resolve<MainWindow>();
						Application.Current.MainWindow = window;
						Application.Current.MainWindow.Show();
					}
				};
			}
		}

		public ICommand StartWithWindows
		{
			get
			{
				return new DelegateCommand
				{
					CanExecuteFunc = () => !AutoStart.IsStartup("RepoZ"),
					CommandAction = () =>
					{
						AutoStart.SetStartup("RepoZ", true);
					}
				};
			}
		}

		public ICommand DoNotStartWithWindows
		{
			get
			{
				return new DelegateCommand
				{
					CanExecuteFunc = () => AutoStart.IsStartup("RepoZ"),
					CommandAction = () =>
					{
						AutoStart.SetStartup("RepoZ", false);
					}
				};
			}
		}

		/// <summary>
		/// Shuts down the application.
		/// </summary>
		public ICommand ExitApplicationCommand
		{
			get
			{
				return new DelegateCommand { CommandAction = () => Application.Current.Shutdown() };
			}
		}
	}

	/// <summary>
	/// Simplistic delegate command for the demo.
	/// </summary>
	public class DelegateCommand : ICommand
	{
		public Action CommandAction { get; set; }
		public Func<bool> CanExecuteFunc { get; set; }

		public void Execute(object parameter)
		{
			CommandAction();
		}

		public bool CanExecute(object parameter)
		{
			return CanExecuteFunc == null || CanExecuteFunc();
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}
	}
}
