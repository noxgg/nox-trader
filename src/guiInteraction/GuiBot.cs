using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using noxiousET.src.data.characters;
using noxiousET.src.data.client;
using noxiousET.src.data.paths;
using noxiousET.src.data.uielements;
using noxiousET.src.etevent;
using noxiousET.src.helpers;
using noxiousET.src.orders;

namespace noxiousET.src.guiInteraction
{
    internal class GuiBot
    {
        protected static readonly int Left = (int) Mouse.ClickTypes.Left;
        protected static readonly int Right = (int) Mouse.ClickTypes.Right;
        protected static readonly int Double = (int) Mouse.ClickTypes.Double;
        protected ClientConfig ClientConfig;
        protected Boolean ConfirmingOrderInput = true;
        protected ErrorParser ErrorParser;
        protected IntPtr EveHandle;
        protected Keyboard Keyboard;
        protected Boolean LastOrderModified;
        protected EventDispatcher Logger;
        protected Mouse Mouse;
        protected OrderAnalyzer OrderAnalyzer;
        protected Paths Paths;
        protected Boolean ShortCopyPasteMenu;
        protected int Timing;
        protected UiElements UiElements;
        private int _shortCopyPasteAdjustment;

        public GuiBot(ClientConfig clientConfig, UiElements uiElements, Paths paths, Character character,
                      OrderAnalyzer orderAnalyzer)
        {
            ClientConfig = clientConfig;
            UiElements = uiElements;
            Paths = paths;
            Character = character;
            Logger = EventDispatcher.Instance;
            Timing = clientConfig.TimingMultiplier;
            Mouse = new Mouse(clientConfig.TimingMultiplier);
            Keyboard = new Keyboard();
            ErrorParser = new ErrorParser();
            OrderAnalyzer = orderAnalyzer;
        }

        public Character Character { set; get; }

        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        protected static extern IntPtr GetForegroundWindow();

        protected int ErrorCheck()
        {
            Mouse.PointAndClick(Left, UiElements.AlertCancelButton, 0, 1, 0);
            return 0;
        }

        protected int ConfirmErrorCheck()
        {
            Mouse.PointAndClick(Left, UiElements.AlertConfirmButton, 0, 2, 0);
            return 0;
        }

        protected void Wait(int multiplier)
        {
            Thread.Sleep(ClientConfig.TimingMultiplier*multiplier);
        }

        protected bool IsEveRunningForSelectedCharacter()
        {
            SetEveHandle(Character.Name);
            return !(EveHandle == IntPtr.Zero);
        }

        protected void SetEveHandle(String character)
        {
            EveHandle = FindWindow("triuiScreen", "EVE - " + character);
        }

        protected int GetError()
        {
            Clipboard.SetClip("0");
            Mouse.PointAndClick(Right, UiElements.AlertMessageBody, 0, 1, 1);
            Mouse.OffsetAndClick(Left, UiElements.AlertContextMenuCopyOffset, 0, 1, 1);
            string message = Clipboard.GetTextFromClipboard();

            return message.Equals("0") ? 0 : ErrorParser.parse(message);
        }

        protected void ExportOrders(int tries, int waitMultiplier)
        {
            for (int i = 0; i < tries; i++)
            {
                Wait(waitMultiplier);

                ErrorCheck();
                Mouse.PointAndClick(Left, UiElements.WalletExportButton, 0, 2, 0);

                StreamReader file = null;
                try
                {
                    var directory = new DirectoryInfo(Paths.LogPath);
                    String newestFileName = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First().ToString();
                    file = new StreamReader(directory + newestFileName);
                    OrderAnalyzer.OrderSet.CreateOrderSet(directory + newestFileName, ref file,
                                                          Character.TradeHistory);
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

        protected int ConfirmOrder(int[] coords, int confirmationType, int buyOrSell)
        {
            int failCount = 0;
            string result;
            Clipboard.SetClip("0");
            do
            {
                Mouse.PointAndClick(Left, coords, 1, 1, 1);
                if (confirmationType == 1 && failCount > 1)
                {
                    int errorFlag = GetError();
                    //If the error is 'above regional average' and this is a sell order || it is below/buy
                    if (errorFlag == 1 && buyOrSell == EtConstants.Sell ||
                        errorFlag == 2 && buyOrSell == EtConstants.Buy)
                    {
                        ConfirmErrorCheck();
                        Wait(1);
                        ConfirmErrorCheck();
                    }
                    else
                    {
                        ErrorCheck();
                        Wait(1);
                        ErrorCheck();
                    }
                    Clipboard.SetClip("0");
                }

                //Right click where OK should no longer exist. 
                Mouse.PointAndClick(Right, UiElements.OrderBoxConfirm, 0, 1, 1);

                //Click on copy
                Mouse.OffsetAndClick(Left, UiElements.ChatCopyOffset, 0, 1, 1);
                if (failCount > 1)
                    Mouse.WaitDuration *= 2;
                result = Clipboard.GetTextFromClipboard();
                ++failCount;
            } while (result.Equals("0") && failCount < 5);
            Mouse.WaitDuration = Timing;
            LastOrderModified = false;
            if (!result.Equals("0"))
            {
                Clipboard.SetClip("0");
                return 0;
            }
            return 1;
        }

        public int KillClient()
        {
            ProcessKiller.killProcessByHandle(EveHandle);
            return 0;
        }

        protected void InputValue(int tries, double timingScaleFactor, int[] coords, string value)
        {
            _shortCopyPasteAdjustment = ShortCopyPasteMenu ? UiElements.StandardRowHeight : 0;
            for (int i = 0; i < tries; i++)
            {
                Mouse.PointAndClick(Double, coords, 4, 2, 2);
                Clipboard.SetClip(value);
                Mouse.Click(Right, 2, 2);
                Mouse.OffsetAndClick(Left, UiElements.ContextMenuPasteOffset[0],
                                     UiElements.ContextMenuPasteOffset[1] - _shortCopyPasteAdjustment, 0, 2, 0);
                if (VerifyInput(coords, value))
                {
                    Mouse.WaitDuration = Timing;
                    return;
                }
                Mouse.WaitDuration = Convert.ToInt32(Mouse.WaitDuration*timingScaleFactor);
            }
            Mouse.WaitDuration = Timing;
            throw new Exception("Failed to input value " + value);
        }

        private bool VerifyInput(int[] coords, string desiredValue)
        {
            Clipboard.SetClip("");
            Mouse.PointAndClick(Right, coords, 1, 1, 1);
            Mouse.OffsetAndClick(Left, UiElements.ContextMenuCopyOffset[0],
                                 UiElements.ContextMenuCopyOffset[1] - _shortCopyPasteAdjustment, 1, 1, 1);

            try
            {
                if (desiredValue.Equals(Clipboard.GetTextFromClipboard()) ||
                    (desiredValue == Character.Account.Password &&
                     desiredValue.Length == Clipboard.GetTextFromClipboard().Length))
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