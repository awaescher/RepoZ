using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepoZ.Api.Common;
using RepoZ.Api.Git;

namespace RepoZ.Api.Common.Git
{
	public abstract class FileRepositoryStore : IRepositoryStore
	{
		private IErrorHandler _errorHandler;

		public FileRepositoryStore(IErrorHandler errorHandler)
		{
			_errorHandler = errorHandler;
		}

		public abstract string GetFileName();

		public IEnumerable<string> Get()
		{
			string file = GetFileName();

			if (File.Exists(file))
			{
				try
				{
					return File.ReadAllLines(file);
				}
				catch (Exception ex)
				{
					_errorHandler.Handle(ex.Message);
				}
			}

			return new string[0];
		}

		public void Set(IEnumerable<string> paths)
		{
			string file = GetFileName();
			string path = Directory.GetParent(file).FullName;

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			try
			{
				File.WriteAllLines(GetFileName(), paths.ToArray());
			}
			catch (Exception ex)
			{
				_errorHandler.Handle(ex.Message);
			}
		}
	}
}
