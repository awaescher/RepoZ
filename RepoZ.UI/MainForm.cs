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
		private ObservableCollection<RepositoryInfo> _dataSource;

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

			_dataSource = new ObservableCollection<RepositoryInfo>();

			createGrid();
		}

		private void createGrid()
		{
			var grid = new GridView { DataStore = _dataSource };

			grid.Columns.Add(new GridColumn()
			{
				DataCell = new TextBoxCell(nameof(RepositoryInfo.Name)),
				Sortable = true,
				Width = 200,
				HeaderText = "Repository"
			});

			grid.Columns.Add(new GridColumn()
			{
				DataCell = new TextBoxCell(nameof(RepositoryInfo.CurrentBranch)),
				Sortable = true,
				Width = 200,
				HeaderText = "Current Branch"
			});

			grid.Columns.Add(new GridColumn()
			{
				DataCell = new TextBoxCell(nameof(RepositoryInfo.Path)),
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

		private void notifyRepoChange(RepositoryInfo repo)
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