using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using noxiousET.src.data.characters;
using noxiousET.src.data.client;
using noxiousET.src.data.paths;
using noxiousET.src.data.uidata;
using noxiousET.src.helpers;
using noxiousET.src.orders;

namespace noxiousET.src.guiInteraction.login
{
    internal class LoginBot : GuiBot
    {
        private readonly PixelReader _pixelReader;

        public LoginBot(ClientConfig clientConfig, EveUi eveUi, Paths paths, Character character,
                        OrderAnalyzer orderAnalyzer
            ) : base(clientConfig, eveUi, paths, character, orderAnalyzer)
        {
            _pixelReader = new PixelReader(EveUi.CharacterSelectActiveSlot[0] - 5,
                                          EveUi.CharacterSelectActiveSlot[1] - 5);
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

        public void Login(Character character)
        {
            Character = character;

            if (!IsEveRunningForSelectedCharacter())
            {
                try
                {
                    LaunchClient();
                    SwapInUserSettings(character);
                    EnterCredentials();
                    SelectCharacter();
                    WaitForEnvironment();
                }
                catch (Exception e)
                {
                    Logger.Log(character.Name + e.Message);
                    throw;
                }
            }
            else
            {
                WaitForEnvironment();
            }
        }

        private void SwapInUserSettings(Character character)
        {
            const string characterFilePrefix = "core_char_";
            const string userFilePrefix = "core_user_";
            const string fileSuffix = ".dat";

            String source = Paths.ConfigPath + Paths.ClientSettingsSubDir + characterFilePrefix + character.Id +
                            fileSuffix;
            String destination = Paths.EveSettingsPath + characterFilePrefix + character.Id + fileSuffix;
            File.Copy(source, destination, true);

            source = Paths.ConfigPath + Paths.ClientSettingsSubDir + userFilePrefix + character.Account.Id +
                     character.Name + fileSuffix;
            destination = Paths.EveSettingsPath + userFilePrefix + character.Account.Id + fileSuffix;
            File.Copy(source, destination, true);
        }

        private void EnterCredentials()
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
            Clipboard.SetClip(EtConstants.ClipboardNullValue);
            ErrorCheck();
            for (int i = 0; i < 10; ++i)
            {
                try
                {
                    ErrorCheck();
                    SetForegroundWindow(EveHandle);
                    //ProcessKiller.killProcess("Chrome");
                    InputValue(5, 1, EveUi.LoginUserNameField, Character.Account.UserName);
                    InputValue(5, 1, EveUi.LoginPasswordField, Character.Account.Password);
                    Keyboard.Send("{ENTER}");
                    Thread.Sleep(5000);
                    IdentifyCharacterSelectionWindow();
                    return;
                }
                catch
                {
                }
            }
            ProcessKiller.killProcess("EXEFile");
            throw new Exception("Error logging in. Failed to enter credentials.");
        }

        private void IdentifyCharacterSelectionWindow()
        {
            for (int i = 0; i < 20; ++i)
            {
                if (GetError() > 0)
                    throw new Exception("Found error message");
                Clipboard.SetClip("");
                Mouse.PointAndClick(Right, EveUi.CharacterSelectTip, 5, 5, 10);
                //TODO: Fix hack in y coord
                Mouse.OffsetAndClick(Left, EveUi.ContextMenuCopyOffset[0],
                                     EveUi.ContextMenuCopyOffset[1] - EveUi.StandardRowHeight, 5, 5, 5);
                if (Clipboard.GetTextFromClipboard().CompareTo("") != 0)
                    return;
                Thread.Sleep(1000);
            }
            ProcessKiller.killProcess("EXEFile");
            throw new Exception("Error logging in. Could not find character select screen.");
        }

        private void SelectCharacter()
        {
            for (int i = 0; i < 20; ++i)
            {
                SetForegroundWindow(EveHandle);
                int result = FindCharacter();
                if (result == 0)
                {
                    //ProcessKiller.killProcess("Chrome");
                    return;
                }
                int errorFlag = GetError();
                if (errorFlag != 0)
                {
                    ErrorCheck();
                    Mouse.Click(Double, 0, 0);
                    Thread.Sleep(1000);
                    return;
                }
            }
            ProcessKiller.killProcess("EXEFile");
            throw new Exception("Error logging in. Could not find character select screen.");
        }

        private int FindCharacter()
        {
            if (_pixelReader.CheckForTarget(Character.LoginColor))
            {
                //pick this character if it is the right one.
                Mouse.PointAndClick(Left, EveUi.CharacterSelectActiveSlot, 0, 10, 2);
                return 0;
            }

            //select alt2
            Mouse.PointAndClick(Left, EveUi.CharacterSelectSlot3, 0, 10, 2);
            Wait(5);

            if (_pixelReader.CheckForTarget(Character.LoginColor))
            {
                //pick this character if it is the right one.
                Mouse.PointAndClick(Left, EveUi.CharacterSelectActiveSlot, 0, 10, 2);
                return 0;
            }
            //Select alt1
            Mouse.PointAndClick(Left, EveUi.CharacterSelectSlot2, 0, 10, 2);
            Wait(5);

            //Check new character
            if (_pixelReader.CheckForTarget(Character.LoginColor))
            {
                //pick this character if it is the right one.
                Mouse.PointAndClick(Left, EveUi.CharacterSelectActiveSlot, 0, 10, 2);
                return 0;
            }
            Wait(5);
            return 1;
        }


        private void WaitForEnvironment()
        {
            SetEveHandle(Character.Name);
            SetForegroundWindow(EveHandle);
            for (int i = 0; i < 20; i++)
            {
                Mouse.PointAndClick(Left, EveUi.CharacterSelectActiveSlot, 0, 10, 2);
                Wait(2);
                Wait(2);
                try
                {
                    ConfirmOrder(EveUi.OrderBoxCancel, 0, false);
                    return;
                }
                catch (Exception)
                {
                    Clipboard.SetClip(EtConstants.OrderWindowClosedVerificationText); 
                    Keyboard.Shortcut(new[] {Keyboard.VkLcontrol}, Keyboard.VkV);
                }
                Thread.Sleep(1000);
            }
            ProcessKiller.killProcess("EXEFile");
            throw new Exception("Error Logging in. Failed to find environment.");
        }

        private string GetLoginText()
        {
            return Character.Account.UserName;
        }

        private bool AtLoginScreen()
        {
            EveHandle = FindWindow("triuiScreen", "EVE");
            SetForegroundWindow(EveHandle);
            Wait(2);
            Clipboard.SetClip(EtConstants.ClipboardNullValue);
            ErrorCheck();
            Mouse.PointAndClick(Right, EveUi.LoginUserNameField, 0, 10, 2);
            Mouse.OffsetAndClick(Left, EveUi.ContextMenuModifyOrderOffset, 0, 10, 2);
            if (Clipboard.GetTextFromClipboard().CompareTo(EtConstants.ClipboardNullValue) != 0)
                return true;
            return false;
        }
    }
}