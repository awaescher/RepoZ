using System;
using Eto.Forms;
using Eto.Drawing;
using System.Collections.ObjectModel;

namespace branch.UI
{
	public class MainForm : Form
	{
		class MyPoco
		{
			public string Text { get; set; }

			public bool Check { get; set; }
		}

		public MainForm()
		{
			Title = "branch";
			this.Maximizable = false;
			ClientSize = new Size(405, 350);

			var collection = new ObservableCollection<MyPoco>();
			collection.Add(new MyPoco { Text = "Row 1", Check = true });
			collection.Add(new MyPoco { Text = "Row 2", Check = false });

			var grid = new GridView { DataStore = collection };

			grid.Columns.Add(new GridColumn
			{
				DataCell = new TextBoxCell { Binding = Binding.Property<MyPoco, string>(r => r.Text) },
				Sortable = true,
				Width = 200,
				HeaderText = "Repository"
			});

			grid.Columns.Add(new GridColumn
			{
				DataCell = new CheckBoxCell { Binding = Binding.Property<MyPoco, bool?>(r => r.Check) },
				Width = 200,
				HeaderText = "Branch"
			});

			Content = grid;
			//Content = new StackLayout
			//{
			//	Padding = 10,
			//	Items =
			//	{
			//		grid
			//	}
			//};

			//// create a few commands that can be used for the menu and toolbar
			//var clickMe = new Command { MenuText = "Click Me!", ToolBarText = "Click Me!" };
			//clickMe.Executed += (sender, e) => MessageBox.Show(this, "I was clicked!");

			//var quitCommand = new Command
			//{
			//	MenuText = "Quit",
			//	Shortcut = Application.Instance.CommonModifier | Keys.Q
			//};
			//quitCommand.Executed += (sender, e) => Application.Instance.Quit();

			//var aboutCommand = new Command { MenuText = "About..." };
			//aboutCommand.Executed += (sender, e) => MessageBox.Show(this, "About my app...");

			//// create menu
			//Menu = new MenuBar
			//{
			//	Items =
			//	{
			//		// File submenu
			//		new ButtonMenuItem { Text = "&File", Items = { clickMe } },
			//		// new ButtonMenuItem { Text = "&Edit", Items = { /* commands/items */ } },
			//		// new ButtonMenuItem { Text = "&View", Items = { /* commands/items */ } },
			//	},
			//	ApplicationItems =
			//	{
			//		// application (OS X) or file menu (others)
			//		new ButtonMenuItem { Text = "&Preferences..." },
			//	},
			//	QuitItem = quitCommand,
			//	AboutItem = aboutCommand
			//};

			//// create toolbar			
			//ToolBar = new ToolBar { Items = { clickMe } };
		}
	}
}