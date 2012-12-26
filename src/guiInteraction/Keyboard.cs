using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace noxiousET.src.guiInteraction
{
    internal class Keyboard
    {
        private readonly Mutex _mutex;
        public const byte VkLshift = 0xA0; // left shift key
        public const byte VkLcontrol = 0xA2;
        public const byte VkTab = 0x09;
        public const byte VkC = 0x43;
        public const byte VkV = 0x56;
        public const int KeyeventfExtendedkey = 0x01;
        public const int KeyeventfKeyup = 0x02;


        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags,
           int dwExtraInfo);

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

        public void Shortcut (byte[] modifiers, byte key)
        {
            foreach (byte b in modifiers)
            {
                keybd_event(b, 0x45, 0, 0);
                Thread.Sleep(15);
            }
            keybd_event(key, 0x45, 0, 0);
            Thread.Sleep(15);
            keybd_event(key, 0x45, KeyeventfKeyup, 0);

            foreach (byte b in modifiers)
            {
                Thread.Sleep(15);
                keybd_event(b, 0x45, KeyeventfKeyup, 0);
            }
        }

    }
}