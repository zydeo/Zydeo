using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace DND.Gui.Zen
{
    /// <summary>
    /// Provides a timer callback service for animations.
    /// </summary>
    internal static class ZenTimer
    {
        /// <summary>
        /// The system timer.
        /// </summary>
        private static readonly System.Timers.Timer timer;

        /// <summary>
        /// List of current subscribers.
        /// </summary>
        private static List<ZenControlBase> timerSubscribers;

        /// <summary>
        /// Initializes static members and starts system timer.
        /// </summary>
        static ZenTimer()
        {
            timerSubscribers = new List<ZenControlBase>();
            timer = new System.Timers.Timer(40);
            timer.AutoReset = true;
            timer.Start();
            timer.Elapsed += onTimerEvent;
        }

        /// <summary>
        /// Adds new subscriber to timer callback.
        /// </summary>
        public static void SubscribeToTimer(ZenControlBase ctrl)
        {
            lock (timerSubscribers)
            {
                if (!timerSubscribers.Contains(ctrl))
                    timerSubscribers.Add(ctrl);
            }
        }

        /// <summary>
        /// Removes subscriber from timer callback.
        /// </summary>
        public static void UnsubscribeFromTimer(ZenControlBase ctrl)
        {
            lock (timerSubscribers)
            {
                if (timerSubscribers.Contains(ctrl))
                    timerSubscribers.Remove(ctrl);
            }
        }

        /// <summary>
        /// Invoked by system timer callback. Calls each subscriber's timer function.
        /// </summary>
        private static void onTimerEvent(object sender, ElapsedEventArgs e)
        {
            List<ZenControlBase> subscribers;
            lock (timerSubscribers)
            {
                subscribers = new List<ZenControlBase>(timerSubscribers);
            }
            foreach (ZenControlBase ctrl in subscribers)
            {
                ctrl.DoTimer();
            }
        }

    }
}
