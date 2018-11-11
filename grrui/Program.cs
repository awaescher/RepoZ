using grrui.Model;
using grrui.UI;
using RepoZ.Api.Git;
using RepoZ.Ipc;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Terminal.Gui;

namespace grrui
{
	class Program
	{
		private static RepozIpcClient _client;
		private static ListView _repositoryList;
		private static RepositoriesView _repositoriesView;

		static void Main(string[] args)
		{
			_client = new RepozIpcClient();
			var answer = _client.GetRepositories();

			if (answer.Repositories == null)
			{
				Console.WriteLine(answer.Answer);
				return;
			}

			if (answer.Repositories.Length == 0)
			{
				Console.WriteLine("No repositories yet");
				return;
			}

			_repositoriesView = new RepositoriesView(answer.Repositories);

			Application.Init();

			var filterLabel = new Label(1, 1, "Filter: ");
			var filterField = new TextField("")
			{
				X = Pos.Right(filterLabel) + 2,
				Y = Pos.Top(filterLabel),
				Width = Dim.Fill(margin: 1),
			};
			filterField.Changed += FilterField_Changed;

			_repositoryList = new ListView(_repositoriesView.Repositories)
			{
				X = Pos.Left(filterLabel),
				Y = Pos.Bottom(filterLabel) + 1,
				Width = Dim.Fill(margin: 1),
				Height = Dim.Fill()
			};
			
			var win = new KeyPreviewWindow("grr: Git repositories of RepoZ");
			win.Add(filterLabel);
			win.Add(filterField);
			win.Add(_repositoryList);

			win.DefineKeyAction(Key.Enter, () =>
			{
				if (_repositoryList.HasFocus)
				{
					var repositories = _repositoriesView.Repositories;
					if (repositories?.Length > _repositoryList.SelectedItem)
					{
						var current = _repositoriesView.Repositories[_repositoryList.SelectedItem];

						//Process.Start(new ProcessStartInfo(current.Repository.Path) { UseShellExecute = true });
						// use '/' for linux systems and bash command line (will work on cmd and powershell as well)
						var path = current.Repository.Path.Replace(@"\", "/");
						var command = $"cd \"{path}\"";

						TextCopy.Clipboard.SetText(command);
						TimelyMessage.ShowMessage("Path copied to clipboard", TimeSpan.FromMilliseconds(100));

						Application.RequestStop();
					}
				}
				else
				{
					win.SetFocus(_repositoryList);
				}
			});

			Application.Top.Add(win);
			Application.Run();
		}

		private static void FilterField_Changed(object sender, EventArgs e)
		{
			_repositoriesView.Filter = (sender as TextField)?.Text?.ToString() ?? "";
			_repositoryList.SetSource(_repositoriesView.Repositories);
		}
	}
}