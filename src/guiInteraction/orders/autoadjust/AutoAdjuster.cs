using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using noxiousET.src.etevent;
using noxiousET.src.data.characters;
using noxiousET.src.data.client;
using noxiousET.src.data.io;
using noxiousET.src.data.modules;
using noxiousET.src.data.paths;
using noxiousET.src.data.uielements;

namespace noxiousET.src.guiInteraction.orders.autoadjuster
{
    class AutoAdjuster : OrderBot
    {
        int numModified;
        int numScanned;
        int iterations = 1;//Fix these
        bool launchEVEWhenNearlyDone = false;
        private int freeOrders;

        public AutoAdjuster(ClientConfig clientConfig, UiElements uiElements, Paths paths, Character character, Modules modules, EventDispatcher eventDispatcher) 
            : base(clientConfig, uiElements, paths, character, modules, eventDispatcher)
        {
            numModified = 0;
            numScanned = 0;
            freeOrders = 0;
        }

        public int getNumberOfFreeOrders()
        {
            return freeOrders;
        }

        public int execute(Character character, bool multiUserMode)
        {
            this.character = character;
            int sellResult = 0;
            int buyResult = 0;
            numModified = 0;
            numScanned = 0;
            if (!isEVERunningForSelectedCharacter())
            {
                logger.log(character.name + ": Auto adjuster failed to find client.");
                return 1;
            }
            else
            {
                logger.autoAdjusterLog(character.name);

                mouse.waitDuration = timing;
                setEVEHandle(character.name);
                for (int i = 0; i < iterations; ++i)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    SetForegroundWindow(eveHandle);

                    DirectoryEraser.nuke(paths.logPath);

                    try { orderSet = exportOrders(10, 30); }
                    catch (Exception e) { throw e; }
                    freeOrders = orderSet.getNumberOfActiveOrders();

                    //Keyboard.send("^(V)");

                    mouse.pointAndClick(LEFT,uiElements.buySortByType, 30, 1, 30);
                    mouse.pointAndClick(LEFT, uiElements.sellSortByType, 0, 1, 0);

                    if (character.adjustBuys)
                    {
                        if (multiUserMode && i == iterations - 1 && !character.adjustSells)
                            launchEVEWhenNearlyDone = true;
                        buyResult = modOrders(uiElements.buyTop, uiElements.buySortByType, 1);
                            
                        orderAnalyzer.clearLastBuyOrder();
                        Keyboard.send("{HOME}");
                    }
                    if (character.adjustSells)
                    {
                        if (multiUserMode && i == iterations - 1)
                            launchEVEWhenNearlyDone = true;
                        sellResult = modOrders(uiElements.sellTop, uiElements.sellSortByType, 0);
                            
                        orderAnalyzer.clearLastBuyOrder();
                        Keyboard.send("{HOME}");
                    }
                    stopwatch.Stop();

                    if (sellResult != 0 && buyResult != 0)
                        logger.log(character.name + ": Auto adjuster failed to complete buy and sell runs.");
                    else if (sellResult != 0)
                        logger.log(character.name + ": Auto adjuster failed to complete sell run.");
                    else if (buyResult != 0)
                        logger.log(character.name + ": Auto adjuster failed to complete buy run.");
                    logger.log(character.name + ": AA scanned " + numScanned + ", adjusted " + numModified + " in " + stopwatch.Elapsed.ToString());
                    numScanned = numModified = 0;
                }
                return 0;

            }
        }

