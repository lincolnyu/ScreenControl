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
    public class ControlledWindow
    {
        private const int MaximizeDelay = 1000;

        // Start parameters
        public string ProcessPath { get; set; }
        public string Arguments { get; set; }
        public bool MaximizeOnStartup { get; set; } = true;

        public Process Process { get; private set; }

        public enum Statuses
        {
            Uninitialized,
            WindowInvalid,
            WindowValid,
        }
        public Statuses Status = Statuses.Uninitialized;

        private static Dictionary<int, string> GetProcessTitleMap(IEnumerable<Process> processes)
        {
            var res = new Dictionary<int, string>();
            foreach (var p in processes)
            {
                var title = EnumWindows.GetTitle(p.MainWindowHandle);
                res[p.Id] = title;
            }
            return res;
        }

        private static IEnumerable<int> DiffMap(Dictionary<int, string> prev, Dictionary<int, string> curr)
        {
            foreach (var pprev in prev)
            {
                if (curr.TryGetValue(pprev.Key, out var bval) && bval != pprev.Value)
                {
                    yield return pprev.Key;
                }
            }
            foreach (var pcurr in curr)
            {
                if (!prev.ContainsKey(pcurr.Key))
                {
                    yield return pcurr.Key;
                }
            }
        }

        private Process[] GetDiff(Dictionary<int, string> prevTitleMap)
        {
            var processes = GetAllExistingProcess();
            var titleMap = GetProcessTitleMap(processes);
            var diff = DiffMap(prevTitleMap, titleMap).ToArray();
            var diffProcesses = diff.Select(x =>
            {
                try
                {
                    return Process.GetProcessById(x);
                }
                catch(ArgumentException)
                {
                    return null;
                }
            }).Where(p=>p != null).ToArray();
            return diffProcesses;
        }

        /// <summary>
        ///  Switch to the main window of the process.
        ///  If 'Process' is null a new process will be created.
        ///  If unable to get the window from the process will attempt to find the window
        ///  with <paramref name="titleMatch"/>.
        /// </summary>
        /// <param name="titleMatch">Match the title</param>
        /// <param name="titleWaitTimeOutMs">The time out for waiting the window</param>
        /// <returns>If successful</returns>
        private bool SwitchTo(Func<string, bool> titleMatch = null,
            int titleWaitTimeOutMs = 10000)
        {
            const int waitStep = 1000;
            
            var prevProcesses = GetAllExistingProcess();
            var prevTitleMap = GetProcessTitleMap(prevProcesses);

            bool processWasNull = Process == null;
            if (processWasNull)
            {
                var psi = new ProcessStartInfo
                {
                    FileName = ProcessPath,
                    Arguments = Arguments
                };
                Process = Process.Start(psi);
                Thread.Sleep(MaximizeDelay);
            }

            if (Process.HasExited)
            {
                var diffProcesses = GetDiff(prevTitleMap);
                if (diffProcesses.Length == 1 && titleMatch == null)
                {
                    Process = diffProcesses[0];
                    SwitchToThisWindow();
                    return true;
                }
                else if (titleMatch != null)
                {
                    var lastReg = DateTime.UtcNow;
                    var end = lastReg.AddMilliseconds(titleWaitTimeOutMs);
                    while (true)
                    {
                        var tdiff = end - DateTime.UtcNow;
                        var elapsed = DateTime.UtcNow - lastReg;
                        if (elapsed.TotalMilliseconds > 3000)
                        {
                            diffProcesses = GetDiff(prevTitleMap);
                            lastReg = DateTime.UtcNow;
                        }
                        foreach (var process in diffProcesses)
                        {
                            var title = EnumWindows.GetTitle(process.MainWindowHandle);
                            var found = titleMatch(title);
                            if (found)
                            {
                                Process = process;
                                SwitchToThisWindow();
                                return true;
                            }
                            else if (tdiff <= TimeSpan.Zero)
                            {
                                return false;
                            }
                        }
                        Thread.Sleep(Math.Min(waitStep, (int)tdiff.TotalMilliseconds));
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (titleMatch != null)
                {
                    var processes = GetAllExistingProcess();
                    var lastReg = DateTime.UtcNow;
                    var end = lastReg.AddMilliseconds(titleWaitTimeOutMs);
                    while (true)
                    {
                        var tdiff = end - DateTime.UtcNow;
                        var elapsed = DateTime.UtcNow - lastReg;
                        if (elapsed.TotalMilliseconds > 3000)
                        {
                            processes = GetAllExistingProcess();
                            lastReg = DateTime.UtcNow;
                        }
                        foreach (var process in processes)
                        {
                            var title = EnumWindows.GetTitle(process.MainWindowHandle);
                            if (titleMatch(title))
                            {
                                Process = process;
                                SwitchToThisWindow();
                                return true;
                            }
                            else if (tdiff <= TimeSpan.Zero)
                            {
                                return false;
                            }
                        }
                    }   
                }
                else
                {
                    SwitchToThisWindow();
                    return true;
                }
            }
        }

        /// <summary>
        ///  When window is available for the process
        /// </summary>
        public void SwitchToThisWindow()
        {
            if (MaximizeOnStartup)
            {
                while (!ShowWindow(Process.MainWindowHandle, (int)ShowWindowCommands.SW_MAXIMIZE))
                {
                    Thread.Sleep(250);
                }
            }
            //The following seems deprecated
            //SwitchToThisWindow(Process.MainWindowHandle, true);
            // Seems this is needed to set focus on such windows as loading page
            SetForegroundWindow(Process.MainWindowHandle);
            SetAsCurrent();
        }

        public static ControlledWindow Capture(string processName, Action action, bool maximize = true)
        {
            var prevProcesses = Process.GetProcessesByName(processName);
            var prevTitleMap = GetProcessTitleMap(prevProcesses);
            action();
            var processes = Process.GetProcessesByName(processName);
            var titleMap = GetProcessTitleMap(processes);
            var diff = DiffMap(prevTitleMap, titleMap).ToArray();
            if (diff.Length == 1)
            {
                var proc = Process.GetProcessById(diff[0]);
                var w = new ControlledWindow
                {
                    Process = proc,
                    ProcessPath = proc.StartInfo.FileName,
                    Arguments = proc.StartInfo.Arguments,
                    MaximizeOnStartup = maximize,
                    Status = Statuses.WindowValid
                };
                w.SwitchToThisWindow();
                return w;
            }
            return null;
        }

        /// <summary>
        ///  Create a new process using <paramref name="titleMatch"/> to match
        ///  the window to be created in case the window cannot be retrieved
        ///  from the process object.
        /// </summary>
        /// <param name="titleMatch">Match the title</param>
        /// <returns>If the process is created and the window retrieved</returns>
        public bool CreateNew(Func<string, bool> titleMatch = null, int timeoutMs = int.MaxValue)
        {
            Process = null;
            var succ = SwitchTo(titleMatch, timeoutMs);
            Status = succ? Statuses.WindowValid : Statuses.WindowInvalid;
            return succ;
        }

        public void SwitchToOnlyOne(bool killAllExisting, 
            Func<string, bool> titleMatch = null, int timeoutMs = int.MaxValue)
        {
            var existing = GetAllExistingProcess();
            if (existing.Length > 0)
            {
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
                    if (!string.IsNullOrEmpty(Arguments))
                    {
                        foreach (var p in existing)
                        {
                            if (p.StartInfo.Arguments == Arguments && Process == null)
                            {
                                Process = p;
                            }
                            else
                            {
                                p.Kill();
                            }
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
            }
            //TODO for process in sys tray Process.MainWindowHandle will be zero
            Status = SwitchTo(titleMatch, timeoutMs) ? Statuses.WindowValid : Statuses.WindowInvalid;
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
            UnsetAsCurrent();
        }

        public void Kill()
        {
            if (Process != null)
            {
                Process.Kill();
            }
            UnsetAsCurrent();
        }

        public void Close()
        {
            if (Process != null)
            {
                Process.CloseMainWindow();
            }
            UnsetAsCurrent();
        }

        public void SoftCloseWindow()
        {
            SwitchToThisWindow();
            Input.SendKeys(
                new []
                {
                    new Input.Key
                    {
                        ScanCode = ScanCodeShort.LCONTROL,
                        Up = false,
                    },
                    new Input.Key
                    {
                        ScanCode = ScanCodeShort.F4,
                        Up = false,
                    },
                    new Input.Key
                    {
                        ScanCode = ScanCodeShort.F4,
                        Up = true,
                    },
                    new Input.Key
                    {
                        ScanCode = ScanCodeShort.LCONTROL,
                        Up = true
                    }
                }
            );
        }

        public static string ProcessPathToName(string processPath)
            => Path.GetFileNameWithoutExtension(Path.GetFileName(processPath));

        private Process[] GetAllExistingProcess()
            => Process.GetProcessesByName(ProcessPathToName(ProcessPath));

        public RECT? WindowRect => Process != null && GetWindowRect(Process.MainWindowHandle, out var rect) ? rect : (RECT?)null;

        public static ControlledWindow CurrentForeground;

        private void SetAsCurrent()
        {
            CurrentForeground = this;
        }
        private void UnsetAsCurrent()
        {
            if (CurrentForeground == this)
            {
                CurrentForeground = null;
            }
        }
    }
}
