using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using noxiousET.src.etevent;
using noxiousET.src.data.characters;
using noxiousET.src.data.client;
using noxiousET.src.data.paths;
using noxiousET.src.data.uielements;
using noxiousET.src.helpers;
using noxiousET.src.orders;


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
        protected Keyboard keyboard;
        protected IntPtr eveHandle;
        protected ErrorParser errorParser;
        protected Boolean lastOrderModified = false;
        protected EventDispatcher logger;
        protected OrderAnalyzer orderAnalyzer;
        protected int timing;
        protected static readonly int LEFT = (int)Mouse.clickTypes.LEFT;
        protected static readonly int RIGHT = (int)Mouse.clickTypes.RIGHT;
        protected static readonly int DOUBLE = (int)Mouse.clickTypes.DOUBLE;
        protected Boolean shortCopyPasteMenu = false;
        protected Boolean confirmingOrderInput = true;
        private int shortCopyPasteAdjustment = 0;
        private int confirmingOrderAdjustment = 0;

        public GuiBot(ClientConfig clientConfig, UiElements uiElements, Paths paths, Character character, OrderAnalyzer orderAnalyzer)
        {
            this.clientConfig = clientConfig;
            this.uiElements = uiElements;
            this.paths = paths;
            this.character = character;
            logger = EventDispatcher.Instance;
            timing = clientConfig.timingMultiplier;
            mouse = new Mouse(clientConfig.timingMultiplier);
            this.keyboard = new Keyboard();
            errorParser = new ErrorParser();
            this.orderAnalyzer = orderAnalyzer;
        }

        protected int errorCheck()
        {
            mouse.pointAndClick(LEFT, uiElements.errorCheck, 0, 1, 0);
            return 0;
        }

        protected int confirmErrorCheck()
        {
            mouse.pointAndClick(LEFT, uiElements.confirmErrorCheck, 0, 2, 0);
            return 0;
        }

        protected void wait(int multiplier)
        {
            Thread.Sleep(clientConfig.timingMultiplier * multiplier);
        }

        protected bool isEVERunningForSelectedCharacter()
        {
            setEVEHandle(character.name);
            return !(eveHandle == IntPtr.Zero);
        }

        protected void setEVEHandle(String character)
        {
            eveHandle = FindWindow("triuiScreen", "EVE - " + character);
        }

        protected int getError()
        {
            Clipboard.setClip("0");
            mouse.pointAndClick(RIGHT, uiElements.parseErrorMessage, 0, 1, 1);
            mouse.offsetAndClick(LEFT, uiElements.parseErrorMessageCopyOffset, 0, 1, 1);
            string message = Clipboard.getTextFromClipboard();

            if (string.Compare(message, "0") != 0)
                return errorParser.parse(message);
            return 0;
        }

        protected void exportOrders(int tries, int waitMultiplier)
        {
            for (int i = 0; i < tries; i++)
            {
                wait(waitMultiplier);

                errorCheck();
                mouse.pointAndClick(LEFT, uiElements.exportOrderList, 0, 2, 0);

                StreamReader file = null;
                try
                {
                    var directory = new DirectoryInfo(paths.logPath);
                    var fileTemp = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
                    fileTemp = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
                    file = new StreamReader(directory.ToString() + fileTemp.ToString());
                    orderAnalyzer.orderSet.createOrderSet(directory.ToString() + fileTemp.ToString(), ref file, character.tradeHistory);
                    return;
                }
                catch (Exception)
                {
                    if (file != null)
                        file.Close();
                }
            }
            throw new Exception("Failed to export orders");
        }

        protected int confirmOrder(int[]coords, int confirmationType, int buyOrSell)
        {
            int failCount = 0;
            string result = "0";
            int errorFlag = 0;
            Clipboard.setClip("0");
            do
            {
                mouse.pointAndClick(LEFT, coords, 1, 1, 1);
                if (confirmationType == 1 && failCount > 1)
                {
                    errorFlag = getError();
                    //If the error is 'above regional average' and this is a sell order || it is below/buy
                    if (errorFlag == 1 && buyOrSell == EtConstants.SELL || errorFlag == 2 && buyOrSell == EtConstants.BUY)
                    {
                        confirmErrorCheck();
                        wait(1);
                        confirmErrorCheck();
                    }
                    else
                    {
                        errorCheck();
                        wait(1);
                        errorCheck();
                    }
                    Clipboard.setClip("0");
                }

                //Right click where OK should no longer exist. 
                mouse.pointAndClick(RIGHT, uiElements.OrderBoxOK, 0, 1, 1);

                //Click on copy
                mouse.offsetAndClick(LEFT, uiElements.confirmationCopyOffset, 0, 1, 1);
                if (failCount > 1)
                    mouse.waitDuration *= 2;
                result = Clipboard.getTextFromClipboard();
                ++failCount;
            } while (string.Compare(result, "0") == 0 && failCount < 5);
            mouse.waitDuration = timing;
            lastOrderModified = false;
            if (string.Compare(result, "0") != 0)
            {
                Clipboard.setClip("0");
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
        
        protected void inputValue(int tries, double timingScaleFactor, int[] coords, string value)
        {
            shortCopyPasteAdjustment = shortCopyPasteMenu ? uiElements.lineHeight : 0;
            confirmingOrderAdjustment = confirmingOrderInput ? uiElements.confirmingOrderAdjustment : 0;
            for (int i = 0; i < tries; i++)
            {
                mouse.pointAndClick(DOUBLE, coords, 4, 2, 2);
                Clipboard.setClip(value);
                mouse.click(RIGHT, 2, 2);
                mouse.offsetAndClick(LEFT, uiElements.pasteOffset[0] + confirmingOrderAdjustment, uiElements.pasteOffset[1] - shortCopyPasteAdjustment, 0, 2, 0);
                if (verifyInput(coords, value))
                {
                    mouse.waitDuration = timing;
                    return;
                }
                mouse.waitDuration = Convert.ToInt32(mouse.waitDuration * timingScaleFactor);
            }
            mouse.waitDuration = timing;
            throw new Exception("Failed to input value " + value);
        }

        private bool verifyInput(int[] coords, string desiredValue)
        {
            Clipboard.setClip("");
            mouse.pointAndClick(RIGHT, coords, 1, 1, 1);
            mouse.offsetAndClick(LEFT, uiElements.copyOffset[0] + confirmingOrderAdjustment, uiElements.copyOffset[1] - shortCopyPasteAdjustment, 1, 1, 1);

            try
            {
                if (desiredValue.Equals(Clipboard.getTextFromClipboard()) || 
                    (desiredValue == character.account.p && desiredValue.Length == Clipboard.getTextFromClipboard().Length))
                    return true;
            }
            catch
            {
                return false;
            }
            return false;
        }
    }
}
