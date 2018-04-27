using System;
using AppKit;
using Foundation;
using RepoZ.Api.Common;

namespace RepoZ.UI.Mac.Story.NativeSupport
{
    public class MacThreadDispatcher : IThreadDispatcher
    {
        public void Invoke(Action act)
        {
            NSApplication.SharedApplication.InvokeOnMainThread(act);
        }
    }
}
