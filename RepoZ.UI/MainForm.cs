using System;
using Eto.Forms;
using Eto.Drawing;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using RepoZ.Api.Git;
using RepoZ.Api.IO;
using System.Linq;
using System.Text;

namespace RepoZ.UI
{
	public class MainForm : Form, IRepositoryInformationAggregator
	{
		private IRepositoryMonitor _repositoryMonitor;
		private ObservableCollection<RepositoryView> _dataSource = new ObservableCollection<RepositoryView>();
		private IRepositoryActionProvider _repositoryActionProvider;

		public MainForm(IRepositoryMonitor repositoryMonitor, IRepositoryActionProvider repositoryActionProvider)
		{
			_repositoryMonitor = repositoryMonitor;
			_repositoryMonitor.OnChangeDetected = (repo) => notifyRepoChange(repo);
			_repositoryMonitor.OnDeletionDetected = (repoPath) => notifyRepoDeletion(repoPath);
			_repositoryMonitor.Observe();

			_repositoryActionProvider = repositoryActionProvider;
		
			Title = "RepoZ";
			Maximizable = false;
			ClientSize = new Size(1025, 600);

			createGrid();
		}

		private void createGrid()
		{
			var filtered = new FilterCollection<RepositoryView>(_dataSource);
			filtered.Sort = (x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal);

			var grid = new GridView { DataStore = filtered };

			grid.Columns.Add(new GridColumn()
			{
				DataCell = new TextBoxCell(nameof(RepositoryView.Name)),
				Sortable = true,
				Width = 200,
				HeaderText = "Repository",
			});

			grid.Columns.Add(new GridColumn()
			{
				DataCell = new TextBoxCell(nameof(RepositoryView.CurrentBranch)),
				Sortable = true,
				Width = 200,
				HeaderText = "Current Branch"
			});

			grid.Columns.Add(new GridColumn()
			{
				DataCell = new TextBoxCell(nameof(RepositoryView.Path)),
				Sortable = true,
				Width = 400,
				HeaderText = "Location"
			});

			grid.Columns.Add(new GridColumn()
			{
				DataCell = new TextBoxCell(nameof(RepositoryView.Status)),
				Sortable = true,
				Width = 200,
				HeaderText = "Status"
			});

			grid.CellDoubleClick += Grid_CellDoubleClick;
			grid.MouseUp += Grid_MouseUp;

			Content = grid;
		}

		private void Grid_MouseUp(object sender, MouseEventArgs e)
		{
			var view = sender as GridView;

			if (view != null && e.Buttons == MouseButtons.Alternate)
			{
				var repo = view.SelectedItem as RepositoryView;

				if (repo == null || !repo.WasFound)
					return;

                var items = new List<MenuItem>();

                foreach (var action in _repositoryActionProvider.GetFor(repo.Repository))
                {
                    if (action.BeginGroup && items.Any())
                        items.Add(new SeparatorMenuItem());

					var location = this.PointToScreen(e.Location);
					location.Offset(-9, -32); // seems to be the offset of the titlebar --> TODO detect
					float[] coords = { location.X, location.Y };

					items.Add(CreateMenuItem(sender, action, coords));
                }
                
				var menu = new ContextMenu(items);
				menu.Show(Content);
			}
		}

		private MenuItem CreateMenuItem(object sender, RepositoryAction action, float[] coords)
		{
			Action clickAction = () =>
			{
				if (action?.Action != null)
					action.Action(sender, coords);
			};

			var item = new ButtonMenuItem((s, e) => clickAction())
			{
				Text = action.Name,
				Enabled = action.CanExecute
			} ;

			if (action.SubActions != null)
			{
				foreach (var subAction in action.SubActions)
					item.Items.Add(CreateMenuItem(sender, subAction, coords));
			}

			return item;
		}

		private void Grid_CellDoubleClick(object sender, GridViewCellEventArgs e)
		{
			var repo = e.Item as RepositoryView;
			if (repo == null || !repo.WasFound)
				return;

			var action = _repositoryActionProvider.GetFor(repo.Repository)
						 .FirstOrDefault(a => a.IsDefault);

			action?.Action?.Invoke(sender, e);
		}

		private void notifyRepoChange(Repository repo)
		{
			Application.Instance.Invoke(() =>
			{
				try
				{
					var view = new RepositoryView(repo);
					_dataSource.Remove(view);
					_dataSource.Add(view);
				}
				catch (Exception)
				{
					// happened to be swallowed by Eto
					throw;
				}
			});
		}

		private void notifyRepoDeletion(string repoPath)
		{
			Application.Instance.Invoke(() =>
			{
				try
				{
					var repoViewsToRemove = _dataSource.Where(r => r.Path.Equals(repoPath, StringComparison.OrdinalIgnoreCase)).ToArray();

					for (int i = repoViewsToRemove.Length - 1; i >= 0; i--)
						_dataSource.Remove(repoViewsToRemove[i]);

				}
				catch (Exception)
				{
					// happened to be swallowed by Eto
					throw;
				}
			});
		}

		public string Get(string path)
		{
			if (!path.EndsWith("\\", StringComparison.Ordinal))
				path += "\\";

			var repos = _dataSource.Where(r => path.StartsWith(r.Path, StringComparison.OrdinalIgnoreCase));

			if (!repos.Any())
				return string.Empty;

			var repo = repos.OrderByDescending(r => r.Path.Length).First();

			return repo.CurrentBranch + " " + StatusCompressor.Compress(repo.Repository);
		}
	}
}