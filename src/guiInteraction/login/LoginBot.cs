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

namespace noxiousET.src.guiInteraction.login
{
    class LoginBot : GuiBot
    {
        private PixelReader pixelReader;

        public LoginBot(ClientConfig clientConfig, UiElements uiElements, Paths paths, Character character): base(clientConfig, uiElements, paths, character)
        {
            pixelReader = new PixelReader(uiElements.loginStage2ActiveCharacter[0] - 5, uiElements.loginStage2ActiveCharacter[1] - 5);
        }

        public int launchEVEStateUnknown()
        {
            eveHandle = FindWindow("triuiScreen", "EVE");
            Process[] proc = Process.GetProcessesByName("EXEFile");
            if (!(proc.Count() == 1 && eveHandle != IntPtr.Zero))
            {
                ProcessKiller.killProcess("EXEFile");
                launchEVE();
            }
            int result = 1;

            if (login() != 0)
                return 1;
            if (errorCheck() == stopAllActivity)
                return stopAllActivity;
            if (loginStage2() != 0)
                return 2;
            if (errorCheck() == stopAllActivity)
                return stopAllActivity;
            if (extendedTryToExportOrders() != 0)
                return 3;
            if (errorCheck() == stopAllActivity)
                return stopAllActivity;

            result = setConfirmationValue();

            if (result == stopAllActivity)
                return stopAllActivity;
            else if (result == 1)
                return 4;

            //closeItemsAndMarketWindows();

            //excepListBox.Items.Add("Success! Logged in as " + selectedCharacter);
            return 0;
        }

        public int launchEVEPrep()
        {
            if (errorCheck() == stopAllActivity)
                return stopAllActivity;
            if (isSelectedCharacterLoggedIn() && extendedTryToExportOrders() == 0 && setConfirmationValue() == 0)
            {
                return 0;
            }
            int result = launchEVEStateUnknown();

            switch (result)
            {
                case 1:
                    //excepListBox.Items.Add("Error logging in. Failed at or before login screen.");
                    break;
                case 2:
                    //excepListBox.Items.Add("Error logging in. Failed at character selection.");
                    break;
                case 3:
                    //excepListBox.Items.Add("Error logging in. Failed at character selection.");
                    break;
                case 4:
                    //excepListBox.Items.Add("Error logging in. Failed to set confirmatino value.");
                    break;
                case -69:
                    return stopAllActivity;
            }
            return 0;
        }

        public int launchEVE()
        {
            Process.Start(paths.EVEPath);
            return 0;
        }

        private int login()
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
                return 1;
            Clipboard.setClipboardText("0");
            errorCheck();
            for (int i = 0; i < 25; ++i)
            {
                if (stopCheck() == stopAllActivity)
                    return stopAllActivity;
                SetForegroundWindow(eveHandle);
                ProcessKiller.killProcess("Chrome");
                mouse.pointCursor(uiElements.loginScreenUserName[0], uiElements.loginScreenUserName[1]);
                mouse.rightClick(10, 2);
                mouse.pointCursor(Cursor.Position.X + uiElements.modifyOffset[0], Cursor.Position.Y + uiElements.modifyOffset[1]);
                mouse.leftClick(10, 2);
                if (Clipboard.GetTextFromClip().ToString().CompareTo("0") != 0)
                {
                    mouse.pointCursor(uiElements.loginScreenUserName[0], uiElements.loginScreenUserName[1]);
                    mouse.doubleClick(2, 2);
                    mouse.doubleClick(2, 2);
                    Keyboard.send(getLoginText());
                    wait(1);
                    mouse.pointCursor(uiElements.loginScreenUserName[0], uiElements.loginScreenUserName[1] + 15);
                    mouse.doubleClick(2, 2);
                    mouse.doubleClick(2, 2);
                    wait(1);
                    //debugexcepListBox.Items.Add(p);
                    Keyboard.send(character.account.p);
                    mouse.pointCursor(uiElements.loginScreenConnect[0], uiElements.loginScreenConnect[1]);
                    mouse.leftClick(1, 1);

                    return 0;
                }
            }
            return 1;
        }

