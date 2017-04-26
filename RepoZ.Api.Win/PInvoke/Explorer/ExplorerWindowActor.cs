using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Api.Win.PInvoke.Explorer
{
	public abstract class ExplorerWindowActor
	{
		private Type _shellApplicationType;

		public void Pulse()
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
						Act((IntPtr)ie.hwnd, ie.LocationURL);
					}
				}
			}
			finally
			{
				Marshal.FinalReleaseComObject(o);
			}
		}

		protected abstract void Act(IntPtr hwnd, string explorerLocationUrl);
	}
}
