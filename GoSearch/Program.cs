using ScreenControlLib;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using Clipboard = System.Windows.Forms.Clipboard;
using MessageBox = System.Windows.MessageBox;

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

        struct SizeInt
        {
            public int Width;
            public int Height;
            public SizeInt(int width, int height)
            {
                Width = width;
                Height = height;
            }
        }

        struct PointInt
        {
            public int X;
            public int Y;
            public PointInt(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        class GImgClicks
        {
            public SizeInt ScreenResolution;
            public PointInt CameraButton;
            public PointInt UploadImageTab;
            public PointInt ChooseFileButton;
        }

        static GImgClicks[] GImgClicksSupported = new []
        {
            new GImgClicks
            {
                ScreenResolution = new SizeInt(1920, 1080),
                CameraButton = new PointInt(1200, 550),
                UploadImageTab = new PointInt(990, 650),
                ChooseFileButton = new PointInt(660, 720)
            },
            new GImgClicks
            {
                ScreenResolution = new SizeInt(1680, 1050),
                CameraButton = new PointInt(1000, 420),
                UploadImageTab = new PointInt(829, 479),
                ChooseFileButton = new PointInt(621, 526)
            }
        };

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

            var screenBounds= Screen.PrimaryScreen.Bounds;
            GImgClicks gimgClicks = null;
            foreach (var supported in GImgClicksSupported)
            {
                if (supported.ScreenResolution.Width == screenBounds.Width && supported.ScreenResolution.Height == screenBounds.Height)
                {
                    gimgClicks = supported;
                    break;
                }
            }
            if (gimgClicks == null)
            {
                MessageBox.Show("Error: Screen resolution not supported.", "GoSearch");
                return;
            }

            Utility.RunAsSTAThread(() =>
            {
                if (Clipboard.ContainsText())
                {
                    var s= Clipboard.GetText();
                    if (s.StartsWith("http://") || s.StartsWith("https://"))
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
                if (Clipboard.ContainsImage())
                {
                    var image = Clipboard.GetImage();
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

            var w = new ControlledWindow
            {
                ProcessPath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
                Arguments = @"-inprivate http://images.google.com"
            };
            void SleepAndCheck(int sleepMs=500)
            {
                Thread.Sleep(500);
                if (w.Process.HasExited) throw new TargetWindowExited();
            }
            if (!w.CreateNew(x=>x.StartsWith("Google Images")))
            {
                MessageBox.Show("Error: Failed to create new browser window.", "GoSearch");
                return;
            }
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

                    //var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;                    
                    //const int StandardResolutionWidth = 1920; 
                    //const int StandardResolutionHeight = 1080;
                    //const int StandardClickX = 1200;
                    //const int StandardClickY = 550;
                    //if (bounds.Width == StandardResolutionHeight && bounds.Height  == StandardResolutionWidth)
                    //{
                    //    Input.MouseClick(1200, 550);
                    //}
                    //else
                    //{
                    //    var x = StandardClickX * bounds.Width / StandardResolutionWidth;
                    //    var y = StandardClickY * bounds.Height / StandardResolutionHeight;
                    //    Input.MouseClick(x, y);
                    //}
                    Input.MouseClick(gimgClicks.CameraButton.X, gimgClicks.CameraButton.Y);

                    SleepAndCheck();
                    if (w.Process.HasExited) return;
                    if (pathType == PathType.WebUrl)
                    {
                        Debug.Assert(parsedPath == null);
                        CtrlV();
                    }
                    else if (pathType == PathType.LocalFile)
                    {
                        Input.MouseClick(gimgClicks.UploadImageTab.X, gimgClicks.UploadImageTab.Y);
                        SleepAndCheck();
                        Input.MouseClick(gimgClicks.ChooseFileButton.X, gimgClicks.ChooseFileButton.Y);
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
