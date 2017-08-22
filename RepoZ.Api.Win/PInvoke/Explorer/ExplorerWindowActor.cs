using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

			var comShellApplication = Activator.CreateInstance(_shellApplicationType);
			using (var shell = new Combridge(comShellApplication))
			{
				try
				{
					var comWindows = shell.InvokeMethod<IEnumerable>("Windows");

					foreach (var comWindow in comWindows)
					{
						if (comWindow == null)
							continue;

						using (var window = new Combridge(comWindow))
						{
							var fullName = window.GetPropertyValue<string>("FullName");
							var executable = Path.GetFileName(fullName);
							if (executable.ToLower() == "explorer.exe")
							{
								// thanks http://docwiki.embarcadero.com/Libraries/Seattle/en/SHDocVw.IWebBrowser2_Properties
								var hwnd = window.GetPropertyValue<long>("hwnd");
								var locationUrl = window.GetPropertyValue<string>("LocationURL");

								Act((IntPtr)hwnd, locationUrl);
							}
						}
					}
				}
				catch (COMException)
				{
					// nothing we can do in here ...
				}
			}
		}

		protected abstract void Act(IntPtr hwnd, string explorerLocationUrl);
	}
}