        private int loginStage2()
        {
            int result = 1;
            int errorFlag = 0;
            for (int i = 0; i < 20; ++i)
            {
                if (stopCheck() == stopAllActivity)
                    return stopAllActivity;
                SetForegroundWindow(eveHandle);
                result = loginCharacterSelect();
                if (result == 0)
                {
                    ProcessKiller.killProcess("Chrome");
                    return 0;
                }
                errorFlag = getError();
                if (errorFlag != 0)
                {
                    errorCheck();
                    mouse.doubleClick();
                    Thread.Sleep(1000);
                    return 1;
                }

            }
            return 1;
        }

        private int loginCharacterSelect()
        {
            wait(200);
            if (pixelReader.checkForTarget(character.loginColors))
            {
                //pick this character if it is the right one.
                mouse.pointCursor(uiElements.loginStage2ActiveCharacter[0], uiElements.loginStage2ActiveCharacter[1]);
                mouse.leftClick(10, 2);
                return 0;
            }

            //select alt1
            mouse.pointCursor(uiElements.loginStage2Alt1[0], uiElements.loginStage2Alt1[1]);
            mouse.leftClick(10, 2);
            wait(200);

            if (pixelReader.checkForTarget(character.loginColors))
            {
                //pick this character if it is the right one.
                mouse.pointCursor(uiElements.loginStage2ActiveCharacter[0], uiElements.loginStage2ActiveCharacter[1]);
                mouse.leftClick(10, 2);
                return 0;
            }
            //Select alt2
            mouse.pointCursor(uiElements.loginStage2Alt2[0], uiElements.loginStage2Alt2[1]);
            mouse.leftClick(10, 2);
            wait(200);

            //Check new character
            if (pixelReader.checkForTarget(character.loginColors))
            {
                //pick this character if it is the right one.
                mouse.pointCursor(uiElements.loginStage2ActiveCharacter[0], uiElements.loginStage2ActiveCharacter[1]);
                mouse.leftClick(10, 2);
                return 0;
            }
            return 1;
        }



        public int extendedTryToExportOrders()
        {

            int result;
            DirectoryEraser.nuke(paths.logPath);
            for (int i = 0; i < 20; i++)
            {
                mouse.pointCursor(uiElements.loginStage2ActiveCharacter[0], uiElements.loginStage2ActiveCharacter[1]);
                mouse.leftClick(10, 2);
                try
                {
                    result = exportOrders();//Clicks on export orders.
                }
                catch
                {
                    result = 1;
                    errorCheck();
                }
                if (result == 0)
                    return 0;
                Thread.Sleep(1000);
            }
            return 1;
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
            Clipboard.setClipboardText("0");
            errorCheck();
            mouse.pointCursor(uiElements.loginScreenUserName[0], uiElements.loginScreenUserName[1]);
            mouse.rightClick(10, 2);
            mouse.pointCursor(Cursor.Position.X + uiElements.modifyOffset[0], Cursor.Position.Y + uiElements.modifyOffset[1]);
            mouse.leftClick(10, 2);
            if (Clipboard.GetTextFromClip().ToString().CompareTo("0") != 0)
            {
                return true;
            }
            return false;
        }

        private int setConfirmationValue()
        {
            wait(2);
            Clipboard.setClipboardText("0");
            wait(2);
            int temp = 1;
            for (int i = 0; i < 15; ++i)
            {
                temp = confirmOrder(0, 0, 0, 0, 0); //TODO refactor this out of here
                if (temp == 0)//Success!
                {
                    return 0;
                }
                else if (temp == stopAllActivity)
                {
                    return stopAllActivity;
                }
                else
                {
                    mouse.pointCursor(uiElements.OrderBoxOK[0], uiElements.OrderBoxOK[1]);
                    mouse.leftClick(1, 1);
                    Keyboard.send("-1");
                }
            }
            return 1;//Failure
        }

        private bool isSelectedCharacterLoggedIn()
        {
            return isEVERunning();
        }
    }



}
