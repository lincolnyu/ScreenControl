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
            const int ItemStartIndex = 0;
            const int ItemTotalCount = 763;
            const int MaxDownloadPageAttempts = 10;
            Input.ForceCurrentWindowBeforeSend = true;
            var fdm = new ControlledWindow
            {
                ProcessPath = FdmPath
            };
            fdm.SwitchToOnlyOne(true, x => x == "Free Download Manager");
            new ControlledWindow
            {
                ProcessPath = MsEdgePath,
            }.SwitchToOnlyOne(true);
            for (var i = ItemStartIndex; i < ItemTotalCount; i++)
            {
                var url = $"http://www.pingshu8.com/down_{95241 + i}.html";
                Console.WriteLine($"Retrieving item {i}...");
                var entryPage = new ControlledWindow
                {
                    ProcessPath = MsEdgePath,
                    Arguments = url
                };
                entryPage.CreateNew(x => x.Contains("在线收听"));
                ControlledWindow downloadPage = null;
                Thread.Sleep(100);
                var attempts = 0;
                for (; downloadPage == null && attempts < MaxDownloadPageAttempts; attempts++)
                {
                    downloadPage = ControlledWindow.Capture(ControlledWindow.ProcessPathToName(MsEdgePath),
                        () =>
                        {
                            Input.MouseClick(410, 420);
                            Thread.Sleep(100);
                        });
                }
                if (downloadPage == null)
                {
                    Console.WriteLine($"Error: unable to retrieve file download page after {attempts} attempts.");
                    break;
                }
                else if (attempts > 1)
                {
                    Console.WriteLine($"Info: {attempts} attempts attempted retriving file download page.");
                }
                downloadPage.SwitchToThisWindow();
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
                    Input.SendKeys(new[]
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
                        downloadPage.SoftCloseWindow();
                        Thread.Sleep(200);
                        fdm.SwitchToThisWindow();
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
                Thread.Sleep(250);
                entryPage.SoftCloseWindow();
                Console.WriteLine($"Item {i} retrieved.");
                Thread.Sleep(1000);
            }
        }
    }
}
