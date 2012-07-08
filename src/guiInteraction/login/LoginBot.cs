using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using noxiousET.src.data.characters;
using noxiousET.src.data.client;
using noxiousET.src.data.io;
using noxiousET.src.data.paths;
using noxiousET.src.data.uielements;
using noxiousET.src.helpers;
using noxiousET.src.etevent;
using noxiousET.src.orders;

namespace noxiousET.src.guiInteraction.login
{
    class LoginBot : GuiBot
    {
        private PixelReader pixelReader;

        public LoginBot(ClientConfig clientConfig, UiElements uiElements, Paths paths, Character character, OrderAnalyzer orderAnalyzer
            )
            : base(clientConfig, uiElements, paths, character, orderAnalyzer)
        {
            pixelReader = new PixelReader(uiElements.selectCharacterActive[0] - 5, uiElements.selectCharacterActive[1] - 5);
        }

        private int launchClient()
        {
            eveHandle = FindWindow("triuiScreen", "EVE");
            Process[] proc = Process.GetProcessesByName("EXEFile");
            if (!(proc.Count() == 1 && eveHandle != IntPtr.Zero))
            {
                ProcessKiller.killProcess("EXEFile");
                Process.Start(paths.clientPath);
            }
            return 0;
        }

        public int login(Character character)
        {

            this.character = character;

            if (!isEVERunningForSelectedCharacter())
            {
                try
                {
                    launchClient();
                    enterCredentials();
                    selectCharacter();
                    waitForEnvironment();
                }
                catch (Exception e)
                {
                    logger.log(character.name + e.Message);
                    return 1;
                }
            }
            else
            {
                waitForEnvironment();
            }
            return 0;
        }

        private void enterCredentials()
        {
            int failCount = 0;
            eveHandle = IntPtr.Zero;
            while (eveHandle == IntPtr.Zero && failCount < 60)
            {
                eveHandle = FindWindow("triuiScreen", "EVE");
                Thread.Sleep(1000);
                failCount++;
            }
            if (eveHandle == IntPtr.Zero)
                throw new Exception("Error logging in. Could not find client.");
            Clipboard.setClip("0");
            errorCheck();
            for (int i = 0; i < 10; ++i)
            {
                try
                {
                    errorCheck();
                    SetForegroundWindow(eveHandle);
                    //ProcessKiller.killProcess("Chrome");
                    shortCopyPasteMenu = true;
                    inputValue(5, 1, uiElements.loginScreenUserName, character.account.l);
                    inputValue(5, 1, uiElements.loginScreenPW, character.account.p);
                    keyboard.send("{ENTER}");
                    shortCopyPasteMenu = false;
                    Thread.Sleep(5000);
                    identifyCharacterSelectionWindow();
                    return;
                }
                catch
                {

                }
            }
            shortCopyPasteMenu = false;
            throw new Exception("Error logging in. Failed to enter credentials.");
        }

        private void identifyCharacterSelectionWindow()
        {
            for (int i = 0; i < 20; ++i)
            {
                if (getError() > 0)
                    throw new Exception("Found error message");
                Clipboard.setClip("");
                mouse.pointAndClick(RIGHT, uiElements.selectCharacterScreenIdentification, 5, 5, 10);
                mouse.offsetAndClick(LEFT, uiElements.copyOffset[0], uiElements.copyOffset[1] - uiElements.lineHeight, 5, 5, 5);
                if (Clipboard.getTextFromClipboard().CompareTo("") != 0)
                    return;
                Thread.Sleep(1000);
            }
            throw new Exception("Error logging in. Could not find character select screen.");
        }

        private int selectCharacter()
        {
            int result = 1;
            int errorFlag = 0;
            for (int i = 0; i < 20; ++i)
            {
                SetForegroundWindow(eveHandle);
                result = findCharacter();
                if (result == 0)
                {
                    //ProcessKiller.killProcess("Chrome");
                    return 0;
                }
                errorFlag = getError();
                if (errorFlag != 0)
                {
                    errorCheck();
                    mouse.click(DOUBLE,0,0);
                    Thread.Sleep(1000);
                    return 1;
                }
            }
            throw new Exception("Error logging in. Could not find character select screen.");
        }

        private int findCharacter()
        {
            if (pixelReader.checkForTarget(character.loginColor))
            {
                //pick this character if it is the right one.
                mouse.pointAndClick(LEFT, uiElements.selectCharacterActive, 0, 10, 2);
                return 0;
            }

            //select alt2
            mouse.pointAndClick(LEFT, uiElements.selectCharacterAlt2, 0, 10, 2);
            wait(5);

            if (pixelReader.checkForTarget(character.loginColor))
            {
                //pick this character if it is the right one.
                mouse.pointAndClick(LEFT, uiElements.selectCharacterActive, 0, 10, 2);
                return 0;
            }
            //Select alt1
            mouse.pointAndClick(LEFT, uiElements.selectCharacterAlt1, 0, 10, 2);
            wait(5);

            //Check new character
            if (pixelReader.checkForTarget(character.loginColor))
            {
                //pick this character if it is the right one.
                mouse.pointAndClick(LEFT, uiElements.selectCharacterActive, 0, 10, 2);
                return 0;
            }
            wait(5);
            return 1;
        }



        private int waitForEnvironment()
        {
            setEVEHandle(character.name);
            SetForegroundWindow(eveHandle);
            for (int i = 0; i < 20; i++)
            {
                mouse.pointAndClick(LEFT, uiElements.selectCharacterActive, 0, 10, 2);
                wait(2);
                Clipboard.setClip("0");
                wait(2);
                try
                {
                    if (confirmOrder(uiElements.OrderBoxOK, 0, 0) == 0) //TODO refactor this out of here
                        return 0;
                    keyboard.send("-1");
                }
                catch (Exception e)
                {
                    keyboard.send("-1");
                } 
                Thread.Sleep(1000);
            }
            throw new Exception("Error Logging in. Failed to find environment.");
        }

        private string getLoginText()
        {
            return character.account.l;
        }

        private bool atLoginScreen()
        {
            eveHandle = FindWindow("triuiScreen", "EVE");
            SetForegroundWindow(eveHandle);
            wait(2);
            Clipboard.setClip("0");
            errorCheck();
            mouse.pointAndClick(RIGHT, uiElements.loginScreenUserName, 0, 10, 2);
            mouse.offsetAndClick(LEFT, uiElements.modifyOffset, 0, 10, 2);
            if (Clipboard.getTextFromClipboard().CompareTo("0") != 0)
                return true;
            return false;
        }
    }
}
