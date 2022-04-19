namespace RepoZ.App.Win
{
    using System.Windows.Forms;

    public static class TaskbarLocator
    {
        public enum TaskBarLocation
        {
            Top,
            Bottom,
            Left,
            Right,
        }

        public static TaskBarLocation GetTaskBarLocation()
        {
            TaskBarLocation taskBarLocation = TaskBarLocation.Bottom;
            bool taskBarOnTopOrBottom = (Screen.PrimaryScreen.WorkingArea.Width == Screen.PrimaryScreen.Bounds.Width);

            if (taskBarOnTopOrBottom)
            {
                if (Screen.PrimaryScreen.WorkingArea.Top > 0)
                {
                    taskBarLocation = TaskBarLocation.Top;
                }
            }
            else
            {
                taskBarLocation = Screen.PrimaryScreen.WorkingArea.Left > 0
                    ? TaskBarLocation.Left
                    : TaskBarLocation.Right;
            }

            return taskBarLocation;
        }
    }
}