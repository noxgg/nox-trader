using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using noxiousET.src.data.characters;
using noxiousET.src.data.client;
using noxiousET.src.data.paths;
using noxiousET.src.data.uielements;
using noxiousET.src.helpers;
using noxiousET.src.orders;

namespace noxiousET.src.guiInteraction.login
{
    internal class LoginBot : GuiBot
    {
        private readonly PixelReader _pixelReader;

        public LoginBot(ClientConfig clientConfig, UiElements uiElements, Paths paths, Character character,
                        OrderAnalyzer orderAnalyzer
            ) : base(clientConfig, uiElements, paths, character, orderAnalyzer)
        {
            _pixelReader = new PixelReader(uiElements.CharacterSelectActiveSlot[0] - 5,
                                          uiElements.CharacterSelectActiveSlot[1] - 5);
        }

        private void LaunchClient()
        {
            EveHandle = FindWindow("triuiScreen", "EVE");
            Process[] proc = Process.GetProcessesByName("EXEFile");
            if (!(proc.Count() == 1 && EveHandle != IntPtr.Zero))
            {
                ProcessKiller.killProcess("EXEFile");
                Process.Start(Paths.ClientPath);
            }
        }

        public int Login(Character character)
        {
            this.Character = character;

            if (!IsEveRunningForSelectedCharacter())
            {
                try
                {
                    LaunchClient();
                    swapInUserSettings(character);
                    enterCredentials();
                    selectCharacter();
                    waitForEnvironment();
                }
                catch (Exception e)
                {
                    Logger.Log(character.Name + e.Message);
                    return 1;
                }
            }
            else
            {
                waitForEnvironment();
            }
            return 0;
        }

        private void swapInUserSettings(Character character)
        {
            String characterFilePrefix = "core_char_";
            String userFilePrefix = "core_user_";
            String fileSuffix = ".dat";

            String source = Paths.ConfigPath + Paths.ClientSettingsSubDir + characterFilePrefix + character.Id +
                            fileSuffix;
            String destination = Paths.EveSettingsPath + characterFilePrefix + character.Id + fileSuffix;
            File.Copy(source, destination, true);

            source = Paths.ConfigPath + Paths.ClientSettingsSubDir + userFilePrefix + character.Account.Id +
                     character.Name + fileSuffix;
            destination = Paths.EveSettingsPath + userFilePrefix + character.Account.Id + fileSuffix;
            File.Copy(source, destination, true);
        }

        private void enterCredentials()
        {
            int failCount = 0;
            EveHandle = IntPtr.Zero;
            while (EveHandle == IntPtr.Zero && failCount < 60)
            {
                EveHandle = FindWindow("triuiScreen", "EVE");
                Thread.Sleep(1000);
                failCount++;
            }
            if (EveHandle == IntPtr.Zero)
                throw new Exception("Error logging in. Could not find client.");
            Clipboard.SetClip("0");
            ErrorCheck();
            for (int i = 0; i < 10; ++i)
            {
                try
                {
                    ErrorCheck();
                    SetForegroundWindow(EveHandle);
                    //ProcessKiller.killProcess("Chrome");
                    ShortCopyPasteMenu = true;
                    ConfirmingOrderInput = false;
                    InputValue(5, 1, UiElements.LoginUserNameField, Character.Account.UserName);
                    InputValue(5, 1, UiElements.LoginPasswordField, Character.Account.Password);
                    Keyboard.Send("{ENTER}");
                    ShortCopyPasteMenu = false;
                    ConfirmingOrderInput = true;
                    Thread.Sleep(5000);
                    identifyCharacterSelectionWindow();
                    return;
                }
                catch
                {
                }
            }
            ShortCopyPasteMenu = false;
            ConfirmingOrderInput = true;
            ProcessKiller.killProcess("EXEFile");
            throw new Exception("Error logging in. Failed to enter credentials.");
        }

