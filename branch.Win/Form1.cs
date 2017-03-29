using branch.Shared;
using branch.Shared.PathFinding;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace branch.Win
{
	public partial class Form1 : Form
	{
		private Timer _timer;
		private WindowFinder _finder;
		private RepositoryHelper _helper;

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			_helper = new RepositoryHelper();
			_finder = new WindowFinder(new IPathFinder[] { new WindowsExplorerPathFinder() });

			_timer = new Timer() { Interval = 1000, Enabled = true };
			_timer.Tick += _timer_Tick;
			_timer.Start();
		}

		private void _timer_Tick(object sender, EventArgs e)
		{
			string path = _finder.GetPathOfCurrentWindow();
			var repo = _helper.ReadRepository(path);

			lblFound.Text = path;
			lblPath.Text = repo.Path;
			lblRepository.Text =  repo.Name;
			lblGitBranch.Text = repo.CurrentBranch;
		}
	}
}
