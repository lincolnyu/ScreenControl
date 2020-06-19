using System;
using System.Threading;
using System.Windows.Forms;

namespace GlobalMouse
{
    class Program
    {
        static void Main()
        {
            while (true)
            {
                Console.WriteLine($"{Cursor.Position.X}, { Cursor.Position.Y}");
                Thread.Sleep(500);
            }
        }
    }
}
