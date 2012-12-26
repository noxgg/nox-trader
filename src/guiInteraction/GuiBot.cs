using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using noxiousET.src.data.characters;
using noxiousET.src.data.client;
using noxiousET.src.data.paths;
using noxiousET.src.data.uidata;
using noxiousET.src.etevent;
using noxiousET.src.helpers;
using noxiousET.src.orders;

namespace noxiousET.src.guiInteraction
{
    internal class GuiBot
    {
        protected const int Left = (int) Mouse.ClickTypes.Left;
        protected const int Right = (int) Mouse.ClickTypes.Right;
        protected const int Double = (int) Mouse.ClickTypes.Double;
        protected ClientConfig ClientConfig;
        protected ErrorParser ErrorParser;
        protected IntPtr EveHandle;
        protected Keyboard Keyboard;
        protected EventDispatcher Logger;
        protected Mouse Mouse;
        protected OrderAnalyzer OrderAnalyzer;
        protected Paths Paths;
        protected int Timing;
        protected EveUi EveUi;
        private readonly int _shortContextMenuAdjustment;
        private const int OrderConfirmationTries = 5;

        public GuiBot(ClientConfig clientConfig, EveUi eveUi, Paths paths, Character character,
                      OrderAnalyzer orderAnalyzer)
        {
            ClientConfig = clientConfig;
            EveUi = eveUi;
            Paths = paths;
            Character = character;
            Logger = EventDispatcher.Instance;
            Timing = clientConfig.TimingMultiplier;
            Mouse = new Mouse(clientConfig.TimingMultiplier);
            Keyboard = new Keyboard();
            ErrorParser = new ErrorParser();
            OrderAnalyzer = orderAnalyzer;

            _shortContextMenuAdjustment = -eveUi.StandardRowHeight;
        }

        public Character Character { set; get; }

        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        protected static extern IntPtr GetForegroundWindow();

        protected void ErrorCheck()
        {
            Mouse.PointAndClick(Left, EveUi.AlertCancelButton, 0, 1, 0);
        }

        protected void CancelPrompt()
        {
            Mouse.PointAndClick(Left, EveUi.AlertCancelButton, 0, 2, 0);
        }

        protected void ConfirmPrompt()
        {
            Mouse.PointAndClick(Left, EveUi.AlertConfirmButton, 0, 2, 0);
        }

        protected void Wait(int multiplier)
        {
            Thread.Sleep(ClientConfig.TimingMultiplier * multiplier);
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
            Clipboard.SetClip(EtConstants.ClipboardNullValue);
            Mouse.PointAndClick(Right, EveUi.AlertMessageBody, 0, 1, 1);
            Mouse.OffsetAndClick(Left, EveUi.AlertContextMenuCopyOffset, 0, 1, 1);
            string message = Clipboard.GetTextFromClipboard();

            return message.Equals(EtConstants.ClipboardNullValue) ? EtConstants.ErrorNoErrorFound : ErrorParser.parse(message);
        }

        protected void ExportOrders(int tries, int waitMultiplier)
        {
            for (int i = 0; i < tries; i++)
            {
                Wait(waitMultiplier);

                ErrorCheck();
                Mouse.PointAndClick(Left, EveUi.WalletExportButton, 0, 2, 0);

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

        protected void ConfirmOrder(int[] coords, int confirmationType, bool isBuyOrder)
        {
            int failCount = 0;
            string result;

            Clipboard.SetClip(EtConstants.ClipboardNullValue);
            do
            {
                Mouse.PointAndClick(Left, coords, 1, 1, 1);
                if (confirmationType == 1 && failCount > 1)
                {
                    var errorCode = GetError();
                    if (errorCode.Equals(EtConstants.AlertInputAboveRegionalAverage) && !isBuyOrder ||
                        errorCode.Equals(EtConstants.AlertInputBelowRegionalAverage) && isBuyOrder)
                    {
                        //Sometimes the confirmation fails the first time. So retry it once.
                        ConfirmPrompt();
                        Wait(1);
                        ConfirmPrompt();
                    }
                    else
                    {
                        //Sometimes the cancellation fails the first time. So retry it once.
                        CancelPrompt();
                        Wait(1);
                        CancelPrompt();
                    }
                }

                //Right click where OK should no longer exist. Subtract 100 from double click location so we don't
                //cause double error messages to be thrown by clicking 'ok' button multiple times rapidly
                //when there is a problem confirming the order
                Mouse.PointAndClick(Double, EveUi.OrderBoxConfirm[0] - 60, EveUi.OrderBoxConfirm[1], 0, 1, 1);

                //Click on copy
                Clipboard.SetClip(EtConstants.ClipboardNullValue);
                Keyboard.Shortcut(new[] { Keyboard.VkLcontrol }, Keyboard.VkC);

                if (failCount > 1)
                    Mouse.WaitDuration *= 2;

                result = Clipboard.GetTextFromClipboard();
                ++failCount;
            } while (!result.Contains(EtConstants.OrderWindowClosedVerificationSubstring) && failCount < 5);

            Mouse.WaitDuration = Timing;

            if (result.Equals(EtConstants.ClipboardNullValue))
            {
                throw new Exception("Failed to confirm the order!");
            }
            Clipboard.SetClip(EtConstants.ClipboardNullValue);
        }

        public int KillClient()
        {
            ProcessKiller.killProcessByHandle(EveHandle);
            return 0;
        }

        public void InputValue(int tries, double timingScaleFactor, int[] inputFieldCoords, string value)
        {
            for (int i = 0; i < tries; i++)
            {
                try
                {
                    //Highlight any existing value in the field
                    Mouse.PointAndClick(Double, inputFieldCoords, 6, 2, 2);
                    Clipboard.SetClip(value);
                    Keyboard.Shortcut(new[] { Keyboard.VkLcontrol }, Keyboard.VkV);

                    VerifyFieldEquals(inputFieldCoords, value);
                    Mouse.WaitDuration = Timing;
                    return;
                }
                catch (Exception)
                {
                    Mouse.WaitDuration = Convert.ToInt32(Mouse.WaitDuration * timingScaleFactor);
                }
                
            }
            Mouse.WaitDuration = Timing;
            throw new Exception("Failed to input value " + value);
        }

        public void VerifyFieldEquals(int[] inputField, string expectedValue)
        {
            Clipboard.SetClip(EtConstants.ClipboardNullValue);
            Mouse.PointAndClick(Double, inputField, 6, 2, 2);
            Keyboard.Shortcut(new[] { Keyboard.VkLcontrol }, Keyboard.VkC);

            if (expectedValue.Equals(Clipboard.GetTextFromClipboard()) ||
                (expectedValue == Character.Account.Password &&
                    expectedValue.Length == Clipboard.GetTextFromClipboard().Length))
                return;
            throw new Exception("Expected value does not match discovered value!");
        }

        public void VerifyFieldContains(int[] inputField, string expectedValue)
        {
            Clipboard.SetClip(EtConstants.ClipboardNullValue);
            Mouse.PointAndClick(Double, inputField, 6, 2, 2);
            Keyboard.Shortcut(new[] { Keyboard.VkLcontrol }, Keyboard.VkC);

            if (Clipboard.GetTextFromClipboard().Contains(expectedValue))
                return;
            throw new Exception("Expected value does not match discovered value!");
        }
    }
}