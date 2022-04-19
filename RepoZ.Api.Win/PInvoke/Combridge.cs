namespace RepoZ.Api.Win.PInvoke
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Wraps calls to COM objects to make sure that these objects are released properly. 
    /// With it, methods can be invoked and property values can be retrieved without
    /// using the dynamic keyword which leads to memory leaks when used with COM objects.
    /// See: https://stackoverflow.com/questions/33080252/memory-overflow-having-an-increasing-number-of-microsoft-csharp-runtimebinder-s/34123315
    /// Note:
    ///   - Use this class within a using block only! Otherwise COM objects might not be released as planned.
    ///   - Using this class will be slower than accessing COM properties and methods via dynamic objects. Decide for yourself: Performance vs. memory.
    /// </summary>
    /// <remarks>
    /// This class is not related to any cities or universities located in the UK.
    /// </remarks>
    public class Combridge : IDisposable
    {
        private Lazy<Type> _comType;

        /// <summary>
        /// Creates a new instance of the Combridge COM wrapper class.
        /// </summary>
        /// <param name="comObject">The COM object to wrap.</param>
        public Combridge(object comObject)
        {
            ComObject = comObject;
            _comType = new Lazy<Type>(() => ComObject.GetType());
        }

        /// <summary>
        /// Disposes the wrapper class and enforces a Marshal.FinalReleaseComObject()
        /// on the COM object if available.
        /// </summary>
        public void Dispose()
        {
            if (ComObject != null)
            {
                Marshal.FinalReleaseComObject(ComObject);
            }

            ComObject = null;
            _comType = null;
        }

        /// <summary>
        /// Invokes a method by a given name and returns its result.
        /// </summary>
        /// <typeparam name="T">The expected type of the return value.</typeparam>
        /// <param name="methodName">The name of the method to invoke on the COM object.</param>
        /// <returns></returns>
        public T InvokeMethod<T>(string methodName)
        {
            return GetValueViaReflection<T>(methodName, BindingFlags.InvokeMethod);
        }

        /// <summary>
        /// Gets the value of a property by a given property name.
        /// </summary>
        /// <typeparam name="T">The expected type of the return value.</typeparam>
        /// <param name="propertyName">The name of the property get the value from.</param>
        /// <returns></returns>
        public T GetPropertyValue<T>(string propertyName)
        {
            return GetValueViaReflection<T>(propertyName, BindingFlags.GetProperty);
        }

        /// <summary>
        /// Accesses the COM object by using the Method "InvokeMember()" to call methods
        /// or retrieve property values.
        /// </summary>
        /// <typeparam name="T">The expected type of the return value.</typeparam>
        /// <param name="memberName">The name of the member (method or property) to call or to get the value from.</param>
        /// <param name="flags">The binding flags to decide whether to invoke methods or to retrieve property values.</param>
        /// <returns></returns>
        protected T GetValueViaReflection<T>(string memberName, BindingFlags flags)
        {
            return (T)_comType.Value.InvokeMember(memberName, flags, null, ComObject, null);
        }

        /// <summary>
        /// Gets the wrapped COM object for native access.
        /// </summary>
        public object ComObject { get; private set; }
    }
}