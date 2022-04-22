namespace grr
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal static class ConsoleExtensions
    {
        /// <summary>
        /// A utility class to determine a process parent.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ParentProcessUtilities
        {
            // These members must match PROCESS_BASIC_INFORMATION
            internal IntPtr Reserved1;
            internal IntPtr PebBaseAddress;
            internal IntPtr Reserved2_0;
            internal IntPtr Reserved2_1;
            internal IntPtr UniqueProcessId;
            internal IntPtr InheritedFromUniqueProcessId;

            [DllImport("ntdll.dll")]
            private static extern int NtQueryInformationProcess(
                IntPtr processHandle,
                int processInformationClass,
                ref ParentProcessUtilities processInformation,
                int processInformationLength,
                out int returnLength);

            /// <summary>
            /// Gets the parent process of the current process.
            /// </summary>
            /// <returns>An instance of the Process class.</returns>
            public static Process GetParentProcess()
            {
                return GetParentProcess(Process.GetCurrentProcess().Handle);
            }

            /// <summary>
            /// Gets the parent process of specified process.
            /// </summary>
            /// <param name="id">The process id.</param>
            /// <returns>An instance of the Process class.</returns>
            public static Process GetParentProcess(int id)
            {
                var process = Process.GetProcessById(id);
                return GetParentProcess(process.Handle);
            }

            /// <summary>
            /// Climbs up the process tree to find the windowed process where SendKey can send the command keys to
            /// </summary>
            /// <param name="id">The process id</param>
            /// <returns>First parent in the process tree with window handle </returns>
            internal static Process GetWindowedParentProcess(in int id)
            {
                var process = Process.GetProcessById(id);
                while (process.MainWindowHandle == IntPtr.Zero)
                {
                    Process lastProcess = process;
                    process = GetParentProcess(process.Handle);

                    // Better a result without window handle than an infinite loop
                    if (lastProcess == process)
                    {
                        break;
                    }
                }

                return process;
            }

            /// <summary>
            /// Gets the parent process of a specified process.
            /// </summary>
            /// <param name="handle">The process handle.</param>
            /// <returns>An instance of the Process class.</returns>
            public static Process GetParentProcess(IntPtr handle)
            {
                var pbi = new ParentProcessUtilities();
                var status = NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), out var returnLength);
                if (status != 0)
                {
                    throw new Win32Exception(status);
                }

                try
                {
                    return Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32());
                }
                catch (ArgumentException)
                {
                    // not found
                    return null;
                }
            }
        }

        public static void WriteConsoleInput(Process target, string value, int waitMilliseconds = 0)
        {
            PrintDebug($"Write to console input {target.ProcessName} ({target.Id})");

            // Find the first process in the process tree which has a windows handle
            target = ParentProcessUtilities.GetWindowedParentProcess(target.Id);

            PrintDebug($"Found a process, writing to process {target.ProcessName} ({target.Id})");

            // send CTRL+V with Enter to insert the command
            var arguments = "^v{Enter}";

            arguments = $"-pid:{target.Id} \"{arguments}\"";

            if (waitMilliseconds > 0)
            {
                arguments += $" -wait:{waitMilliseconds}";
            }

            var currentPath = Path.GetDirectoryName(Path.Combine(Assembly.GetExecutingAssembly().Location));
            var command = Path.Combine(currentPath, "SendKeys.exe");
            if (File.Exists(command))
            {
                Process.Start(new ProcessStartInfo(command, arguments) { UseShellExecute = true, });
            }
            else
            {
                Console.WriteLine(command + " does not exist.");
            }
        }

        private static void PrintDebug(string value)
        {
            Debug.WriteLine(value);
        }
    }
}