using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace noxiousET.src.guiInteraction
{
    class Mouse
    {
        public enum clickTypes
        {
            LEFT,
            RIGHT,
            DOUBLE,
        };

        private Mutex mutex;
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public int waitDuration { get;  set; }
        public virtual Cursor Cursor { get; set; }

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        public const int MOUSEEVENTF_RIGHTUP = 0x10;

        public Mouse(int waitDuration)
        {
            this.waitDuration = waitDuration;
            this.Cursor = new Cursor(Cursor.Current.Handle);
            this.mutex = new Mutex(false, EtConstants.KB_MOUSE_LOCK);
        }

        public void pointAndClick(int clickType, int[] point, int before, int between, int after)
        {
            mutex.WaitOne();
            if (before > 0)
                wait(before);
            pointCursor(point);
            if (between > 0)
                wait(between);
            doClick(clickType);
            if (after > 0)
                wait(after);
            mutex.ReleaseMutex();
        }

        public void click(int clickType, int before, int after)
        {
            if (before > 0)
                wait(before);
            doClick(clickType);
            if (after > 0)
                wait(after);
        }

        public void pointAndClick(int clickType, int xPoint, int yPoint, int before, int between, int after)
        {
            pointAndClick(clickType, new int[] { xPoint, yPoint }, before, between, after);
        }

        public void offsetAndClick(int clickType, int[] offset, int before, int between, int after)
        {

            pointAndClick(clickType, new int[] { Cursor.Position.X + offset[0], Cursor.Position.Y + offset[1] }, before, between, after);
        }

        public void offsetAndClick(int clickType, int xPoint, int yPoint, int before, int between, int after)
        {

            pointAndClick(clickType, new int[] { Cursor.Position.X + xPoint, Cursor.Position.Y + yPoint }, before, between, after);
        }

        private void doClick(int clickType)
        {
            switch (clickType)
            {
                case 2:
                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    goto case 0;
                case 0:
                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    break;
                case 1:
                    mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                    mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                    break;
            }
        }

        private void pointCursor(int[] point)
        {
            Cursor.Position = new Point(point[0], point[1]);
        }

        private void offsetCursor(int x, int y)
        {
            Cursor.Position = new Point(Cursor.Position.X + x, Cursor.Position.Y + y);
        }

        private void wait(int multiplier)
        {
            Thread.Sleep(waitDuration * multiplier);
        }
    }
}
