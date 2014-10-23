using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Timers;

namespace ZD.Gui.Zen
{
    /// <summary>
    /// Provides a timer callback service for animations.
    /// </summary>
    internal class ZenTimer
    {
        /// <summary>
        /// Owner: provides a way to the form we'll call to request repaint for controls.
        /// </summary>
        private readonly ZenControlBase parent;

        /// <summary>
        /// The system timer.
        /// </summary>
        private readonly System.Timers.Timer timer;

        /// <summary>
        /// List of current subscribers.
        /// </summary>
        private readonly List<ZenControlBase> timerSubscribers = new List<ZenControlBase>();

        /// <summary>
        /// Initializes static members and starts system timer.
        /// </summary>
        internal ZenTimer(ZenControlBase parent)
        {
            this.parent = parent;

            timer = new System.Timers.Timer(40);
            timer.AutoReset = true;
            timer.Start();
            timer.Elapsed += onTimerEvent;
        }

        /// <summary>
        /// Adds new subscriber to timer callback.
        /// </summary>
        public void Subscribe(ZenControlBase ctrl)
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
        public void Unsubscribe(ZenControlBase ctrl)
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
        private void onTimerEvent(object sender, ElapsedEventArgs e)
        {
            List<ZenControlBase> subscribers;
            lock (timerSubscribers)
            {
                subscribers = new List<ZenControlBase>(timerSubscribers);
            }
            List<ZenControlBase.ControlToPaint> ctrlsToPaint = new List<ZenControlBase.ControlToPaint>();
            foreach (ZenControlBase ctrl in subscribers)
            {
                bool? needBackground;
                RenderMode? renderMode;
                ctrl.DoTimer(out needBackground, out renderMode);
                if (renderMode != null)
                {
                    ctrlsToPaint.Add(new ZenControlBase.ControlToPaint(ctrl, needBackground.Value, renderMode.Value));
                }
            }
            // If any controls requested a pain callback, do it
            if (ctrlsToPaint.Count != 0)
                parent.MakeControlsPaint(new ReadOnlyCollection<ZenControlBase.ControlToPaint>(ctrlsToPaint));
        }
    }
}
