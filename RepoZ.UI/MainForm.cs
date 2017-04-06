using System;
using Eto.Forms;
using Eto.Drawing;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using RepoZ.Api.Git;
using RepoZ.Api.IO;

namespace RepoZ.UI
{
	public class MainForm : Form
	{
		private IRepositoryMonitor _repositoryMonitor;
		private ObservableCollection<Repository> _dataSource = new ObservableCollection<Repository>();
		private IPathNavigator _pathNavigator;

		public MainForm(IRepositoryMonitor repositoryMonitor, IPathNavigator pathNavigator)
		{
			_repositoryMonitor = repositoryMonitor;
			_repositoryMonitor.OnChangeDetected = (repo) => notifyRepoChange(repo);
			_repositoryMonitor.Observe();

			_pathNavigator = pathNavigator;

			Title = "RepoZ";
			Maximizable = false;
			ClientSize = new Size(955, 600);

			createGrid();
		}

		private void createGrid()
		{
			_dataSource.Add(new Repository() { Name = "testrepo", CurrentBranch = "cbranch", Path = "pathy", AheadBy= 1, BehindBy = 2 });
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

			Content = grid;
		}

		private void Grid_CellDoubleClick(object sender, GridViewCellEventArgs e)
		{
			var repo = e.Item as Repository;
			if (repo != null && repo.WasFound)
				_pathNavigator.Navigate(repo.Path);
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