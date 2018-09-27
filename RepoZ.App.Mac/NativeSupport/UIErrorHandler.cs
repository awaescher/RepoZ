using System;
using AppKit;
using RepoZ.Api.Common;

namespace RepoZ.App.Mac.NativeSupport
{
    public class UIErrorHandler : IErrorHandler
    {
        public void Handle(string error)
        {
			var alert = new NSAlert()
			{
				MessageText = error,
				AlertStyle = NSAlertStyle.Critical
			};

			alert.AddButton("OK");

			alert.RunModal();
        }
    }
}
