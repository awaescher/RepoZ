using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Api.Win.PInvoke
{
	public class Combridge : IDisposable
	{
		private Lazy<Type> _comType;

		public Combridge(object comObject)
		{
			ComObject = comObject;
			_comType = new Lazy<Type>(() => ComObject.GetType());
		}

		public void Dispose()
		{
			if (ComObject != null)
				Marshal.FinalReleaseComObject(ComObject);

			ComObject = null;
			_comType = null;
		}

		public T InvokeMethod<T>(string methodName)
			=> GetValueViaReflection<T>(methodName, BindingFlags.InvokeMethod);

		public T GetPropertyValue<T>(string propertyName)
			=> GetValueViaReflection<T>(propertyName, BindingFlags.GetProperty);

		protected T GetValueViaReflection<T>(string memberName, BindingFlags flags)
			=>  (T)_comType.Value.InvokeMember(memberName, flags, null, ComObject, null);

		public object ComObject { get; private set; }
	}
}
