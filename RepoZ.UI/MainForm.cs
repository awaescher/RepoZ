using System;
using Eto.Forms;
using Eto.Drawing;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using RepoZ.Api.Git;
using RepoZ.Api.IO;
using System.Linq;
using System.Text;
using RepoZ.Api.Common;

namespace RepoZ.UI
{
	public class MainForm : Form
	{
		private IRepositoryMonitor _repositoryMonitor;
		private IRepositoryActionProvider _repositoryActionProvider;
		private IRepositoryInformationAggregator _repositoryInformationAggregator;
		private FilterCollection<RepositoryView> _datasource;
		private GridView _grid;

		public MainForm(IRepositoryMonitor repositoryMonitor, IRepositoryInformationAggregator repositoryInformationAggregator, IRepositoryActionProvider repositoryActionProvider)
		{
			Title = "RepoZ";
			Maximizable = false;
			ClientSize = new Size(1025, 600);

			_repositoryInformationAggregator = repositoryInformationAggregator;
			_repositoryActionProvider = repositoryActionProvider;

			createGrid();

			_repositoryMonitor = repositoryMonitor;
			_repositoryMonitor.OnChangeDetected = (repo) => notifyRepoChange(repo);
			_repositoryMonitor.OnDeletionDetected = (repoPath) => notifyRepoDeletion(repoPath);
			_repositoryMonitor.Observe();
		}

		private void createGrid()
		{
			_grid = new GridView();

			_grid.Columns.Add(new GridColumn()
			{
				DataCell = new TextBoxCell(nameof(RepositoryView.Name)),
				Sortable = true,
				Width = 200,
				HeaderText = "Repository",
			});

			_grid.Columns.Add(new GridColumn()
			{
				DataCell = new TextBoxCell(nameof(RepositoryView.CurrentBranch)),
				Sortable = true,
				Width = 200,
				HeaderText = "Current Branch"
			});

			_grid.Columns.Add(new GridColumn()
			{
				DataCell = new TextBoxCell(nameof(RepositoryView.Location)),
				Sortable = true,
				Width = 400,
				HeaderText = "Location"
			});

			_grid.Columns.Add(new GridColumn()
			{
				DataCell = new TextBoxCell(nameof(RepositoryView.Status)),
				Sortable = true,
				Width = 200,
				HeaderText = "Status"
			});

			_grid.CellDoubleClick += Grid_CellDoubleClick;
			_grid.MouseUp += Grid_MouseUp;

			Content = _grid;
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
					_repositoryInformationAggregator.Add(repo);
					RefreshGrid();
				}
				catch (Exception)
				{
					// happened to be swallowed by Eto
					throw;
				}
			});
		}

		private void RefreshGrid()
		{
			// huh, not nice :(
			// we're gonna remove this code by moving to native clients instead of Eto ...

			_datasource = new FilterCollection<RepositoryView>(_repositoryInformationAggregator.Repositories.Select(r => new RepositoryView(r)));
			_datasource.Sort = (x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal);

			_grid.DataStore = _datasource;
		}

		private void notifyRepoDeletion(string repoPath)
		{
			Application.Instance.Invoke(() =>
			{
				try
				{
					_repositoryInformationAggregator.RemoveByPath(repoPath);
					RefreshGrid();
				}
				catch (Exception)
				{
					// happened to be swallowed by Eto
					throw;
				}
			});
		}


	}
}