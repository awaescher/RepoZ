namespace grrui.UI
{
    using System;
    using System.Threading.Tasks;
    using Terminal.Gui;

    public static class TimelyMessage
    {
        public static void ShowMessage(string message, TimeSpan duration)
        {
            var width = message.Length + 6;
            var height = 5;
            var lines = Label.MeasureLines(message, width);

            var dialog = new Dialog(null, width, height);

            var label = new Label((width - 4 - message.Length) / 2, 0, message);
            dialog.Add(label);

            Task.Delay(duration).ContinueWith(t => dialog.Running = false);
            Application.Run(dialog);
        }
    }
}