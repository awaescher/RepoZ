using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Api.Win.PInvoke.Explorer
{
	public class CleanWindowTitleActor : ExplorerWindowActor
	{
		protected override void Act(IntPtr hwnd, string explorerLocationUrl)
		{
			Console.WriteLine("Clean " + explorerLocationUrl);
			string separator = "  [";
			WindowHelper.RemoveAppendedWindowText(hwnd, separator);
		}
	}
}
