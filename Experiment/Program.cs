using ScreenControlLib;
using System.Data;
using static WindowsApi.EnumLib;

namespace Experiment
{
    class Program
    {
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

            MonitorOff.SetMonitorInState(MonitorOff.MonitorState.MonitorStateOff);
        }
    }
}
