using ScreenControlLib;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;

namespace ScreenControlConsole
{
    class Program
    {
        enum PathType
        {
            None,
            WebUrl,
            LocalFile,
        }

        class TargetWindowExited : Exception
        {
        }

        /// <summary>
        /// Start an Action within an STA Thread
        /// </summary>
        /// <param name="goForIt"></param>
        static void RunAsSTAThread(Action goForIt)
        {
            AutoResetEvent @event = new AutoResetEvent(false);
            Thread thread = new Thread(
                () =>
                {
                    goForIt();
                    @event.Set();
                });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            @event.WaitOne();
        }
        static void CtrlV()
        {
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
      
        static void Main(string[] args)
        {
            PathType pathType = PathType.None;
            string parsedPath = null;
            
            RunAsSTAThread(() =>
            {
                if (Clipboard.ContainsText())
                {
                    var s= Clipboard.GetText();
                    if (s.StartsWith("http://") && !s.StartsWith("https://"))
                    {
                        pathType = PathType.WebUrl;
                        return;
                    }
                    else if (File.Exists(s))
                    {
                        pathType = PathType.LocalFile;
                        return;
                    }
                }
                // Note The WPF version of Clipboard.GetImage() is buggy
                if (System.Windows.Forms.Clipboard.ContainsImage())
                {
                    var image = System.Windows.Forms.Clipboard.GetImage();
                    parsedPath = Path.Combine(Path.GetTempPath(), "temp-gs.png");
                    pathType = PathType.LocalFile;
                    image.Save(parsedPath, System.Drawing.Imaging.ImageFormat.Png);
                    return;
                }
                if (Clipboard.ContainsFileDropList())
                {
                    var fs = Clipboard.GetFileDropList();
                    parsedPath = fs[0];
                    pathType = PathType.LocalFile;
                }
            });

            if (pathType == PathType.None)
            {
                MessageBox.Show("Error: No valid path copied.", "GoSearch");
                return;
            }

            var w = new MyWindow
            {
                ProcessPath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
                Arguments = @"-inprivate http://images.google.com"
            };
            void SleepAndCheck(int sleepMs=500)
            {
                Thread.Sleep(500);
                if (w.Process.HasExited) throw new TargetWindowExited();
            }
            w.SwitchToOnlyOne(false);
            try
            {
                if (!w.Process.HasExited && w.WindowRect.HasValue)
                {
                    if (!w.WaitUntilTitleStartsWith("Google Images"))
                    {
                        MessageBox.Show("Error: Failed to load Google Images site.", "GoSearch");
                        return;
                    }
                    if (w.Process.HasExited) return;
                    Input.MouseClick(1200, 550);
                    SleepAndCheck();
                    if (w.Process.HasExited) return;
                    if (pathType == PathType.WebUrl)
                    {
                        Debug.Assert(parsedPath == null);
                        CtrlV();
                    }
                    else if (pathType == PathType.LocalFile)
                    {
                        Input.MouseClick(990, 650);
                        SleepAndCheck();
                        Input.MouseClick(600, 720);
                        SleepAndCheck();
                        if (parsedPath != null)
                        {
                            Input.TypeStr("\"" + parsedPath + "\"\n");
                        }
                        else
                        {
                            CtrlV();
                        }
                    }
                }
            }
            catch (TargetWindowExited)
            {
                MessageBox.Show("Error: Search window lost.", "GoSearch");
            }
        }
    }
}
