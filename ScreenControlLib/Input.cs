using static WindowsApi.StructLib;
using static WindowsApi.User32;
using static WindowsApi.EnumLib;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ScreenControlLib
{
    public static class Input
    {
        public static void TypeStr(string str)
        {
            var keys = ConvertStrToKeys(str).ToArray();
            SendKeys(keys);
        }

        private static ScanCodeShort AlphabeticToScanCode(char c)
        {
            switch (c)
            {
                case 'A': 
                case 'a':
                    return ScanCodeShort.KEY_A;
                case 'B':
                case 'b':
                    return ScanCodeShort.KEY_B;
                case 'C':
                case 'c':
                    return ScanCodeShort.KEY_C;
                case 'D':
                case 'd':
                    return ScanCodeShort.KEY_D;
                case 'E':
                case 'e':
                    return ScanCodeShort.KEY_E;
                case 'F':
                case 'f':
                    return ScanCodeShort.KEY_F;
                case 'G':
                case 'g':
                    return ScanCodeShort.KEY_G;
                case 'H':
                case 'h':
                    return ScanCodeShort.KEY_H;
                case 'I':
                case 'i':
                    return ScanCodeShort.KEY_I;
                case 'J':
                case 'j':
                    return ScanCodeShort.KEY_J;
                case 'K':
                case 'k':
                    return ScanCodeShort.KEY_K;
                case 'L':
                case 'l':
                    return ScanCodeShort.KEY_L;
                case 'M':
                case 'm':
                    return ScanCodeShort.KEY_M;
                case 'N':
                case 'n':
                    return ScanCodeShort.KEY_N;
                case 'O':
                case 'o':
                    return ScanCodeShort.KEY_O;
                case 'P':
                case 'p':
                    return ScanCodeShort.KEY_P;
                case 'Q':
                case 'q':
                    return ScanCodeShort.KEY_Q;
                case 'R':
                case 'r':
                    return ScanCodeShort.KEY_R;
                case 'S':
                case 's':
                    return ScanCodeShort.KEY_S;
                case 'T':
                case 't':
                    return ScanCodeShort.KEY_T;
                case 'U':
                case 'u':
                    return ScanCodeShort.KEY_U;
                case 'V':
                case 'v':
                    return ScanCodeShort.KEY_V;
                case 'W':
                case 'w':
                    return ScanCodeShort.KEY_W;
                case 'X':
                case 'x':
                    return ScanCodeShort.KEY_X;
                case 'Y':
                case 'y':
                    return ScanCodeShort.KEY_Y;
                case 'Z':
                case 'z':
                    return ScanCodeShort.KEY_Z;
                default:
                    throw new ArgumentException($"Expecting alphabetical character obtained: '{c}'");
            }
        }

        // https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
        private static (ScanCodeShort, bool) CharToScanCode(char c)
        {
            switch (c)
            {
                case ' ':
                    return (ScanCodeShort.SPACE, false);
                case '.':
                    return (ScanCodeShort.OEM_PERIOD, false);
                case ',':
                    return (ScanCodeShort.OEM_COMMA, false);
                case '(':
                    return (ScanCodeShort.KEY_9, true);
                case ')':
                    return (ScanCodeShort.KEY_0, true);
                case ':':
                    return (ScanCodeShort.OEM_1, true);
                case '\\':
                    return (ScanCodeShort.OEM_5, false);
                case '\n':
                    return (ScanCodeShort.RETURN, false);
                case '"':
                    return (ScanCodeShort.OEM_7, true);
                case '-':
                    return (ScanCodeShort.OEM_MINUS, false);
                case '_':
                    return (ScanCodeShort.OEM_MINUS, true);
                default:
                    throw new NotSupportedException($"Not supported character '{c}'");
            }
        }

        private static ScanCodeShort DigitToScanCode(int i)
        {
            switch (i)
            {
                case 0: return ScanCodeShort.KEY_0;
                case 1: return ScanCodeShort.KEY_1;
                case 2: return ScanCodeShort.KEY_2;
                case 3: return ScanCodeShort.KEY_3;
                case 4: return ScanCodeShort.KEY_4;
                case 5: return ScanCodeShort.KEY_5;
                case 6: return ScanCodeShort.KEY_6;
                case 7: return ScanCodeShort.KEY_7;
                case 8: return ScanCodeShort.KEY_8;
                case 9: return ScanCodeShort.KEY_9;
                default:
                    throw new NotSupportedException($"Not supported digit '{i}'");
            }
        }

        private static IEnumerable<Key> ConvertStrToKeys(string str)
        {
            foreach (var c in str)
            {
                bool shift = false;
                ScanCodeShort k;
                if (char.IsLetter(c))
                {
                    k = AlphabeticToScanCode(c);
                    shift = c >= 'A' && c <= 'Z';
                }
                else if (char.IsDigit(c))
                {
                    k = DigitToScanCode(c - '0');
                }
                else
                {
                    (k, shift) = CharToScanCode(c);
                }
                if (shift)
                {                    
                    yield return new Key
                    {
                        Up = false,
                        ScanCode = ScanCodeShort.SHIFT
                    };
                    yield return new Key
                    {
                        Up = false,
                        ScanCode = k
                    };
                    yield return new Key
                    {
                        Up = true,
                        ScanCode = k
                    };
                    yield return new Key
                    {
                        Up = true,
                        ScanCode = ScanCodeShort.SHIFT
                    };
                }
                else
                {
                    yield return new Key
                    {
                        Up = false,
                        ScanCode = k
                    };
                    yield return new Key
                    {
                        Up = true,
                        ScanCode = k
                    };
                }
            }
        }

        public class Key
        {
            public ScanCodeShort ScanCode { get; set; }
            public bool Up { get; set; } = false;
        }

        public static void SendKeys(IEnumerable<Key> keys)
        {
            var input = keys.Select(x => new INPUT
            {
                type = (uint)INPUT_TYPE.INPUT_KEYBOARD,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = x.ScanCode,
                        dwFlags = KEYEVENTF.SCANCODE | (x.Up ? KEYEVENTF.KEYUP : 0)
                    }
                }
            }).ToArray();
            SendInput((uint)input.Length, input, INPUT.Size);
        }

        public static void MouseClick(int x, int y)
        {
            SetCursorPos(x, y);
            var input = new[]
            {
                new INPUT
                {
                    type = (uint)INPUT_TYPE.INPUT_MOUSE,
                    U = new InputUnion { mi = new MOUSEINPUT
                    {
                        dx = x,
                        dy = y,
                        dwFlags = MOUSEEVENTF.LEFTDOWN
                    } }
                },
                new INPUT
                {
                    type = (uint)INPUT_TYPE.INPUT_MOUSE,
                    U = new InputUnion { mi = new MOUSEINPUT
                    {
                        dx = x,
                        dy = y,
                        dwFlags = MOUSEEVENTF.LEFTUP
                    } }
                },
            };
            SendInput((uint)input.Length, input, INPUT.Size);
        }

        public static void MouseDoubleClick(int x, int y)
        {
            SetCursorPos(x, y);
            var input = new []
            {
                new INPUT
                {
                    type = (uint)INPUT_TYPE.INPUT_MOUSE,
                    U = new InputUnion { mi = new MOUSEINPUT()
                    {
                        dx = x,
                        dy = y,
                        dwFlags = MOUSEEVENTF.LEFTDOWN
                    } }
                },
                new INPUT
                {
                    type = (uint)INPUT_TYPE.INPUT_MOUSE,
                    U = new InputUnion { mi = new MOUSEINPUT()
                    {
                        dx = x,
                        dy = y,
                        dwFlags = MOUSEEVENTF.LEFTUP
                    } }
                },
                new INPUT
                {
                    type = (uint)INPUT_TYPE.INPUT_MOUSE,
                    U = new InputUnion { mi = new MOUSEINPUT
                    {
                        dx = x,
                        dy = y,
                        dwFlags = MOUSEEVENTF.LEFTDOWN,
                    } }
                },
                new INPUT
                {
                    type = (uint)INPUT_TYPE.INPUT_MOUSE,
                    U = new InputUnion { mi = new MOUSEINPUT
                    {
                        dx = x,
                        dy = y,
                        dwFlags = MOUSEEVENTF.LEFTUP,
                    } }
                },
            };
            SendInput((uint)input.Length, input, INPUT.Size);
        }
    }
}
