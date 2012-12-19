using System.Threading;
using System.Windows.Forms;
using noxiousET.src.guiInteraction;

namespace noxiousET.src.control
{
    internal class AutomationRequester
    {
        private readonly Control _control;
        private readonly Hotkey _pauseKey;
        private readonly PuppetMaster _puppetMaster;
        private readonly Hotkey _terminateKey;
        private readonly Hotkey _unpauseKey;
        private Thread _automator;

        public AutomationRequester(PuppetMaster puppetMaster)
        {
            _puppetMaster = puppetMaster;
            _pauseKey = new Hotkey(Keys.Z, true, true, false, false);
            _pauseKey.Pressed += delegate { Pause(); };
            _unpauseKey = new Hotkey(Keys.X, true, true, false, false);
            _unpauseKey.Pressed += delegate { Unpause(); };
            _terminateKey = new Hotkey(Keys.Q, true, true, true, false);
            _terminateKey.Pressed += delegate { Terminate(); };

            _control = new Control();
            _pauseKey.Register(_control);
            _unpauseKey.Register(_control);
            _terminateKey.Register(_control);
        }

        public void Automate()
        {
            if (_automator != null && _automator.IsAlive)
                _automator.Abort();

            _automator = new Thread(Handoff);
            _automator.SetApartmentState(ApartmentState.STA);
            _automator.Start();
        }

        private void Handoff()
        {
            _puppetMaster.Automate(999);
        }

        private static void Pause()
        {
            Mouse.SuspendEvent.Reset();
        }

        private static void Unpause()
        {
            Mouse.SuspendEvent.Set();
        }

        private void Terminate()
        {
            _automator.Abort();
            Unpause();
        }
    }
}