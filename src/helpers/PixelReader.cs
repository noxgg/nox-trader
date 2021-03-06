﻿using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace noxiousET.src.helpers
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

        public PixelReader()
        {
            
        }

        public int SetTarget()
        {
            target = GetPixel(hdcScr, xPos, yPos);
            return target;
        }

        public Boolean CheckForTarget(int target)
        {
            int cr = 0;
            cr = GetPixel(hdcScr, xPos, yPos);
            if (cr == target)
                return true;
            return false;
        }

        public int GetPixelColor(int x, int y)
        {
            return GetPixel(hdcScr, x, y);
        }

        public string GetPixelHexColor(int x, int y)
        {
            int pixel = GetPixel(hdcScr, x, y);

            Color color = Color.FromArgb((pixel & 0x000000FF),
                    (pixel & 0x0000FF00) >> 8,
                    (pixel & 0x00FF0000) >> 16);
            return ColorTranslator.ToHtml(color);
        }
    }


}
