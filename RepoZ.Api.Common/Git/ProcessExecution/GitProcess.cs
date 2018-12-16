using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Api.Common.Git.ProcessExecution
{
	public class GitProcess : IDisposable
	{
		private readonly Process _process;

		public GitProcess(Process process)
		{
			_process = process;
		}

		public static implicit operator Process(GitProcess process)
		{
			return process._process;
		}

		public void ConsumeStandardError()
		{
			StandardErrorString = "";
			_process.ErrorDataReceived += StdErrReceived;
			_process.BeginOutputReadLine();
			_process.BeginErrorReadLine();
		}

		private void StdErrReceived(object sender, DataReceivedEventArgs e)
		{
			if (e.Data != null && e.Data.Trim() != "")
			{
				var data = e.Data;
				Trace.WriteLine(data.TrimEnd(), "git stderr");
				StandardErrorString += data;
			}
		}

		public bool WaitForExit(int milliseconds)
		{
			return _process.WaitForExit(milliseconds);
		}

		public void Dispose()
		{
			_process?.Dispose();
		}

		public string StandardErrorString { get; private set; }

		public ProcessStartInfo StartInfo { get { return _process.StartInfo; } }

		public int ExitCode { get { return _process.ExitCode; } }

		public StreamWriter StandardInput { get { return _process.StandardInput; } }

		public StreamReader StandardOutput { get { return _process.StandardOutput; } }
	}
}