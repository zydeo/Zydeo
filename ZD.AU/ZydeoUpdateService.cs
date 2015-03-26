using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace ZD.AU
{
    /// <summary>
    /// Update helper service. All it does is re-launch from TEMP folder.
    /// </summary>
    internal partial class ZydeoUpdateService : ServiceBase
    {
        public ZydeoUpdateService()
        {
            InitializeComponent();
        }

        /// <summary>
        /// When started, re-launches from TEMP folder and stops immediately.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            if (!Helper.IsRunningFromTemp())
            {
                // Running from original location, launch ourselves from temp
                Helper.StartFromTemp();

                // Stop service
                Program.ServiceToRun.Stop();

                return;
            }
            else throw new Exception("This should not happen.");
        }

        /// <summary>
        /// Nothing particular to do when stopping.
        /// </summary>
        protected override void OnStop()
        {
        }
    }
}