        private void identifyCharacterSelectionWindow()
        {
            for (int i = 0; i < 20; ++i)
            {
                if (GetError() > 0)
                    throw new Exception("Found error message");
                Clipboard.SetClip("");
                Mouse.PointAndClick(Right, UiElements.CharacterSelectTip, 5, 5, 10);
                //TODO: Fix hack in y coord
                Mouse.OffsetAndClick(Left, UiElements.ContextMenuCopyOffset[0],
                                     UiElements.ContextMenuCopyOffset[1] - UiElements.StandardRowHeight, 5, 5, 5);
                if (Clipboard.GetTextFromClipboard().CompareTo("") != 0)
                    return;
                Thread.Sleep(1000);
            }
            ProcessKiller.killProcess("EXEFile");
            throw new Exception("Error logging in. Could not find character select screen.");
        }

        private int selectCharacter()
        {
            int result = 1;
            int errorFlag = 0;
            for (int i = 0; i < 20; ++i)
            {
                SetForegroundWindow(EveHandle);
                result = findCharacter();
                if (result == 0)
                {
                    //ProcessKiller.killProcess("Chrome");
                    return 0;
                }
                errorFlag = GetError();
                if (errorFlag != 0)
                {
                    ErrorCheck();
                    Mouse.Click(Double, 0, 0);
                    Thread.Sleep(1000);
                    return 1;
                }
            }
            ProcessKiller.killProcess("EXEFile");
            throw new Exception("Error logging in. Could not find character select screen.");
        }

        private int findCharacter()
        {
            if (_pixelReader.checkForTarget(Character.LoginColor))
            {
                //pick this character if it is the right one.
                Mouse.PointAndClick(Left, UiElements.CharacterSelectActiveSlot, 0, 10, 2);
                return 0;
            }

            //select alt2
            Mouse.PointAndClick(Left, UiElements.CharacterSelectSlot3, 0, 10, 2);
            Wait(5);

            if (_pixelReader.checkForTarget(Character.LoginColor))
            {
                //pick this character if it is the right one.
                Mouse.PointAndClick(Left, UiElements.CharacterSelectActiveSlot, 0, 10, 2);
                return 0;
            }
            //Select alt1
            Mouse.PointAndClick(Left, UiElements.CharacterSelectSlot2, 0, 10, 2);
            Wait(5);

            //Check new character
            if (_pixelReader.checkForTarget(Character.LoginColor))
            {
                //pick this character if it is the right one.
                Mouse.PointAndClick(Left, UiElements.CharacterSelectActiveSlot, 0, 10, 2);
                return 0;
            }
            Wait(5);
            return 1;
        }


        private int waitForEnvironment()
        {
            SetEveHandle(Character.Name);
            SetForegroundWindow(EveHandle);
            for (int i = 0; i < 20; i++)
            {
                Mouse.PointAndClick(Left, UiElements.CharacterSelectActiveSlot, 0, 10, 2);
                Wait(2);
                Clipboard.SetClip("0");
                Wait(2);
                try
                {
                    if (ConfirmOrder(UiElements.OrderBoxCancel, 0, 0) == 0) //TODO refactor this out of here
                        return 0;
                    Keyboard.Send("-1");
                }
                catch (Exception)
                {
                    Keyboard.Send("-1");
                }
                Thread.Sleep(1000);
            }
            ProcessKiller.killProcess("EXEFile");
            throw new Exception("Error Logging in. Failed to find environment.");
        }

        private string getLoginText()
        {
            return Character.Account.UserName;
        }

        private bool atLoginScreen()
        {
            EveHandle = FindWindow("triuiScreen", "EVE");
            SetForegroundWindow(EveHandle);
            Wait(2);
            Clipboard.SetClip("0");
            ErrorCheck();
            Mouse.PointAndClick(Right, UiElements.LoginUserNameField, 0, 10, 2);
            Mouse.OffsetAndClick(Left, UiElements.ContextMenuModifyOrderOffset, 0, 10, 2);
            if (Clipboard.GetTextFromClipboard().CompareTo("0") != 0)
                return true;
            return false;
        }
    }
}