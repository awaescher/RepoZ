using RepoZ.Api.Git;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Api.Win.PInvoke
{
	public class WindowsExplorerHandler
	{
		private IRepositoryInformationAggregator _repositoryInfoAggregator;
		private Type _shellApplicationType;

		public WindowsExplorerHandler(IRepositoryInformationAggregator repositoryInfoAggregator)
		{
			_repositoryInfoAggregator = repositoryInfoAggregator;
		}

		public bool CanHandle(string processName)
		{
			return string.Equals("explorer", processName, StringComparison.OrdinalIgnoreCase);
		}

		public string Pulse()
		{
			if (_shellApplicationType == null)
				_shellApplicationType = Type.GetTypeFromProgID("Shell.Application");

			dynamic o = Activator.CreateInstance(_shellApplicationType);
			try
			{
				var ws = o.Windows();
				for (int i = 0; i < ws.Count; i++)
				{
					var ie = ws.Item(i);
					if (ie == null)
						continue;

					var executable = System.IO.Path.GetFileName((string)ie.FullName);
					if (executable.ToLower() == "explorer.exe")
					{
						// thanks http://docwiki.embarcadero.com/Libraries/Seattle/en/SHDocVw.IWebBrowser2_Properties
						string url = ie.LocationURL;

						if (!string.IsNullOrEmpty(url))
						{
							string path = new Uri(url).LocalPath;

							string info = _repositoryInfoAggregator.Get(path);

							if (!string.IsNullOrEmpty(info))
							{
								string separator = "  [";
								WindowHelper.AppendWindowText((IntPtr)ie.hwnd, separator, info + "]");
							}
						}
					}
				}
			}
			finally
			{
				Marshal.FinalReleaseComObject(o);
			}

			return null;
		}
	}
}
