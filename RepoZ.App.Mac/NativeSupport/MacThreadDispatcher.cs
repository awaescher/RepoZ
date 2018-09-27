using System;
using AppKit;
using Foundation;
using RepoZ.Api.Common;

namespace RepoZ.App.Mac.NativeSupport
{
    public class MacThreadDispatcher : IThreadDispatcher
    {
        public void Invoke(Action act)
        {
            NSApplication.SharedApplication.InvokeOnMainThread(act);
        }
    }
}
