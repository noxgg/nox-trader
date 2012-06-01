using System;
using System.Runtime.InteropServices;

namespace noxiousET.src.model.helpers
{
    class PixelReader
    {
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateDC(string strDriver, string strDevice, string strOutput, IntPtr pData);
        [DllImport("gdi32.dll")]
        static extern bool DeleteDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        static extern int GetPixel(IntPtr hdc, int nXPos, int nYPos);

        IntPtr hdcScr = CreateDC("Display", null, null, IntPtr.Zero);
        int target = 0;
        int xPos = 0;
        int yPos = 0;

        public PixelReader(int xPos, int yPos)
        {
            this.xPos = xPos;
            this.yPos = yPos;
        }

        public int setTarget()
        {
            target = GetPixel(hdcScr, xPos, yPos);
            return target;
        }

        public Boolean checkForTarget(int target)
        {
            int cr = 0;
            cr = GetPixel(hdcScr, xPos, yPos);
            if (cr == target)
                return true;
            return false;
        }
    }


}
