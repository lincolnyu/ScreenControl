using static ScreenControlLib.MonitorOff;
namespace MonitorOff
{
    class Program
    {
        static void Main(string[] args)
        {
            SetMonitorInState(MonitorState.MonitorStateOff);
        }
    }
}
