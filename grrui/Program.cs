namespace grrui
{
    using grrui.Model;
    using grrui.UI;
    using RepoZ.Ipc;
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Terminal.Gui;

    class Program
    {
        private const int BUTTON_BORDER = 4; // 2 chars to the left, 2 to the right
        private const int BUTTON_DISTANCE = 1;

        private static IpcClient _client;
        private static ListView _repositoryList;
        private static RepositoriesView _repositoriesView;
        private static TextField _filterField;

        static void Main(string[] args)
        {
            _client = new IpcClient(new DefaultIpcEndpoint());
            IpcClient.Result answer = _client.GetRepositories();

            var repositoryCount = answer?.Repositories?.Length ?? 0;
            if (repositoryCount == 0)
            {
                if (!string.IsNullOrEmpty(answer?.Answer))
                {
                    Console.WriteLine(answer.Answer);
                }
                else
                {
                    Console.WriteLine("No repositories yet");
                }

                return;
            }

            _repositoriesView = new RepositoriesView(answer.Repositories);

            Application.Init();

            var filterLabel = new Label(1, 1, "Filter: ");
            _filterField = new TextField("")
                {
                    X = Pos.Right(filterLabel) + 2,
                    Y = Pos.Top(filterLabel),
                    Width = Dim.Fill(margin: 1),
                };
            _filterField.Changed += FilterField_Changed;

            _repositoryList = new ListView(_repositoriesView.Repositories)
                {
                    X = Pos.Left(filterLabel),
                    Y = Pos.Bottom(filterLabel) + 1,
                    Width = Dim.Fill(margin: 1),
                    Height = Dim.Fill() - 2,
                };

            var win = new KeyPreviewWindow("grr: Git repositories of RepoZ")
                {
                    filterLabel,
                    _filterField,
                    _repositoryList,
                };

            var buttonX = Pos.Left(filterLabel);

            var navigationButton = new Button("Navigate")
                {
                    Clicked = Navigate,
                    X = buttonX,
                    Y = Pos.AnchorEnd(1),
                    CanFocus = false,
                };

            if (!CanNavigate)
            {
                navigationButton.Clicked = CopyNavigationCommandAndQuit;
            }

            buttonX = buttonX + navigationButton.Text.Length + BUTTON_BORDER + BUTTON_DISTANCE;
            var copyPathButton = new Button("Copy")
                {
                    Clicked = Copy,
                    X = buttonX,
                    Y = Pos.AnchorEnd(1),
                    CanFocus = false,
                };

            buttonX = buttonX + copyPathButton.Text.Length + BUTTON_BORDER + BUTTON_DISTANCE;
            var browseButton = new Button("Browse")
                {
                    Clicked = Browse,
                    X = buttonX,
                    Y = Pos.AnchorEnd(1),
                    CanFocus = false,
                };

            var quitButton = new Button("Quit")
                {
                    Clicked = Application.RequestStop,
                    X = Pos.AnchorEnd("Quit".Length + BUTTON_BORDER + BUTTON_DISTANCE),
                    Y = Pos.AnchorEnd(1),
                    CanFocus = false,
                };

            win.Add(navigationButton, copyPathButton, browseButton, quitButton);

            win.DefineKeyAction(Key.Enter, () => win.SetFocus(_repositoryList));
            win.DefineKeyAction(Key.Esc, () =>
                {
                    if (_filterField.HasFocus)
                    {
                        SetFilterText(string.Empty);
                    }
                    else
                    {
                        win.SetFocus(_filterField);
                    }
                });

            if (args?.Length > 0)
            {
                SetFilterText(string.Join(" ", args));
            }

            Application.Top.Add(win);
            Application.Run();
        }

        private static void SetFilterText(string filter)
        {
            _filterField.Text = filter;
            _filterField.PositionCursor();
            FilterField_Changed(_filterField, NStack.ustring.Empty);
        }

        private static void Navigate()
        {
            ExecuteOnSelectedRepository(r =>
                {
                    var command = $"cd \"{r.SafePath}\"";

                    // type the path into the console which is hosting grrui.exe to change to the directory
                    TextCopy.ClipboardService.SetText(command);
                    grr.ConsoleExtensions.WriteConsoleInput(Process.GetCurrentProcess(), command, waitMilliseconds: 1000);

                    Application.RequestStop();
                });
        }

        private static void CopyNavigationCommandAndQuit()
        {
            ExecuteOnSelectedRepository(r =>
                {
                    var command = $"cd \"{r.SafePath}\"";
                    TextCopy.ClipboardService.SetText(command);
                    TimelyMessage.ShowMessage("Copied to clipboard. Please paste and run the command manually now.", TimeSpan.FromMilliseconds(1000));
                    Application.RequestStop();
                });
        }

        private static void Copy()
        {
            ExecuteOnSelectedRepository(r =>
                {
                    var command = $"\"{r.SafePath}\"";
                    TextCopy.ClipboardService.SetText(command);
                });
        }

        private static void Browse()
        {
            ExecuteOnSelectedRepository(r =>
                {
                    Process.Start(new ProcessStartInfo(r.SafePath) { UseShellExecute = true, });
                });
        }

        private static void ExecuteOnSelectedRepository(Action<Repository> action)
        {
            RepositoryView[] repositories = _repositoriesView?.Repositories;
            if (!(repositories?.Length > _repositoryList.SelectedItem))
            {
                return;
            }

            RepositoryView current = repositories[_repositoryList.SelectedItem];
            action(current.Repository);
        }

        private static void FilterField_Changed(object sender, NStack.ustring e)
        {
            _repositoriesView.Filter = (sender as TextField)?.Text?.ToString() ?? "";
            _repositoryList.SetSource(_repositoriesView.Repositories);
        }

        private static bool CanNavigate => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
}