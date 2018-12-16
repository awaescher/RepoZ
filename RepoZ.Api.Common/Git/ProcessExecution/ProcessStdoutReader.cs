using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Api.Common.Git.ProcessExecution
{
	internal class ProcessStdoutReader : TextReader
	{
		private readonly GitProcess _process;
		private readonly IGitCommander _gitCommander;

		public ProcessStdoutReader(IGitCommander gitCommander, GitProcess process)
		{
			_gitCommander = gitCommander;
			_process = process;
		}

		public override void Close()
		{
			_gitCommander.Close(_process);
		}

		public override System.Runtime.Remoting.ObjRef CreateObjRef(Type requestedType)
		{
			return _process.StandardOutput.CreateObjRef(requestedType);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _process != null)
			{
				Close();
			}
			base.Dispose(disposing);
		}

		public override bool Equals(object obj)
		{
			return _process.StandardOutput.Equals(obj);
		}

		public override int GetHashCode()
		{
			return _process.StandardOutput.GetHashCode();
		}

		public override object InitializeLifetimeService()
		{
			return _process.StandardOutput.InitializeLifetimeService();
		}

		public override int Peek()
		{
			return _process.StandardOutput.Peek();
		}

		public override int Read()
		{
			return _process.StandardOutput.Read();
		}

		public override int Read(char[] buffer, int index, int count)
		{
			return _process.StandardOutput.Read(buffer, index, count);
		}

		public override int ReadBlock(char[] buffer, int index, int count)
		{
			return _process.StandardOutput.ReadBlock(buffer, index, count);
		}

		public override string ReadLine()
		{
			return _process.StandardOutput.ReadLine();
		}

		public override string ReadToEnd()
		{
			return _process.StandardOutput.ReadToEnd();
		}

		public override string ToString()
		{
			return _process.StandardOutput.ToString();
		}
	}
}
