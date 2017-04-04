using System;
using Eto;
using Eto.Forms;
using RepoZ.Api.IO;
using TinyIoC;

namespace RepoZ.UI.Desktop
{
	public class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			var container = TinyIoCContainer.Current;

			//container.Register<IPathCrawler, null>();

			container.Register<MainForm>();

			var application = new Application(Platform.Detect);
			var mainForm = container.Resolve<MainForm>();
			application.Run(mainForm);
		}
	}
}
