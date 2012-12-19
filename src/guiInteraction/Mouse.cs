using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace noxiousET.src.guiInteraction
{
    internal sealed class Mouse
    {
        #region ClickTypes enum

        public enum ClickTypes
        {
            Left,
            Right,
            Double,
        };

        #endregion

        private const int MouseeventfLeftdown = 0x02;
        private const int MouseeventfLeftup = 0x04;
        private const int MouseeventfRightdown = 0x08;
        private const int MouseeventfRightup = 0x10;

        public Mouse(int waitDuration)
        {
            WaitDuration = waitDuration;
            Cursor = new Cursor(Cursor.Current.Handle);
        }

        public static ManualResetEvent SuspendEvent { set; get; }
        public int WaitDuration { get; set; }
        public Cursor Cursor { get; set; }

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public void PointAndClick(int clickType, int[] point, int before, int between, int after)
        {
            SuspendEvent.WaitOne(Timeout.Infinite);
            if (before > 0)
                Wait(before);
            PointCursor(point);
            if (between > 0)
                Wait(between);
            DoClick(clickType);
            if (after > 0)
                Wait(after);
        }

        public void Click(int clickType, int before, int after)
        {
            if (before > 0)
                Wait(before);
            DoClick(clickType);
            if (after > 0)
                Wait(after);
        }

        public void Drag(int[] startCoords, int[] endCoords, int before, int between, int after)
        {
            PointCursor(startCoords);
            Wait(before);
            mouse_event(MouseeventfLeftdown, 0, 0, 0, 0);
            Wait(between);
            PointCursor(endCoords);
            Wait(after);
            mouse_event(MouseeventfLeftup, 0, 0, 0, 0);
        }

        public void PointAndClick(int clickType, int xPoint, int yPoint, int before, int between, int after)
        {
            PointAndClick(clickType, new[] {xPoint, yPoint}, before, between, after);
        }

        public void OffsetAndClick(int clickType, int[] offset, int before, int between, int after)
        {
            PointAndClick(clickType, new[] {Cursor.Position.X + offset[0], Cursor.Position.Y + offset[1]}, before,
                          between, after);
        }

        public void OffsetAndClick(int clickType, int xPoint, int yPoint, int before, int between, int after)
        {
            PointAndClick(clickType, new[] {Cursor.Position.X + xPoint, Cursor.Position.Y + yPoint}, before, between,
                          after);
        }

        private static void DoClick(int clickType)
        {
            switch (clickType)
            {
                case 2:
                    mouse_event(MouseeventfLeftdown, 0, 0, 0, 0);
                    mouse_event(MouseeventfLeftup, 0, 0, 0, 0);
                    goto case 0;
                case 0:
                    mouse_event(MouseeventfLeftdown, 0, 0, 0, 0);
                    mouse_event(MouseeventfLeftup, 0, 0, 0, 0);
                    break;
                case 1:
                    mouse_event(MouseeventfRightdown, 0, 0, 0, 0);
                    mouse_event(MouseeventfRightup, 0, 0, 0, 0);
                    break;
            }
        }

        private static void PointCursor(int[] point)
        {
            Cursor.Position = new Point(point[0], point[1]);
        }

        private void OffsetCursor(int x, int y)
        {
            Cursor.Position = new Point(Cursor.Position.X + x, Cursor.Position.Y + y);
        }

        private void Wait(int multiplier)
        {
            Thread.Sleep(WaitDuration*multiplier);
        }
    }
}