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
using RepoZ.Win.Crawlers;
using RepoZ.Shared.Git;

namespace RepoZ.Win
{
	public partial class Form1 : Form
	{
		private Timer _timer;
		private WindowFinder _finder;
		private RepositoryReader _reader;
		private RepositoryMonitor _monitor;
		private BindingList<RepositoryInfo> _dataSource = new BindingList<RepositoryInfo>();

		public Form1()
		{
			InitializeComponent();

			dataGridView1.AutoGenerateColumns = false;
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			_reader = new RepositoryReader();
			_finder = new WindowFinder(new IPathFinder[] { new WindowsExplorerPathFinder() });

			_timer = new Timer() { Interval = 1000, Enabled = true };
			_timer.Tick += _timer_Tick;
			_timer.Start();

			_monitor = new RepositoryMonitor(new WindowsDriveEnumerator(), _reader, () => new WindowsRepositoryWatcher(_reader), () => new GravellPathCrawler());
			_monitor.OnChangeDetected = (repo) => notifyRepoChange(repo);
			_monitor.Watch();

			dataGridView1.DataSource = _dataSource;
		}

		private void _timer_Tick(object sender, EventArgs e)
		{
			string path = _finder.GetPathOfCurrentWindow();
			var repo = _reader.ReadRepository(path);

			lblFound.Text = path;
			lblPath.Text = repo.Path;
			lblRepository.Text = repo.Name;
			lblGitBranch.Text = repo.CurrentBranch;
		}

		private void notifyRepoChange(RepositoryInfo repo)
		{
			Action act = () =>
			{

				var rems = _dataSource.Where(r => r.Path == repo.Path).ToArray();

				for (int i = 0; i < rems.Length; i++)
					_dataSource.Remove(rems[i]);

				_dataSource.Add(repo);
			};

			this.BeginInvoke(act);
		}

		private void label4_Click(object sender, EventArgs e)
		{

		}
	}
}
