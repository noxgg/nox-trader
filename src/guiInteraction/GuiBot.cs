using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using noxiousET.src.data.characters;
using noxiousET.src.data.client;
using noxiousET.src.data.paths;
using noxiousET.src.data.uielements;
using noxiousET.src.helpers;


namespace noxiousET.src.guiInteraction
{
    class GuiBot
    {
        [DllImport("user32.dll")] 
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        protected static extern IntPtr GetForegroundWindow();

        public Character character { set; get; }
        protected ClientConfig clientConfig;
        protected UiElements uiElements;
        protected Paths paths;
        protected Mouse mouse;
        protected IntPtr eveHandle;
        protected ErrorParser errorParser;
        protected int timingMultiplier;
        protected static readonly int stopAllActivity = -69;
        protected Boolean lastOrderModified = false;
        protected OrderManager orderSet;

        public GuiBot(ClientConfig clientConfig, UiElements uiElements, Paths paths, Character character)
        {
            this.clientConfig = clientConfig;
            this.timingMultiplier = clientConfig.timingMultiplier;
            this.uiElements = uiElements;
            this.paths = paths;
            this.character = character;
            mouse = new Mouse(timingMultiplier);
            errorParser = new ErrorParser();
        }

        protected int errorCheck()
        {
            mouse.pointCursor(uiElements.errorCheck[0], uiElements.errorCheck[1]);
            mouse.leftClick(2);
            return stopCheck();
        }

        protected int stopCheck()
        {
            foreach (Process p in Process.GetProcesses())
            {
                if (p.ProcessName == "taskmgr")
                    return stopAllActivity;
            }
            return 0;
        }

        protected int confirmErrorCheck()
        {
            mouse.pointCursor(uiElements.confirmErrorCheck[0], uiElements.confirmErrorCheck[1]);
            mouse.leftClick(2);
            return stopCheck();
        }

        protected void wait(int multiplier)
        {
            Thread.Sleep(timingMultiplier * multiplier);
        }

        protected bool isEVERunning()
        {
            eveHandle = FindWindow("triuiScreen", "EVE - " + character.name);

            if (eveHandle == IntPtr.Zero)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        protected int getError()
        {
            string message;
            mouse.pointCursor(uiElements.parseErrorMessage[0], uiElements.parseErrorMessage[1]);
            mouse.rightClick(1, 1);
            mouse.offsetCursor(uiElements.parseErrorMessageCopyOffset[0], uiElements.parseErrorMessageCopyOffset[1]);
            mouse.leftClick(1, 1);
            message = Clipboard.GetTextFromClip();

            if (string.Compare(message, "0") != 0)
                return errorParser.parse(message);
            return 0;
        }

        protected int exportOrders()
        {
            errorCheck();
            mouse.pointCursor(uiElements.exportOrderList[0], uiElements.exportOrderList[1]);
            mouse.leftClick(2);

            string fileName;

            var directory = new DirectoryInfo(paths.logPath);
            var fileTemp = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
            StreamReader file;

            try
            {
                fileTemp = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
                file = new System.IO.StreamReader(directory.ToString() + fileTemp.ToString());

            }
            catch
            {
                return 1;
            }
            fileName = fileTemp.ToString();
            orderSet = new OrderManager(directory.ToString() + fileName, ref file);

            file.Close();
            return 0;
        }

        protected int confirmOrder(int longOrderXoffset, int longOrderYOffset, int typeID, int confirmationType, int buyOrSell)
        {
            int failCount = 0;
            string result = "0";
            int errorFlag = 0;
            do
            {
                wait(1);
                mouse.pointCursor(uiElements.OrderBoxOK[0], uiElements.OrderBoxOK[1] + longOrderYOffset);
                mouse.leftClick(1, 1);
                if (confirmationType == 1 && failCount > 3)
                {
                    errorFlag = getError();
                    //If the error is 'above regional average' and this is a sell order || it is below/buy
                    if (errorFlag == 1 && buyOrSell == 0 || errorFlag == 2 && buyOrSell == 1)
                    {
                        if (confirmErrorCheck() == stopAllActivity)
                            return stopAllActivity;
                        wait(1);
                        if (confirmErrorCheck() == stopAllActivity)
                            return stopAllActivity;
                    }
                    else
                    {
                        if (errorCheck() == stopAllActivity)
                            return stopAllActivity;
                        wait(1);
                        if (errorCheck() == stopAllActivity)
                            return stopAllActivity;
                    }
                    Clipboard.setClipboardText("0");
                }

                //Right click where OK should no longer exist. 
                mouse.pointCursor(uiElements.OrderBoxOK[0] + longOrderXoffset, uiElements.OrderBoxOK[1]);
                mouse.rightClick(1, 1);

                //Click on copy
                mouse.offsetCursor(uiElements.confirmationCopyOffset[0], uiElements.confirmationCopyOffset[1]);
                mouse.leftClick(1, 1);

                result = Clipboard.GetTextFromClip();
                ++failCount;
            } while (string.Compare(result, "0") == 0 && failCount < 9);
            lastOrderModified = false;
            if (string.Compare(result, "0") != 0)
            {
                Clipboard.setClipboardText("0");
                return 0;
            }
            else
                return 1;
        }

        public int killClient()
        {
            ProcessKiller.killProcessByHandle(eveHandle);
            return 0;
        }
    }
}
