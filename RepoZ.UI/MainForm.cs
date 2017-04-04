using System;
using Eto.Forms;
using Eto.Drawing;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using RepoZ.Api.Git;

namespace RepoZ.UI
{
	public class MainForm : Form
	{
		private IRepositoryMonitor _repositoryMonitor;
		private ObservableCollection<Repository> _dataSource = new ObservableCollection<Repository>();

		class MyPoco
		{
			public string Text { get; set; }

			public string CurrentBranch { get; set; }

			public string[] Branches { get; set; }
		}

		public MainForm(IRepositoryMonitor repositoryMonitor)
		{
			_repositoryMonitor = repositoryMonitor;
			_repositoryMonitor.OnChangeDetected = (repo) => notifyRepoChange(repo);
			_repositoryMonitor.Observe();

			Title = "RepoZ";
			Maximizable = false;
			ClientSize = new Size(805, 600);

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
			Content = grid;
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
				catch (Exception ex)
				{

					throw;
				}
			});
		}
	}
}