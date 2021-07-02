using System;
using AppKit;

namespace RepoZ.App.Mac.Controls
{
	public class CheckableMenuItem : NSMenuItem
	{
		public CheckableMenuItem(string title, Func<bool> isChecked, Action<bool> onChange) : base(title)
		{
			IsChecked = isChecked ?? throw new ArgumentNullException(nameof(isChecked));
			OnChange = onChange ?? throw new ArgumentNullException(nameof(onChange));

			base.Activated += CheckableMenuItem_Activated;
		}

		private void CheckableMenuItem_Activated(object sender, EventArgs e)
		{
			State = (State == NSCellStateValue.On) ? NSCellStateValue.Off : NSCellStateValue.On;
			OnChange.Invoke(State == NSCellStateValue.On);
		}

		public void Update()
		{
			State = IsChecked.Invoke() ? NSCellStateValue.On : NSCellStateValue.Off;
		}

		public Action<bool> OnChange { get; }

		public Func<bool> IsChecked { get; }
	}
}