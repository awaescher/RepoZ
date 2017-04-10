using System;
using Eto.Forms;
using Eto.Drawing;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using RepoZ.Api.Git;
using RepoZ.Api.IO;
using System.Linq;

namespace RepoZ.UI
{
	public class MainForm : Form
	{
		private IRepositoryMonitor _repositoryMonitor;
		private ObservableCollection<Repository> _dataSource = new ObservableCollection<Repository>();
		private IPathActionProvider _pathActionProvider;

		public MainForm(IRepositoryMonitor repositoryMonitor, IPathActionProvider pathActionProvider)
		{
			_repositoryMonitor = repositoryMonitor;
			_repositoryMonitor.OnChangeDetected = (repo) => notifyRepoChange(repo);
			_repositoryMonitor.Observe();

			_pathActionProvider = pathActionProvider;

			Title = "RepoZ";
			Maximizable = false;
			ClientSize = new Size(955, 600);

			createGrid();
		}

		private void createGrid()
		{
			var filtered = new FilterCollection<Repository>(_dataSource);
			filtered.Sort = (x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal);

			var grid = new GridView { DataStore = filtered };

			grid.Columns.Add(new GridColumn()
			{
				DataCell = new TextBoxCell(nameof(Repository.Name)),
				Sortable = true,
				Width = 200,
				HeaderText = "Repository",
			});

			grid.Columns.Add(new GridColumn()
			{
				DataCell = new TextBoxCell(nameof(Repository.CurrentBranch)),
				Sortable = true,
				Width = 200,
				HeaderText = "Current Branch"
			});

			grid.Columns.Add(new GridColumn()
			{
				DataCell = new TextBoxCell(nameof(Repository.Path)),
				Sortable = true,
				Width = 400,
				HeaderText = "Location"
			});

			grid.Columns.Add(new GridColumn()
			{
				DataCell = new TextBoxCell(nameof(Repository.AheadBy)),
				Sortable = true,
				Width = 75,
				HeaderText = "AheadBy"
			});

			grid.Columns.Add(new GridColumn()
			{
				DataCell = new TextBoxCell(nameof(Repository.BehindBy)),
				Sortable = true,
				Width = 75,
				HeaderText = "BehindBy"
			});

			//grid.Columns.Add(new GridColumn()
			//{
			//	DataCell = new ComboBoxCell("CurrentRepoZ")
			//	{
			//		Binding = Binding.Property<MyPoco, object>(r => r.CurrentBranch),
			//		DataStore = new string[] { "master", "dbless", "global-text-management" }

			//	},
			//	Width = 200,
			//	HeaderText = "Branch",
			//	Editable = true,
			//});

			grid.CellDoubleClick += Grid_CellDoubleClick;
			grid.MouseUp += Grid_MouseUp;

			Content = grid;
		}

		private void Grid_MouseUp(object sender, MouseEventArgs e)
		{
			var view = sender as GridView;

			if (view != null && e.Buttons == MouseButtons.Alternate)
			{
				var repo = view.SelectedItem as Repository;

				if (repo == null || !repo.WasFound)
					return;

                var items = new List<MenuItem>();

                foreach (var item in _pathActionProvider.GetFor(repo.Path))
                {
                    if (item.BeginGroup && items.Any())
                        items.Add(new SeparatorMenuItem());

					var location = this.PointToScreen(e.Location);
					location.Offset(-9, -32); // seems to be the offset of the titlebar --> TODO detect
					float[] coords = { location.X, location.Y };
                    items.Add(new ButtonMenuItem((do_not_use_sender, do_not_use_args) => item.Action(sender, coords)) { Text = item.Name });
                }
                
				var menu = new ContextMenu(items);
				menu.Show(Content);
			}
		}

		private void Grid_CellDoubleClick(object sender, GridViewCellEventArgs e)
		{
			var repo = e.Item as Repository;
			if (repo == null || !repo.WasFound)
				return;

			var action = _pathActionProvider.GetFor(repo.Path)
						 .FirstOrDefault(a => a.IsDefault);

			action?.Action?.Invoke(sender, e);
		}

		private void notifyRepoChange(Repository repo)
		{
			Application.Instance.Invoke(() =>
			{
				try
				{
					_dataSource.Remove(repo);
					_dataSource.Add(repo);
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