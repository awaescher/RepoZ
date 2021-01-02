using System.Windows.Forms;

namespace RepoZ.App.Win
{
	public static class TaskbarLocator
	{
		public enum TaskBarLocation
		{
			Top,
			Bottom,
			Left,
			Right
		}

		public static TaskBarLocation GetTaskBarLocation()
		{
			TaskBarLocation taskBarLocation = TaskBarLocation.Bottom;
			bool taskBarOnTopOrBottom = (Screen.PrimaryScreen.WorkingArea.Width == Screen.PrimaryScreen.Bounds.Width);

			if (taskBarOnTopOrBottom)
			{
				if (Screen.PrimaryScreen.WorkingArea.Top > 0)
					taskBarLocation = TaskBarLocation.Top;
			}
			else
			{
				if (Screen.PrimaryScreen.WorkingArea.Left > 0)
					taskBarLocation = TaskBarLocation.Left;
				else
					taskBarLocation = TaskBarLocation.Right;
			}

			return taskBarLocation;
		}
	}
}
