using System;
using Eto.Forms;
using Eto.Drawing;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace RepoZ.UI
{
	public class MainForm : Form
	{


		class MyPoco
		{
			public string Text { get; set; }

			public string CurrentBranch { get; set; }

			public string[] Branches { get; set; }
		}

		public MainForm()
		{
			Title = "RepoZ";
			this.Maximizable = false;
			ClientSize = new Size(405, 350);

			var collection = new ObservableCollection<MyPoco>();
			collection.Add(new MyPoco { Text = "Repo 1", CurrentBranch = "master", Branches = new string[] { "master", "dbless", "global-text-management" } });
			collection.Add(new MyPoco { Text = "Repo 2", CurrentBranch = "global-text-management", Branches = new string[] { "master", "dbless", "global-text-management" } });

			var grid = new GridView { DataStore = collection };

			grid.Columns.Add(new GridColumn()
			{
				DataCell = new TextBoxCell { Binding = Binding.Property<MyPoco, string>(r => r.Text) },
				Sortable = true,
				Width = 200,
				HeaderText = "Repository"
			});

			grid.Columns.Add(new GridColumn()
			{
				DataCell = new ComboBoxCell("CurrentRepoZ")
				{
					Binding = Binding.Property<MyPoco, object>(r => r.CurrentBranch),
					DataStore = new string[] { "master", "dbless", "global-text-management" }

				},
				Width = 200,
				HeaderText = "Branch",
				Editable = true,
			});

			Content = grid;
		}
	}
}