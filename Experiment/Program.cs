using ScreenControlLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Experiment
{
    class Program
    {
        const string MsEdgePath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";
        private static IEnumerable<Process> GetAllExistingProcess()
        {
            var processName = Path.GetFileNameWithoutExtension(Path.GetFileName(MsEdgePath));
            return Process.GetProcessesByName(processName);
        }
        static void EnumerateProcessTest()
        {
            var start = DateTime.UtcNow;
            var processes = GetAllExistingProcess()?.ToArray() ?? null;
            foreach (var p in processes)
            {
                if (p.MainWindowHandle != null)// && !string.IsNullOrEmpty(p.MainWindowTitle))
                {
                    Console.WriteLine($"{p.MainWindowTitle}");
                }
            }
            var dur= DateTime.UtcNow - start;
            Console.WriteLine($"It took {dur.TotalSeconds}");
        }
        static void StartFdmTest()
        {
            const string FdmPath = @"C:\Program Files\FreeDownloadManager.ORG\Free Download Manager\fdm.exe";
            var w = new ControlledWindow
            {
                ProcessPath = FdmPath
            };
            w.SwitchToOnlyOne(false);
        }
        static void Main(string[] args)
        {
            //var w = new MyWindow
            //{
            //    ProcessPath = @"C:\Windows\system32\notepad.exe",
            //    Arguments=""
            //};
            //w.SwitchToOnlyOne(false);
            //Input.SendKeys(new[] {
            //    //new Input.Key
            //    //{
            //    //    ScanCode = ScanCodeShort.OEM_1,
            //    //    Up = false
            //    //},
            //    //new Input.Key
            //    //{
            //    //    ScanCode = ScanCodeShort.OEM_2,
            //    //    Up = false
            //    //},
            //    //new Input.Key
            //    //{
            //    //    ScanCode = ScanCodeShort.OEM_3,
            //    //    Up = false
            //    //},
            //    //new Input.Key
            //    //{
            //    //    ScanCode = ScanCodeShort.OEM_4,
            //    //    Up = false
            //    //},
            //    //new Input.Key
            //    //{
            //    //    ScanCode = ScanCodeShort.OEM_5,
            //    //    Up = false
            //    //},
            //    //new Input.Key
            //    //{
            //    //    ScanCode = ScanCodeShort.OEM_6,
            //    //    Up = false
            //    //},
            //    //new Input.Key
            //    //{
            //    //    ScanCode = ScanCodeShort.OEM_7,
            //    //    Up = false
            //    //},
            //    //new Input.Key
            //    //{
            //    //    ScanCode = ScanCodeShort.OEM_8,
            //    //    Up = false
            //    //},
            //    ////new Input.Key
            //    //{
            //    //    ScanCode = ScanCodeShort.OEM_102,
            //    //    Up = false
            //    //},
            //    new Input.Key
            //    {
            //        ScanCode = ScanCodeShort.OEM_7,
            //        Up = false
            //    },
            //});

            //MonitorOff.SetMonitorInState(MonitorOff.MonitorState.MonitorStateOff);
            //EnumerateProcessTest();
            StartFdmTest();
        }
    }
}
