﻿using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using noxiousET.src.etevent;
using noxiousET.src.model.data.characters;
using noxiousET.src.model.data.client;
using noxiousET.src.model.data.paths;
using noxiousET.src.model.data.uielements;
using noxiousET.src.model.helpers;
using noxiousET.src.model.orders;


namespace noxiousET.src.model.guiInteraction
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
        protected Boolean lastOrderModified = false;
        protected OrderManager orderSet;
        protected EventDispatcher logger;
        protected static readonly int LEFT = (int)Mouse.clickTypes.LEFT;
        protected static readonly int RIGHT = (int)Mouse.clickTypes.RIGHT;
        protected static readonly int DOUBLE = (int)Mouse.clickTypes.DOUBLE;

        public GuiBot(ClientConfig clientConfig, UiElements uiElements, Paths paths, Character character, EventDispatcher eventDispatcher)
        {
            this.clientConfig = clientConfig;
            this.timingMultiplier = clientConfig.timingMultiplier;
            this.uiElements = uiElements;
            this.paths = paths;
            this.character = character;
            this.logger = eventDispatcher;
            mouse = new Mouse(timingMultiplier);
            errorParser = new ErrorParser();
        }

        protected int errorCheck()
        {
            mouse.pointAndClick(LEFT, uiElements.errorCheck, 0, 2, 0);
            return 0;
        }

        protected int confirmErrorCheck()
        {
            mouse.pointAndClick(LEFT, uiElements.confirmErrorCheck, 0, 2, 0);
            return 0;
        }

        protected void wait(int multiplier)
        {
            Thread.Sleep(timingMultiplier * multiplier);
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
            string message;
            mouse.pointAndClick(RIGHT, uiElements.parseErrorMessage, 0, 1, 1);
            mouse.offsetAndClick(LEFT, uiElements.parseErrorMessageCopyOffset, 0, 1, 1);
            message = Clipboard.getTextFromClipboard();

            if (string.Compare(message, "0") != 0)
                return errorParser.parse(message);
            return 0;
        }

        protected int exportOrders()
        {
            errorCheck();
            mouse.pointAndClick(LEFT, uiElements.exportOrderList, 0, 2, 0);

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
            orderSet = new OrderManager(directory.ToString() + fileName, ref file, character.tradeHistory);

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
                mouse.pointAndClick(LEFT, uiElements.OrderBoxOK[0], uiElements.OrderBoxOK[1] + longOrderYOffset, 1, 1, 1);
                if (confirmationType == 1 && failCount > 3)
                {
                    errorFlag = getError();
                    //If the error is 'above regional average' and this is a sell order || it is below/buy
                    if (errorFlag == 1 && buyOrSell == 0 || errorFlag == 2 && buyOrSell == 1)
                    {
                        confirmErrorCheck();
                        wait(1);
                        confirmErrorCheck();
                    } else
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

                result = Clipboard.getTextFromClipboard();
                ++failCount;
            } while (string.Compare(result, "0") == 0 && failCount < 9);

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
    }
}