        //Try setting orders remotely to reduce lag
        //TODO When the GUI throws an error after editing the last item before a scrolling action, the scrolling action doesn't happen because the warning check
        //occurs after the scrolling action.
        //Returns 0 if successful.
        //Returns 1 if it can't find client.
        //returns 2 if it hangs on order export.
        private int modOrders(int[] topLine, int[] sortByType, int orderType)
        {

            int typeID = 0;
            int lastTypeID = 0;
            int[] cursorPosition = { 0, 0 };
            int[] activeOrders = orderSet.getNumberOfActiveBuysAndActiveSells();
            double modifyTo;
            double originalPrice;
            double temp;
            int offset = uiElements.visLines[orderType];
            int readFailCounter;
            int copyFailCounter;
            string directory = paths.logPath;
            int modificationFailCount = 0;

            consecutiveFailures = 0;

            int ceiling = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(activeOrders[orderType]) / Convert.ToDouble(uiElements.visLines[orderType])));
            for (int j = 0; j < ceiling; ++j)
            {
                cursorPosition[0] = topLine[0];
                cursorPosition[1] = topLine[1];
                int leftToScan = activeOrders[orderType];
                if (j == (ceiling - 1)) //This is the final iteration, so don't go all the way down the list.
                {
                    offset = (activeOrders[orderType] % uiElements.visLines[orderType]);
                }
                for (int i = 0; i < offset; ++i)
                {
                    if (launchEVEWhenNearlyDone && --leftToScan < 18)
                        launchEVEWhenNearlyDone = false;
                    if (eveHandle != GetForegroundWindow())
                        SetForegroundWindow(eveHandle); 
                    if (Directory.GetFiles(directory).Length > 0)
                        DirectoryEraser.nuke(directory);

                    lastTypeID = typeID;
                    originalPrice = copyFailCounter = readFailCounter = 0;
                    modifyTo = -1;

                    do
                    {
                        if (readFailCounter > 9 && (readFailCounter % 4) == 0)
                        {
                            if (lastOrderModified)
                            {
                                errorCheck();
                                confirmOrder(uiElements.OrderBoxOK, 1, orderType);
                            }
                            else
                            {
                                errorCheck();
                                cancelOrder(0, 0);
                            }
                        }
                        if (readFailCounter % 4 == 0 && (modifyTo < 0)) //Try view details again
                            //Double click on current entry to bring up market details
                            mouse.pointAndClick(DOUBLE, cursorPosition, 0, 1, 0);
                        //Click on Export Market info
                        mouse.pointAndClick(LEFT, uiElements.exportItem, 0, 3, 1);
                        modifyTo = orderAnalyzer.getNewPriceForOrder(ref orderSet, ref orderType, 99, ref originalPrice, paths.logPath, ref typeID, character.fileNameTrimLength);//Temp pass run as 999

                        if ((readFailCounter % 11) == 0)
                            mouse.waitDuration *= 2;
                        ++readFailCounter;
                    } while ((modifyTo == -1 || modifyTo == -2) && readFailCounter < 29);

                    mouse.waitDuration = timing;
                    if (readFailCounter == 29)
                        consecutiveFailures++;
                    else
                        consecutiveFailures = 0;

                    if (consecutiveFailures == 3)
                        return 1;
                    else if (lastOrderModified)
                        confirmOrder(uiElements.OrderBoxOK, 1, orderType);

                    if (modifyTo > 0)
                    {
                        do
                        {
                            //RClick on current line.
                            mouse.pointAndClick(RIGHT, cursorPosition, 0, 1, 1);
                            //Click on Modify
                            mouse.offsetAndClick(LEFT, uiElements.modifyOffset, 0, 1, 1);
                            //Right click order box price field
                            mouse.pointAndClick(RIGHT, uiElements.modifyOrderBox, 0, 4, 1);
                            //Click on copy
                            mouse.offsetAndClick(LEFT, uiElements.copyOffset, 0, 1, 1);
                            try
                            {
                                temp = Convert.ToDouble(Clipboard.getTextFromClipboard());
                            }
                            catch
                            {
                                temp = 0;
                            }

                            if (copyFailCounter > 2)
                                errorCheck();

                            if ((temp - 10000) < originalPrice && originalPrice < (temp + 10000))
                            {
                                mouse.waitDuration = timing;
                                modificationFailCount = 0;
                                do
                                {
                                    //Double click to highlight
                                    mouse.pointAndClick(DOUBLE, uiElements.modifyOrderBox, 0, 2, 2);
                                    Clipboard.setClip(modifyTo.ToString());
                                    mouse.click(RIGHT, 2, 2);
                                    mouse.offsetAndClick(LEFT, uiElements.pasteOffset, 0, 2, 0);
                                    Clipboard.setClip("0");
                                    lastOrderModified = true;
                                    ++modificationFailCount;
                                    copyFailCounter = 10;
                                    mouse.waitDuration *= 2;
                                } while (verifyModifyToInput(modifyTo, temp) == false && modificationFailCount < 5);
                                mouse.waitDuration = timing;
                                if (modificationFailCount == 10)
                                {
                                    cancelOrder(0, 0);
                                    temp = 0;
                                } else
                                    ++numModified;
                            } else
                                temp = 0;
                            ++copyFailCounter;

                            mouse.waitDuration *= 2;

                        } while (string.Compare(Convert.ToString(temp), "0") == 0 && copyFailCounter < 5);
                        new SetClipboardHelper(DataFormats.Text, "0").Go();
                    }
                    mouse.waitDuration = timing;
                    if (copyFailCounter == 10 && modificationFailCount == 10)
                        logger.logError("Order of price " + originalPrice + " not adjusted. Failed to make modification.");
                    //Get next line
                    ++numScanned;
                    cursorPosition[1] = cursorPosition[1] + uiElements.lineHeight;
                }
                if (j == (ceiling - 2))
                {
                    mouse.pointAndClick(LEFT, cursorPosition[0], cursorPosition[1] - uiElements.lineHeight, 0, 40, 20);
                    for (int l = 0; l < activeOrders[orderType]; ++l)
                        Keyboard.send("{UP}");
                    mouse.pointAndClick(LEFT, sortByType, 0, 20, 20);
                }
                else if (j < (ceiling - 1))
                {
                    mouse.pointAndClick(LEFT, cursorPosition[0], cursorPosition[1] - uiElements.lineHeight, 0, 40, 20);
                    for (int k = 0; k < uiElements.visLines[orderType]; ++k)
                        Keyboard.send("{DOWN}");
                }

            }
            if (lastOrderModified)
                confirmOrder(uiElements.OrderBoxOK, 1, orderType);
            return 0;
        }



        private bool verifyModifyToInput(double modifyTo, double originalPrice)
        {
            double temp;
            //Right click on the field
            mouse.pointAndClick(RIGHT, uiElements.modifyOrderBox, 1, 1, 1);
            //Click on copy
            mouse.offsetAndClick(LEFT, uiElements.copyOffset, 0, 1, 1);

            try { temp = Convert.ToDouble(Clipboard.getTextFromClipboard()); }
            catch { temp = -1; }

            if (temp - 10000 < modifyTo && modifyTo < temp + 10000 && (temp < originalPrice - .01 || temp > originalPrice + .01))
                return true;
            else
                return false;
        }
    }
}
