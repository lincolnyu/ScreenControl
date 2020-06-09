using System;
using System.Threading;
using System.Windows.Forms;
using ScreenControlLib;
using static WindowsApi.EnumLib;

namespace Pingshu8
{
    class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <remarks>
        ///  References:
        ///  1. https://stackoverflow.com/questions/2251578/how-do-i-get-the-selected-text-from-the-focused-window-using-native-win32-api
        /// 
        /// </remarks>
        static void Main(string[] args)
        {
            // http://www.pingshu8.com/down_95241.html
            // http://www.pingshu8.com/down_96003.html
            const string MsEdgePath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";
            const string FdmPath = @"C:\Program Files\FreeDownloadManager.ORG\Free Download Manager\fdm.exe";
            const int EpisodeStart = 100;
            const int EpisodeCount = 763;
            var fdm = new ControlledWindow
            {
                ProcessPath = FdmPath
            };
            fdm.SwitchToOnlyOne(true, x => x == "Free Download Manager");
            new ControlledWindow
            {
                ProcessPath = MsEdgePath,
            }.SwitchToOnlyOne(true);
            for (var i = EpisodeStart; i < EpisodeCount; i++)
            {
                var url = $"http://www.pingshu8.com/down_{95241 + i}.html";
                Console.WriteLine($"Retrieving episode {i}");
                var w = new ControlledWindow
                {
                    ProcessPath = MsEdgePath,
                    Arguments = url
                };
                w.CreateNew(x => x.Contains("在线收听"));
                var dlw = ControlledWindow.Capture(ControlledWindow.ProcessPathToName(MsEdgePath), 
                    () =>
                    {
                        Thread.Sleep(100);
                        Input.MouseClick(410, 420);
                        Thread.Sleep(100);
                    });
                if (dlw != null)
                {
                    dlw.SwitchToWindow();
                    Input.SendKeys(
                        new[]
                        {
                            new Input.Key
                            {
                                ScanCode = ScanCodeShort.LMENU
                            },
                            new Input.Key
                            {
                                ScanCode = ScanCodeShort.KEY_D
                            },
                            new Input.Key
                            {
                                ScanCode = ScanCodeShort.KEY_D,
                                Up = true
                            },
                            new Input.Key
                            {
                                ScanCode = ScanCodeShort.LMENU,
                                Up = true
                            }
                    });
                    Utility.RunAsSTAThread(() =>
                    {
                        Clipboard.Clear();
                    });
                    Thread.Sleep(200);
                    while (true)
                    {
                        Input.SendKeys(
                        new[]
                        {
                            new Input.Key
                            {
                                ScanCode = ScanCodeShort.LCONTROL
                            },
                            new Input.Key
                            {
                                ScanCode = ScanCodeShort.KEY_C
                            },
                            new Input.Key
                            {
                                ScanCode = ScanCodeShort.KEY_C,
                                Up = true
                            },
                            new Input.Key
                            {
                                ScanCode = ScanCodeShort.LCONTROL,
                                Up = true
                            }
                        });
                        Thread.Sleep(200);
                        string text = "";
                        Utility.RunAsSTAThread(() =>
                        {
                            text = Clipboard.GetText();
                        });
                        if (text.Contains("down01.pingshu88.com"))
                        {
                            dlw.SoftCloseWindow();
                            Thread.Sleep(200);
                            fdm.SwitchToWindow();
                            Thread.Sleep(100);
                            Input.MouseClick(40, 40);
                            Thread.Sleep(100);
                            Input.SendKeys(new[]
                            {
                                new Input.Key
                                {
                                    ScanCode = ScanCodeShort.RETURN
                                },
                                new Input.Key
                                {
                                    ScanCode = ScanCodeShort.RETURN,
                                    Up = true
                                }
                            });
                            Input.SendKeys(new[]
                            {
                                new Input.Key
                                {
                                    ScanCode = ScanCodeShort.RETURN
                                },
                                new Input.Key
                                {
                                    ScanCode = ScanCodeShort.RETURN,
                                    Up = true
                                }
                            });
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }
                Thread.Sleep(500);
                w.SoftCloseWindow();
                Thread.Sleep(500);
            }
        }
    }
}
