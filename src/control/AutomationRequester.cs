using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using noxiousET.src.guiInteraction;

namespace noxiousET.src.control
{
    class AutomationRequester
    {
        PuppetMaster puppetMaster;
        Thread automator;
        Hotkey pauseKey;
        Hotkey unpauseKey;
        Hotkey terminateKey;
        Control control;

        public AutomationRequester(PuppetMaster puppetMaster)
        {
            this.puppetMaster = puppetMaster;
            this.pauseKey = new Hotkey(Keys.Z, true, true, false, false);
            pauseKey.Pressed += delegate { this.pause(); };
            this.unpauseKey = new Hotkey(Keys.X, true, true, false, false);
            unpauseKey.Pressed += delegate { this.unpause(); };
            this.terminateKey = new Hotkey(Keys.Q, true, true, true, false);
            terminateKey.Pressed += delegate { this.terminate(); };

            this.control = new Control();
            pauseKey.Register(control);
            unpauseKey.Register(control);
            terminateKey.Register(control);

        }

        public void automate()
        {
            if (automator != null && automator.IsAlive)
                automator.Abort();

            automator = new Thread(new ThreadStart(this.handoff));
            automator.SetApartmentState(ApartmentState.STA);
            automator.Start();
        }

        private void handoff()
        {
            puppetMaster.automate(999);
        }

        private void pause()
        {
            Mouse.suspendEvent.Reset();
        }

        private void unpause()
        {
            Mouse.suspendEvent.Set();
        }

        private void terminate()
        {
            automator.Abort();
            unpause();
        }
    }
}
