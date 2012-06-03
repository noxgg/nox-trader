using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using noxiousET.src.data.characters;
using noxiousET.src.data.client;
using noxiousET.src.data.io;
using noxiousET.src.data.paths;
using noxiousET.src.data.uielements;
using noxiousET.src.helpers;
using noxiousET.src.etevent;

namespace noxiousET.src.guiInteraction.login
{
    class LoginBot : GuiBot
    {
        private PixelReader pixelReader;

        public LoginBot(ClientConfig clientConfig, UiElements uiElements, Paths paths, Character character, EventDispatcher eventDispatcher)
            : base(clientConfig, uiElements, paths, character, eventDispatcher)
        {
            pixelReader = new PixelReader(uiElements.loginStage2ActiveCharacter[0] - 5, uiElements.loginStage2ActiveCharacter[1] - 5);
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

            if (!(isEVERunningForSelectedCharacter() && waitForEnvironment() == 0 && prepareEnvironment() == 0))
            {
                try
                {
                    launchClient();
                    enterCredentials();
                    selectCharacter();
                    waitForEnvironment();
                    prepareEnvironment();
                }
                catch (Exception e)
                {
                    logger.log(character.name + e.Message);
                    return 1;
                }
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
            for (int i = 0; i < 25; ++i)
            {
                errorCheck();
                SetForegroundWindow(eveHandle);
                ProcessKiller.killProcess("Chrome");
                mouse.pointAndClick(RIGHT, uiElements.loginScreenUserName, 0, 10, 2);
                mouse.offsetAndClick(LEFT, uiElements.modifyOffset, 0, 10, 2);
                if (Clipboard.getTextFromClipboard().CompareTo("0") != 0)
                {
                    mouse.pointAndClick(DOUBLE, uiElements.loginScreenUserName, 0, 2, 2);
                    mouse.click(DOUBLE, 2, 2);
                    Keyboard.send(getLoginText());
                    mouse.pointAndClick(DOUBLE, uiElements.loginScreenUserName[0], uiElements.loginScreenUserName[1] + 15, 1, 2, 2);
                    mouse.click(DOUBLE, 2, 3);
                    Keyboard.send(character.account.p);
                    mouse.pointAndClick(LEFT, uiElements.loginScreenConnect, 0, 1, 1);
                    mouse.pointAndClick(LEFT, uiElements.loginScreenConnect, 0, 1, 1);
                    return;
                }
            }
            throw new Exception("Error logging in. Failed to enter credentials.");
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
                    ProcessKiller.killProcess("Chrome");
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
            wait(200);
            if (pixelReader.checkForTarget(character.loginColor))
            {
                //pick this character if it is the right one.
                mouse.pointAndClick(LEFT, uiElements.loginStage2ActiveCharacter, 0, 10, 2);
                return 0;
            }

            //select alt2
            mouse.pointAndClick(LEFT, uiElements.loginStage2Alt2, 0, 10, 2);
            wait(200);

            if (pixelReader.checkForTarget(character.loginColor))
            {
                //pick this character if it is the right one.
                mouse.pointAndClick(LEFT, uiElements.loginStage2ActiveCharacter, 0, 10, 2);
                return 0;
            }
            //Select alt1
            mouse.pointAndClick(LEFT, uiElements.loginStage2Alt1, 0, 10, 2);
            wait(200);

            //Check new character
            if (pixelReader.checkForTarget(character.loginColor))
            {
                //pick this character if it is the right one.
                mouse.pointAndClick(LEFT, uiElements.loginStage2ActiveCharacter, 0, 10, 2);
                return 0;
            }
            return 1;
        }



        private int waitForEnvironment()
        {
            DirectoryEraser.nuke(paths.logPath);
            setEVEHandle(character.name);
            SetForegroundWindow(eveHandle);
            for (int i = 0; i < 20; i++)
            {
                mouse.pointAndClick(LEFT, uiElements.loginStage2ActiveCharacter, 0, 10, 2);

                try
                { 
                    orderSet = exportOrders(1, 1);
                    return 0;
                }
                catch (Exception e) 
                { 
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

        private int prepareEnvironment()
        {
            wait(2);
            Clipboard.setClip("0");
            wait(2);
            for (int i = 0; i < 15; ++i)
            {
                if (confirmOrder(uiElements.OrderBoxOK, 0,0) == 0) //TODO refactor this out of here
                    return 0;
                else
                {
                    mouse.pointAndClick(LEFT, uiElements.OrderBoxOK, 0, 1, 1);
                    Keyboard.send("-1");
                }
            }
            throw new Exception("Error Logging in. Could not prepare environment.");
        }
    }
}
