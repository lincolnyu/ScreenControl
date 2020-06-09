using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ScreenControlLib
{
    public static class Utility
    {
        /// <summary>
        /// Start an Action within an STA Thread
        /// </summary>
        /// <param name="goForIt"></param>
        public static void RunAsSTAThread(Action goForIt)
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
    }
}
