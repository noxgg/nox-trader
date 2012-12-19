using System.Threading;
using System.Windows.Forms;

namespace noxiousET.src.guiInteraction
{
    internal class Keyboard
    {
        private readonly Mutex _mutex;

        public Keyboard()
        {
            _mutex = new Mutex(false, EtConstants.KbMouseLock);
        }

        public void Send(string s)
        {
            _mutex.WaitOne();
            SendKeys.SendWait(s);
            _mutex.ReleaseMutex();
        }
    }
}