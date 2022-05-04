namespace RepoZ.Plugin.EverythingFileSearch.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>Wrapper for Everything.</summary>
    /// <remarks>See <see href="https://www.voidtools.com/support/everything/sdk/csharp/"/> for the SDK.</remarks>
    internal static class Everything64Api
    {
        private static readonly object _lock = new object();
        private const int EVERYTHING_REQUEST_FILE_NAME = 0x00000001;
        private const int EVERYTHING_REQUEST_PATH = 0x00000002;

        [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
        public static extern void Everything_SetSearch(string lpSearchString);

        [DllImport("Everything64.dll")]
        public static extern void Everything_SetMatchCase(bool bEnable);

        [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
        public static extern bool Everything_Query(bool bWait);

        [DllImport("Everything64.dll")]
        public static extern uint Everything_GetNumResults();

        [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
        public static extern void Everything_GetResultFullPathName(uint nIndex, StringBuilder lpString, uint nMaxCount);

        [DllImport("Everything64.dll")]
        public static extern void Everything_CleanUp();

        [DllImport("Everything64.dll")]
        public static extern uint Everything_GetMajorVersion();

        // Everything 1.4
        [DllImport("Everything64.dll")]
        public static extern void Everything_SetRequestFlags(uint dwRequestFlags);

        public static IEnumerable<string> Search(string query)
        {
            lock (_lock)
            {
                try
                {
                    const int BUFFER_SIZE = 1024;

                    Everything_SetSearch(query);
                    Everything_SetRequestFlags(EVERYTHING_REQUEST_FILE_NAME | EVERYTHING_REQUEST_PATH);
                    Everything_SetMatchCase(false);

                    if (!Everything_Query(true))
                    {
                        return Enumerable.Empty<string>();
                    }

                    var nrResults = Everything_GetNumResults();

                    if (nrResults == 0)
                    {
                        return Enumerable.Empty<string>();
                    }

                    var buf = new StringBuilder(BUFFER_SIZE);
                    var result = new List<string>((int)nrResults);

                    for (uint i = 0; i < nrResults; i++)
                    {
                        buf.Clear();
                        Everything_GetResultFullPathName(i, buf, BUFFER_SIZE);
                        result.Add(buf.ToString());
                    }

                    return result;
                }
                finally
                {
                    Ignore(Everything_CleanUp);
                }
            }
        }

        public static bool IsInstalled()
        {
            lock (_lock)
            {
                try
                {
                    Everything_GetMajorVersion();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        private static void Ignore(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception)
            {
                // intentionally do nothing
            }
        }
    }
}