using RepoZ.Shared;
using RepoZ.Shared.PathFinding;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RepoZ.Win
{
	public partial class Form1 : Form
	{
		private Timer _timer;
		private WindowFinder _finder;
		private RepositoryHelper _helper;
		private RepositoryMonitor _monitor;
		private BindingList<RepositoryHelper.RepositoryInfo> _dataSource = new BindingList<RepositoryHelper.RepositoryInfo>();

		public Form1()
		{
			InitializeComponent();

			dataGridView1.AutoGenerateColumns = false;
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			_helper = new RepositoryHelper();
			_finder = new WindowFinder(new IPathFinder[] { new WindowsExplorerPathFinder() });

			_timer = new Timer() { Interval = 1000, Enabled = true };
			_timer.Tick += _timer_Tick;
			_timer.Start();

			//enumerateHeads();

			_monitor = new RepositoryMonitor("C:\\");
			_monitor.OnChangeDetected = (repo) => notifyRepoChange(repo);
			_monitor.Watch();

			dataGridView1.DataSource = _dataSource;
		}

		private void _timer_Tick(object sender, EventArgs e)
		{
			string path = _finder.GetPathOfCurrentWindow();
			var repo = _helper.ReadRepository(path);

			lblFound.Text = path;
			lblPath.Text = repo.Path;
			lblRepository.Text = repo.Name;
			lblGitBranch.Text = repo.CurrentBranch;
		}

		private void showRepositories()
		{
			Action action = () => dataGridView1.DataSource = _monitor.Repositories;
			this.BeginInvoke(action);
		}

		private void buttonCrawlerTest_Click(object sender, EventArgs e)
		{
			//var sw = System.Diagnostics.Stopwatch.StartNew();
			//var list1 = new Crawlers.WindowsPathCrawler().Find("C:\\", "HEAD");
			//sw.Stop();
			//var elap1 = sw.Elapsed;

			//sw.Restart();

			Task.Run(() => new Crawlers.GravellPathCrawler().Find("C:\\", "HEAD", file => onFound(file)));

			//var list2 = new Crawlers.GravellPathCrawler().Find("C:\\", "HEAD", file => onFound(file));
			//sw.Stop();
			//var elap2 = sw.Elapsed;

			//string message = $"Crawler 1 found {list1.Count} HEADs in {elap1.ToString()}\nCrawler 2 found {list2.Count} HEADs in {elap2.ToString()}";
			//MessageBox.Show(this, message, "Crawler Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void onFound(string file)
		{
			var repo = _helper.ReadRepository(file);
			if (repo.WasFound)
				notifyRepoChange(repo);
		}

		private void notifyRepoChange(RepositoryHelper.RepositoryInfo repo)
		{
			Action act = () =>
			{

				var rems = _dataSource.Where(r => r.Path == repo.Path).ToArray();

				for (int i = 0; i < rems.Length; i++)
				{
					_dataSource.Remove(rems[i]);
				}

				_dataSource.Add(repo);
			};

			this.BeginInvoke(act);
		}
	}
}
