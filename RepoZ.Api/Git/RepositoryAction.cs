using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Api.Git
{
	public class RepositoryAction
	{
		public string Name { get; set; }

		public Action<object, object> Action { get; set; }

		public bool BeginGroup { get; set; }

		public bool ExecutionCausesSynchronizing { get; set; }

		public bool CanExecute { get; set; } = true;

		public Func<RepositoryAction[]> DeferredSubActionsEnumerator { get; set; }
	}
}
