using System.Diagnostics;
using System.Threading;
using noxiousET.src.guiInteraction;

namespace noxiousET.src.control
{
    class AutomationRequester
    {
        PuppetMaster puppetMaster;
        Thread automator;
        Thread executor;

        public AutomationRequester(PuppetMaster puppetMaster)
        {
            this.puppetMaster = puppetMaster;
        }

        public void automate()
        {
            if (automator != null && automator.IsAlive)
                automator.Abort();

            automator = new Thread(new ThreadStart(this.handoff));
            automator.SetApartmentState(ApartmentState.STA);
            automator.Start();
            while (!automator.IsAlive);
            Thread.Sleep(1);

            Thread executor = new Thread(new ThreadStart(this.waitForEvents));
            executor.Start();
            while (!executor.IsAlive) ;
            Thread.Sleep(1);
        }

        private void handoff()
        {
            puppetMaster.automate(999);
        }

        private void waitForEvents()
        {
            bool automatorSuspended = false;
            while (true)
            {
                if (!automatorSuspended && processExists("taskmgr"))
                {
                    automator.Suspend();
                    automatorSuspended = true;
                }
                if (processExists("calc"))
                {
                    automator.Abort();
                    Thread.CurrentThread.Abort();
                }
                if (automatorSuspended && !processExists("taskmgr"))
                {
                    try
                    {
                        automator.Resume();
                        automatorSuspended = false;
                    }
                    catch
                    {
                    }
                }
            }
        }

        private bool processExists(string processName)
        {
            try
            {
                foreach (Process p in Process.GetProcesses())
                {
                    if (p.ProcessName == processName)
                        return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }
    }
}
