using System;
using System.Threading;
using static ScreenControlLib.MonitorOff;

namespace MonitorOff
{
    class Program
    {
        static void Main(string[] args)
        {
            var i = Array.IndexOf(args, "-r");
            if (i >= 0)
            {
                if (!(i < args.Length-1 && int.TryParse(args[i+1], out var sleepMs)))
                {
                    sleepMs = 6000;
                }
                while (true)
                {
                    SetMonitorInState(MonitorState.MonitorStateOff);
                    Thread.Sleep(sleepMs);
                }
            }
            else
            {
                SetMonitorInState(MonitorState.MonitorStateOff);
            }
        }
    }
}
