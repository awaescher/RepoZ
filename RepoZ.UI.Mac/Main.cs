using AppKit;
using Eto;
using Eto.Forms;

namespace RepoZ.UI.Mac
{
	static class MainClass
	{
		static void Main(string[] args)
		{
			new Application(Platform.Detect)
				.Run(new MainForm(
					new MacRepositoryMonitor(),
					new MacPathNavigator()
				));
		}
	}
}
