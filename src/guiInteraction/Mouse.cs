using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace noxiousET.src.guiInteraction
{
    class Mouse
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        private int waitDuration { get;  set; }
        public virtual Cursor Cursor { get; set; }

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        public const int MOUSEEVENTF_RIGHTUP = 0x10;

        public Mouse(int waitDuration)
        {
            this.waitDuration = waitDuration;
            this.Cursor = new Cursor(Cursor.Current.Handle);
        }

        public void pointCursor(int x, int y)
        {
            Cursor.Position = new Point(x, y);
        }

        public void offsetCursor(int x, int y)
        {
            Cursor.Position = new Point(Cursor.Position.X + x, Cursor.Position.Y + y);
        }

        public void leftClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        public void leftClick(int before, int after)
        {
            wait(before);
            leftClick();
            wait(after);
        }

        public void leftClick(int before)
        {
            wait(before);
            leftClick();
        }

        public void doubleClick()
        {
            leftClick();
            leftClick();
        }

        public void doubleClick(int before, int after)
        {
            wait(before);
            doubleClick();
            wait(after);
        }

        public void doubleClick(int before)
        {
            wait(before);
            doubleClick();
        }

        public void rightClick()
        {
            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
        }

        public void rightClick(int before, int after)
        {
            wait(before);
            rightClick();
            wait(after);
        }

        public void rightClick(int before)
        {
            wait(before);
            rightClick();
        }

        private void wait(int multiplier)
        {
            Thread.Sleep(waitDuration * multiplier);
        }
    }
}
