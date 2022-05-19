namespace RepoZ.Api.Common.IO;

using System;
using System.Diagnostics;

public static class ProcessHelper
{
    public static void StartProcess(string process, string arguments, IErrorHandler errorHandler)
    {
        try
        {
            Debug.WriteLine("Starting: " + process + arguments);
            Process.Start(process, arguments);
            return;
        }
        catch (Exception)
        {
            // swallow, retry below.
        }

        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo(process, arguments)
                {
                    UseShellExecute = true,
                };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            errorHandler.Handle(ex.Message);
        }
    }
}