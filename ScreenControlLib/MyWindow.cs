using System.IO;
using System.Diagnostics;
using System.Threading;
using static WindowsApi.EnumLib;
using static WindowsApi.User32;
using static WindowsApi.StructLib;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ScreenControlLib
{
    public class MyWindow
    {
        private const int MaximizeDelay = 1000;

        public string Command { get; set; }
        public string ProcessPath { get; set; }
        public string Arguments { get; set; }
        public bool MaximizeOnStartup { get; set; } = true;

        public Process Process { get; private set; }

        private bool SwitchTo(string titleClue = null, int titleWaitTimeOutMs = 10000)
        {
            bool processWasNull = Process == null;
            if (processWasNull)
            {
                if (Command != null)
                {
                    Process = Process.Start(Command);
                }
                else
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = ProcessPath,
                        Arguments = Arguments
                    };
                    Process = Process.Start(psi);
                }
                Thread.Sleep(MaximizeDelay);
            }

            try
            {
                SwitchToThisWindow(Process.MainWindowHandle, true);
            }
            catch (InvalidOperationException)
            {
                // e.g. Creating a new msedge process may end up creating a new tab with process exiting
            }
            var processes = GetAllExistingProcess()?.ToArray() ?? null;
            if (processes == null)
            {
                processes = new[] { Process };
            }

            if (Process.HasExited && titleClue != null)
            {
                const int waitStep = 1000;
                var end = DateTime.UtcNow.AddMilliseconds(titleWaitTimeOutMs);
                while (true)
                {
                    var tdiff = end - DateTime.UtcNow;
                    foreach (var process in processes)
                    {
                        var title = EnumWindows.GetTitle(process.MainWindowHandle);
                        var good = title.StartsWith(titleClue);
                        if (good || tdiff <= TimeSpan.Zero)
                        {
                            Process = process;
                            ShowWindow(process.MainWindowHandle, (int)ShowWindowCommands.SW_MAXIMIZE);
                            return true;
                        }
                    }
                    Thread.Sleep(Math.Min(waitStep, (int)tdiff.TotalMilliseconds));
                }
                return false;
            }
            else
            {
                foreach (var process in processes)
                {
                    if (MaximizeOnStartup)
                    {
                        ShowWindow(process.MainWindowHandle, (int)ShowWindowCommands.SW_MAXIMIZE);
                    }
                }
                return true;
            }
        }

        public bool CreateNew(string titleClue = null)
        {
            Process = null;
            return SwitchTo(titleClue);
        }

        public void SwitchToOnlyOne(bool killAllExisting)
        {
            var existing = GetAllExistingProcess();
            if (existing != null)
            {
                if (!string.IsNullOrEmpty(Arguments))
                {
                    killAllExisting = true;
                }
                Process = null;
                if (killAllExisting)
                {
                    foreach (var p in existing)
                    {
                        p.Kill();
                    }
                }
                else
                {
                    var a = existing.ToArray();
                    if (a.Length > 0)
                    {
                        Process = a[0];
                        for (var i = 1; i < a.Length; i++)
                        {
                            a[i].Kill();
                        }
                    }
                }
            }
            SwitchTo();
        }

        public bool WaitUntilTitleStartsWith(string startsWith, int timeoutMs=10000)
        {
            const int waitStep = 1000;
            var end = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            while (true)
            {
                var title = EnumWindows.GetTitle(Process.MainWindowHandle);
                var good = title.StartsWith(startsWith);
                var tdiff = end - DateTime.UtcNow;
                if (good || tdiff <= TimeSpan.Zero)
                {
                    return good;
                }
                Thread.Sleep(Math.Min(waitStep, (int)tdiff.TotalMilliseconds));
            }
        }

        public void KillAll()
        {
            var existing = GetAllExistingProcess();
            foreach (var p in existing)
            {
                p.Kill();
            }
        }

        private IEnumerable<Process> GetAllExistingProcess()
        {
            if (ProcessPath == null)
            {
                return null;
            }
            var processName = Path.GetFileNameWithoutExtension(Path.GetFileName(ProcessPath));
            return Process.GetProcessesByName(processName);
        }

        public RECT? WindowRect => Process != null && GetWindowRect(Process.MainWindowHandle, out var rect) ? rect : (RECT?)null;
    }
}
