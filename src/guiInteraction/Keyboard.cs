using System.Threading;
using System.Windows.Forms;

namespace noxiousET.src.guiInteraction
{
    class Keyboard
    {
        private Mutex mutex;

        public Keyboard()
        {
            this.mutex = new Mutex(false, EtConstants.KB_MOUSE_LOCK);
        }
        public void send(string s)
        {
            mutex.WaitOne();
            SendKeys.SendWait(s);
            mutex.ReleaseMutex();
        }
    }
}
