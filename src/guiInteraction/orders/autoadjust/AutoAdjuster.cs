using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using noxiousET.src.data.characters;
using noxiousET.src.data.client;
using noxiousET.src.data.paths;
using noxiousET.src.data.uielements;
using noxiousET.src.data.modules;
using noxiousET.src.data.io;

namespace noxiousET.src.guiInteraction.orders.autoadjuster
{
    class AutoAdjuster : OrderBot
    {
        int numModified;
        int numScanned;
        int iterations = 1;//Fix these
        bool launchEVEWhenNearlyDone = false;

        public AutoAdjuster(ClientConfig clientConfig, UiElements uiElements, Paths paths, Character character, Modules modules): base(clientConfig, uiElements, paths, character, modules)
        {
            numModified = 0;
            numScanned = 0;
        }

        public int execute(bool multiUserMode)
        {
            int sellResult = 0;
            int buyResult = 0;
            numModified = 0;
            numScanned = 0;
            //excepListBox.Items.Add("Modifying orders...");
            if (!isEVERunning())
            {
                //excepListBox.Items.Add("Failed to modify orders. Could not find EVE.");
                return 1;
            }
            else
            {
                timingMultiplier = timingBackup;
                for (int i = 0; i < iterations; ++i)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    SetForegroundWindow(eveHandle);

                    int failCount = 0;
                    DirectoryEraser.nuke(paths.logPath);
                    do
                    {
                        wait(20);
                        int result;
                        try
                        {
                            result = exportOrders();//Clicks on export orders.
                        }
                        catch
                        {
                            result = 1;
                        }
                        if (result == 1)
                        {
                            ++failCount;
                            errorCheck();
                            wait(20);
                        }
                        else
                        {
                            failCount = 4;
                        }
                    } while (failCount < 3);

                    //Keyboard.send("^(V)");

                    if (failCount == 4)
                    {
                        wait(30);
                        mouse.pointCursor(uiElements.buySortByType[0], uiElements.buySortByType[1]);
                        mouse.leftClick(1, 30);
                        mouse.pointCursor(uiElements.sellSortByType[0], uiElements.sellSortByType[1]);
                        mouse.leftClick(1);
                        if (character.adjustBuys)
                        {
                            if (multiUserMode && i == iterations - 1 && !character.adjustSells)
                                launchEVEWhenNearlyDone = true;
                            if ((buyResult = modOrders(uiElements.buyTop, uiElements.buySortByType, 1)) == stopAllActivity)
                            {
                                orderAnalyzer.clearLastBuyOrder();
                                Keyboard.send("{HOME}");
                                return stopAllActivity;
                            }
                            else if (buyResult != 0)
                            {
                                orderAnalyzer.clearLastBuyOrder();
                                Keyboard.send("{HOME}");
                                return buyResult;
                            }
                            orderAnalyzer.clearLastBuyOrder();
                            Keyboard.send("{HOME}");
                        }
                        if (character.adjustSells)
                        {
                            if (multiUserMode && i == iterations - 1)
                                launchEVEWhenNearlyDone = true;
                            if ((sellResult = modOrders(uiElements.sellTop, uiElements.sellSortByType, 0)) == stopAllActivity)
                            {
                                orderAnalyzer.clearLastBuyOrder();
                                Keyboard.send("{HOME}");
                                return stopAllActivity;
                            }
                            else if (sellResult != 0)
                            {
                                orderAnalyzer.clearLastBuyOrder();
                                Keyboard.send("{HOME}");
                                return sellResult;
                            }
                            orderAnalyzer.clearLastBuyOrder();
                            Keyboard.send("{HOME}");
                        }
                    }
                    stopwatch.Stop();

                    //ListViewItem item = runsStatsListView.Items.Add(run.ToString());
                    //item.SubItems.Add(numScanned.ToString());
                    //item.SubItems.Add(numModified.ToString());
                    //item.SubItems.Add(stopwatch.Elapsed.ToString());



                    /*if (sellResult != 0 && buyResult != 0)
                        excepListBox.Items.Add("Failed to complete buy run. Failed to complete sell run.");
                    else if (sellResult != 0)
                        excepListBox.Items.Add("Failed to complete sell run.");
                    else if (buyResult != 0)
                        excepListBox.Items.Add("Failed to complete buy run.");
                    else
                    {
                        excepListBox.Items.Add("Modifications completed successfully.");
                        if (runMode == 1)
                            displayExceptions();
                    }*/
                    //excepListBox.Items.Add("Scanned " + numScanned + " items and made " + numModified + " modifications in " + stopwatch.Elapsed.ToString());
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
            int[] activeOrders = orderSet.getNumOfActiveOrders();
            double modifyTo;
            double originalPrice;
            double temp;
            int offset = uiElements.visLines[orderType];
            int readFailCounter;
            int copyFailCounter;
            string directory = paths.configPath;
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
                    if (stopCheck() == stopAllActivity)
                        return stopAllActivity;
                    if (launchEVEWhenNearlyDone && --leftToScan < 18)
                    {
                        launchEVEWhenNearlyDone = false;
                        //loginBot.launchEVE();
                    }
                    lastTypeID = typeID;
                    if (eveHandle != GetForegroundWindow())
                    {
                        SetForegroundWindow(eveHandle);
                    }
                    originalPrice = copyFailCounter = readFailCounter = 0;
                    modifyTo = -1;

                    if (Directory.GetFiles(directory).Length > 0)
                    {
                        DirectoryEraser.nuke(directory);
                    }
                    do
                    {
                        if (modifyTo == -1 && readFailCounter > 9 && (readFailCounter % 4) == 0)
                        {
                            if (lastOrderModified)
                            {
                                //debugexcepListBox.Items.Add("AConfirming " + typeID);
                                if (confirmOrder(0, 0, typeID, 1, orderType) == stopAllActivity)
                                    return stopAllActivity;
                            }
                            if (errorCheck() == stopAllActivity)
                                return stopAllActivity;
                        }
                        if ((modifyTo == -1 || modifyTo == -2) && readFailCounter % 4 == 0) //Try view details again
                        {
                            //RClick current line
                            mouse.pointCursor(cursorPosition[0], cursorPosition[1]);
                            mouse.doubleClick(1);//View market details
                        }
                        //Click on Export Market info
                        mouse.pointCursor(uiElements.exportItem[0], uiElements.exportItem[1]);
                        mouse.leftClick(3, 1);
                        modifyTo = orderAnalyzer.getNewPriceForOrder(ref orderSet, ref orderType, 99, ref originalPrice, paths.logPath, ref typeID, character.fileNameTrimLength);//Temp pass run as 999

                        if ((readFailCounter % 11) == 0)
                        {
                            timingMultiplier += timingBackup;
                        }
                        ++readFailCounter;
                    } while ((modifyTo == -1 || modifyTo == -2) && readFailCounter < 29);
                    if (timingMultiplier != timingBackup)
                    {
                        timingMultiplier = timingBackup;
                    }
                    if (readFailCounter == 29)
                    {
                        consecutiveFailures++;
                        //exception.Add("Failed to modify an item. Exceeded retry limit");
                    }
                    else
                        consecutiveFailures = 0;
                    if (consecutiveFailures == 3)
                    {
                        return 1;
                    }
                    if (modifyTo == -1)
                    {
                        return 2;
                    }
                    if (lastOrderModified)
                    {
                        //debugexcepListBox.Items.Add("BConfirming " + lastTypeID);
                        if (confirmOrder(0, 0, lastTypeID, 1, orderType) == stopAllActivity)
                            return stopAllActivity;
                    }
                    /*if (modifyTo > 0)//If we're going to make a modification, close the window to prevent refresh lag.
                    {
                        //Close the market window
                        pointCursor(element.closeMarketWindow[0], element.closeMarketWindow[1]);
                        leftClick(5);
                    }*/
                    if (modifyTo > 0)
                    {
                        do
                        {
                            //RClick on current line.
                            mouse.pointCursor(cursorPosition[0], cursorPosition[1]);
                            mouse.rightClick(1, 1);

                            //Click on Modify
                            mouse.pointCursor(cursorPosition[0] + uiElements.modifyOffset[0], cursorPosition[1] + uiElements.modifyOffset[1]);
                            mouse.leftClick(1, 1);

                            //Right click on the field
                            mouse.pointCursor(uiElements.modifyOrderBox[0], uiElements.modifyOrderBox[1]);
                            mouse.rightClick(4, 1);

                            //Click on copy
                            mouse.offsetCursor(uiElements.copyOffset[0], uiElements.copyOffset[1]);
                            mouse.leftClick(1, 1);
                            try
                            {
                                temp = Convert.ToDouble(Clipboard.GetTextFromClip());
                            }
                            catch
                            {
                                temp = 0;
                            }

                            if ((temp - 10000) < originalPrice && originalPrice < (temp + 10000))
                            {
                                modificationFailCount = 0;
                                do
                                {
                                    if (modificationFailCount % 2 == 1)
                                    {
                                        timingMultiplier += timingBackup;
                                    }
                                    //Double click to highlight
                                    mouse.pointCursor(uiElements.modifyOrderBox[0], uiElements.modifyOrderBox[1]);
                                    mouse.doubleClick(2, 2);
                                    Clipboard.setClipboardText(modifyTo.ToString());
                                    mouse.rightClick(2, 2);
                                    mouse.offsetCursor(uiElements.pasteOffset[0], uiElements.pasteOffset[1]);
                                    mouse.leftClick(2);
                                    Clipboard.setClipboardText("0");
                                    lastOrderModified = true;
                                    ++modificationFailCount;
                                    copyFailCounter = 10;
                                } while (verifyModifyToInput(modifyTo, temp) == false && modificationFailCount < 10);
                                timingMultiplier = timingBackup;
                                if (modificationFailCount == 10)
                                {
                                    cancelOrder(0, 0, typeID);
                                    temp = 0;
                                }
                                else
                                {
                                    ++numModified;
                                }
                            }
                            else
                            {
                                temp = 0;
                            }
                            ++copyFailCounter;
                            if (copyFailCounter % 5 == 4)
                            {
                                timingMultiplier += timingBackup;
                            }
                        } while (string.Compare(Convert.ToString(temp), "0") == 0 && copyFailCounter < 10);
                        new SetClipboardHelper(DataFormats.Text, "0").Go();
                    }
                    if (timingMultiplier != timingBackup)
                    {
                        timingMultiplier = timingBackup;
                    }
                    if (copyFailCounter == 10 && modificationFailCount == 10)
                    {
                        //exception.Add("Order of price " + originalPrice + " not adjusted. Failed to make modification.");

                    }
                    //Get next line
                    ++numScanned;
                    cursorPosition[1] = cursorPosition[1] + uiElements.lineHeight;
                }
                if (j == (ceiling - 2))
                {
                    mouse.pointCursor(cursorPosition[0], cursorPosition[1] - uiElements.lineHeight);
                    mouse.leftClick(40, 20);
                    for (int l = 0; l < activeOrders[orderType]; ++l)
                    {
                        Keyboard.send("{UP}");
                    }
                    mouse.pointCursor(sortByType[0], sortByType[1]);
                    mouse.leftClick(20, 20);
                }
                else if (j < (ceiling - 1))
                {
                    mouse.pointCursor(cursorPosition[0], cursorPosition[1] - uiElements.lineHeight);
                    mouse.leftClick(40, 20);
                    for (int k = 0; k < uiElements.visLines[orderType]; ++k)
                    {
                        Keyboard.send("{DOWN}");

                    }
                }

            }
            if (lastOrderModified)
            {
                if (confirmOrder(0, 0, typeID, 1, orderType) == stopAllActivity)
                    return stopAllActivity;
            }
            return 0;
        }



        private bool verifyModifyToInput(double modifyTo, double originalPrice)
        {
            double temp;
            //Right click on the field
            wait(1);
            mouse.pointCursor(uiElements.modifyOrderBox[0], uiElements.modifyOrderBox[1]);
            mouse.rightClick(1, 1);

            //Click on copy
            mouse.pointCursor(Cursor.Position.X + uiElements.copyOffset[0], Cursor.Position.Y + uiElements.copyOffset[1]);
            mouse.leftClick(1, 1);


            try { temp = Convert.ToDouble(Clipboard.GetTextFromClip()); }
            catch { temp = -1; }

            if (temp - 10000 < modifyTo && modifyTo < temp + 10000 && (temp < originalPrice - .01 || temp > originalPrice + .01))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
