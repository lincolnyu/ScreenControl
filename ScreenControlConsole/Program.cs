using ScreenControlLib;
using System.Threading;
using Xamarin.Essentials;

namespace ScreenControlConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var w = new MyWindow
            {
                ProcessPath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
                Arguments = @"-inprivate http://images.google.com"
            };
            w.SwitchToOnlyOne(false);
            if (w.WindowRect.HasValue)
            {
                if (Clipboard.HasText)
                {
                    Thread.Sleep(8000);
                    Input.MouseClick(793, 357);
                    var task = Clipboard.GetTextAsync();
                    task.Wait();
                    var s = task.Result;
                    if (s.StartsWith("http://") || s.StartsWith("https://"))
                    {
                        Thread.Sleep(1000);
                        var keys = new[]
                        {
                            new Input.Key {
                                Up = false,
                                ScanCode = WindowsApi.EnumLib.ScanCodeShort.CONTROL
                            },
                            new Input.Key {
                                Up = false,
                                ScanCode = WindowsApi.EnumLib.ScanCodeShort.KEY_V
                            },
                            new Input.Key {
                                Up = true,
                                ScanCode = WindowsApi.EnumLib.ScanCodeShort.CONTROL
                            },
                            new Input.Key
                            {
                                Up = false,
                                ScanCode = WindowsApi.EnumLib.ScanCodeShort.RETURN
                            }
                        };
                        Input.SendKeys(keys);
                    }
                    else
                    {
                        Input.MouseClick(700, 357);

                    }
                }
            }
        }
    }
}
