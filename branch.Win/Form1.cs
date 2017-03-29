using branch.Shared;
using branch.Shared.PathFinding;
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

namespace branch.Win
{
	public partial class Form1 : Form
	{
		private Timer _timer;
		private WindowFinder _finder;
		private RepositoryHelper _helper;
		private FileSystemWatcher _watcher;
		private Dictionary<string, string> _repos = new Dictionary<string, string>();

		private string _path = @"C:\";

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

			_watcher = new FileSystemWatcher(_path, "HEAD");
			_watcher.Created += _watcher_Created;
			_watcher.Changed += _watcher_Changed;
			_watcher.Deleted += _watcher_Deleted;
			_watcher.IncludeSubdirectories = true;
			_watcher.EnableRaisingEvents = true;

			enumerateHeads();
		}

		private void enumerateHeads()
		{
			//var heads = Directory.GetFiles(_path, "HEAD", SearchOption.AllDirectories);

			var heads = FileFinder.Find(_path, "HEAD");
			foreach (var head in heads.Where(h => isHead(h)))
				takeRepo(head);
			showAlternative();
		}

		private void _watcher_Deleted(object sender, FileSystemEventArgs e)
		{
			if (!isHead(e.FullPath))
				return;
		}

		private void _watcher_Changed(object sender, FileSystemEventArgs e)
		{
			if (!isHead(e.FullPath))
				return;

			takeRepo(e.FullPath);

			showAlternative();
		}

		private void _watcher_Created(object sender, FileSystemEventArgs e)
		{
			if (!isHead(e.FullPath))
				return;

			Task.Run(
				() => Task.Delay(5000))
				.ContinueWith(t =>
				{
					takeRepo(e.FullPath);
					showAlternative();
				});
		}

		private bool isHead(string fullPath)
		{
			return fullPath.IndexOf(@".git\HEAD", StringComparison.OrdinalIgnoreCase) > -1;
		}

		private void takeRepo(string path)
		{
			var info = _helper.ReadRepository(path);
			_repos[info.Path] = info.CurrentBranch;
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

		private void showAlternative()
		{
			Action action = () => dataGridView1.DataSource = makeDatasource();
			this.BeginInvoke(action);
		}

		private class Rep
		{
			public string Repo { get; set; }
			public string Branch { get; set; }
		}

		private List<Rep> makeDatasource()
		{
			var list = new List<Rep>();
			foreach (var pair in _repos)
				list.Add(new Rep() { Repo = pair.Key, Branch = pair.Value });
			return list;
		}
	}
}
