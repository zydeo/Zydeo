using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace ZD.AU
{
    internal partial class ZydeoUpdateService : ServiceBase
    {
        public ZydeoUpdateService()
        {
            InitializeComponent();
        }

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

        protected override void OnStop()
        {
        }
    }
}
