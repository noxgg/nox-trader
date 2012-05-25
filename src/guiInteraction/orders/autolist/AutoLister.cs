using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using noxiousET.src.data.characters;
using noxiousET.src.data.client;
using noxiousET.src.data.paths;
using noxiousET.src.data.uielements;
using noxiousET.src.data.modules;
using noxiousET.src.data.io;

namespace noxiousET.src.guiInteraction.orders.autolister
{
    class AutoLister : OrderBot
    {
        //excepListBox.Items.Add("Automatically placing orders...");
        private int terminalItemID = 0;
        private int[] activeOrders = { 0, 0 };
        private int openOrders = 0;
        private int buyOrdersCreated = 0;
        private int sellOrdersCreated = 0;
        private int result = 0;
        private int autoListerTiming;
        
        public AutoLister(ClientConfig clientConfig, UiElements uiElements, Paths paths, Character character, Modules modules): base(clientConfig, uiElements, paths, character, modules)
        {
        }

        public int execute()
        {
            autoListerTiming = timingBackup * 3;
            timingMultiplier = timingBackup; //TODO Sync with client setting
            if (!isEVERunning())
            {
                return 1;
            }
            else
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                SetForegroundWindow(eveHandle);
                int failCount = 0;
                DirectoryEraser.nuke(paths.logPath);
                do
                {
                    if (stopCheck() == stopAllActivity)
                        return stopAllActivity;
                    Thread.Sleep(timingMultiplier * 20);
                    try
                    {
                        result = exportOrders();//Clicks on export orders.
                    }
                    catch
                    {
                        result = 1;
                    }
                    if (result == 1)//Clicks on export orders.
                    {
                        ++failCount;
                        errorCheck();
                        Thread.Sleep(timingMultiplier * 20);
                    }
                    else
                    {
                        activeOrders = orderSet.getNumOfActiveOrders();
                        openOrders = character.maximumOrders - (activeOrders[0] + activeOrders[1]);
                        failCount = 4;
                    }
                } while (failCount < 3);

                new SetClipboardHelper(DataFormats.Text, "0").Go();
                if (failCount == 4)
                {
                    if (character.tradeItems)
                    {
                        terminalItemID = 5321;
                        Keyboard.send("{PGUP}");
                        timingMultiplier = timingMultiplier * 3;
                        result = autoSeller(0);
                        if (result == stopAllActivity)
                        {
                            timingMultiplier = autoListerTiming;
                            orderAnalyzer.clearLastBuyOrder();
                            Keyboard.send("{PGUP}");
                            return stopAllActivity;
                        }
                        else if (result == 1)
                        {
                            timingMultiplier = autoListerTiming;
                            orderAnalyzer.clearLastBuyOrder();
                            Keyboard.send("{PGUP}");
                            return 1;
                        }
                        timingMultiplier = timingBackup;
                        orderAnalyzer.clearLastBuyOrder();
                        wait(1);
                        Keyboard.send("{PGUP}");
                        wait(1);
                        Keyboard.send("{HOME}");
                    }

                    if (character.tradeShips)
                    {
                        terminalItemID = 2078;
                        Keyboard.send("{PGDN}");
                        timingMultiplier = timingMultiplier * 3;
                        result = autoSeller(1);
                        if (result == stopAllActivity)
                        {
                            timingMultiplier = autoListerTiming;
                            orderAnalyzer.clearLastBuyOrder();
                            Keyboard.send("{PGDN}");
                            return stopAllActivity;
                        }
                        else if (result == 1)
                        {
                            timingMultiplier = autoListerTiming;
                            orderAnalyzer.clearLastBuyOrder();
                            Keyboard.send("{PGDN}");
                            return 1;
                        }
                        timingMultiplier = timingBackup;
                        orderAnalyzer.clearLastBuyOrder();
                        wait(1);
                        Keyboard.send("{PGDN}");
                        wait(1);
                        Keyboard.send("{HOME}");
                    }
                }
                stopwatch.Stop();
                //excepListBox.Items.Add("Automatic order creation completed.");
                //excepListBox.Items.Add("Created " + sellOrdersCreated + " sell orders and " + buyOrdersCreated + " buy orders in " + stopwatch.Elapsed.ToString());
            }
            //if (runMode == 2)
                //displayExceptions();
            return 0;
        }

        private int autoSeller(int itemType)
        {
            string itemName;
            int typeID = 0;
            int[] cursorPosition = { 0, 0 };
            int offsetYModifier = 0;//When an item is a ship module, it can be fit to the current ship, which causes an extra row in the context menu. This offset jumps over the extra row (offset pulled from elementsXY when needed). 
            int readFailCounter;
            int copyFailCounter;
            double activeOrderCheck;//0 if there is no active order, 1 for active sell order, 2 for active buy order, 3 for active buy and sell, 4 if no orders to compare against
            double bestSellOrderPrice;
            double bestBuyOrderPrice;
            int buyOrderQuantity;
            int longOrderNameXOffset = 0;
            int longOrderNameYOffset = 0;
            int offsetFlag = 0;


            string directory = paths.logPath;

            cursorPosition[0] = uiElements.itemsTop[0];
            cursorPosition[1] = uiElements.itemsTop[1];
            consecutiveFailures = 0;
            wait(10);
            while (openOrders > 0)
            {
                if (stopCheck() == stopAllActivity)
                    return stopAllActivity;
                //debugexcepListBox.Items.Add("Open orders = " + openOrders);
                bestSellOrderPrice = bestBuyOrderPrice = readFailCounter = copyFailCounter = buyOrderQuantity = longOrderNameXOffset = longOrderNameYOffset = offsetFlag = 0;
                activeOrderCheck = -1;

                if (itemType == 0)
                {
                    offsetYModifier = uiElements.itemsViewModuleDetailExtraOffset;
                }
                else
                {
                    offsetYModifier = 0;
                }


                if (Directory.GetFiles(directory).Length > 0)
                {
                    DirectoryEraser.nuke(directory);
                }
                do
                {
                    if (activeOrderCheck == -1 && readFailCounter > 3)
                    {
                        //debugexcepListBox.Items.Add("Error Checking");
                        if (errorCheck() == stopAllActivity)
                            return stopAllActivity;
                    }
                    if ((activeOrderCheck == -1 || activeOrderCheck == -2)) //Try view details again
                    {
                        //RClick current line
                        wait(1);
                        mouse.pointCursor(cursorPosition[0], cursorPosition[1]);
                        mouse.rightClick(1, 1);

                        //View details

                        //autoSellerListBox.Items.Add("Modifier is" + offsetYModifier);//debug
                        //autoSellerListBox.Items.Add("Y = " + (cursorPosition[1] + uiElements.itemsViewDetailsOffset[1] + offsetYModifier));//debug

                        mouse.pointCursor(cursorPosition[0] + uiElements.itemsViewDetailsOffset[0], cursorPosition[1] + uiElements.itemsViewDetailsOffset[1] + offsetYModifier);
                        mouse.leftClick(2, 1);

                        //Alternate between offset and no offset
                        if (offsetYModifier == 0 && itemType == 0)
                        {
                            //autoSellerListBox.Items.Add("Going to 21");//debug
                            offsetYModifier = uiElements.itemsViewModuleDetailExtraOffset;
                        }
                        else if (itemType == 0)
                        {
                            //autoSellerListBox.Items.Add("Going back to 0");//debug
                            offsetYModifier = 0;
                        }
                        if (readFailCounter == 8)//invert
                        {
                            if (offsetYModifier == 0 && itemType == 0)
                            {
                                //autoSellerListBox.Items.Add("Going to 21");//debug
                                offsetYModifier = uiElements.itemsViewModuleDetailExtraOffset;
                            }
                            else if (itemType == 0)
                            {
                                //autoSellerListBox.Items.Add("Going back to 0");//debug
                                offsetYModifier = 0;
                            }
                        }
                    }
                    //Click on Export Market info
                    mouse.pointCursor(uiElements.exportItem[0], uiElements.exportItem[1]);
                    mouse.leftClick(5, 3);
                    activeOrderCheck = orderAnalyzer.findBestBuyAndSell(ref orderSet, out bestSellOrderPrice, out bestBuyOrderPrice, out itemName, out typeID, paths.logPath, ref terminalItemID, Convert.ToString(character.stationid), character.fileNameTrimLength, ref offsetFlag);
                    ++readFailCounter;
                } while ((activeOrderCheck == -1 || activeOrderCheck == -2) && readFailCounter < 17);
                if (readFailCounter >= 17)
                {
                    ++consecutiveFailures;
                    //exception.Add("Failed to check an item. Retry limit exceeded.");
                }
                else
                {
                    consecutiveFailures = 0;
                }
                if ((activeOrderCheck == -1 || activeOrderCheck == -2) && consecutiveFailures == 3)
                {
                    return 1;
                }
                else if (activeOrderCheck == -4)
                    return 0;
                //debug
                //autoSellerListBox.Items.Add(itemName);
                //autoSellerListBox.Items.Add(activeOrderCheck);
                //autoSellerListBox.Items.Add(bestSellOrderPrice.ToString());
               // autoSellerListBox.Items.Add(bestBuyOrderPrice.ToString());
                //autoSellerListBox.Items.Add("");
                if (modules.longNameTypeIDs.ContainsKey(typeID))
                {
                    longOrderNameXOffset = 253;
                    longOrderNameYOffset = 22;
                }

                if (activeOrderCheck == 4)
                {
                    //autoSellerListBox.Items.Add(itemName + " not processed. No orders exist for comparision.");
                    //autoSellerListBox.Items.Add("");
                }
                offsetYModifier = 0;
                if (activeOrderCheck == 0 || activeOrderCheck == 1)//If a new buy order needs to be placed.
                {
                    double temp;
                    buyOrderQuantity = getBuyOrderQty(ref bestBuyOrderPrice, ref bestSellOrderPrice);

                    if (buyOrderQuantity > 0)
                    {
                        temp = 0;
                        if (cancelOrder(0, 0, typeID) == 0)
                        {
                            do
                            {
                                if (copyFailCounter % 4 == 0)
                                {
                                    if (errorCheck() == stopAllActivity)
                                        return stopAllActivity;
                                }

                                //Click on place buy order
                                mouse.pointCursor(uiElements.placeBuyOrder[0], uiElements.placeBuyOrder[1]);
                                mouse.leftClick(1, 6);
                                mouse.leftClick(1, 6);

                                //Right click on the field
                                mouse.pointCursor(uiElements.buyOrderBox[0], uiElements.buyOrderBox[1] + longOrderNameYOffset);
                                mouse.rightClick(4, 2);

                                //Click on copy
                                mouse.offsetCursor(uiElements.copyOffset[0], uiElements.copyOffset[1]);
                                mouse.leftClick(2, 2);
                                try
                                {
                                    temp = Convert.ToDouble(Clipboard.GetTextFromClip());
                                }
                                catch
                                {
                                    temp = 0;
                                }


                                //If this is the correct item. 
                                if ((temp - 1000) < bestSellOrderPrice && bestBuyOrderPrice < (temp + 1000))
                                {
                                    //Input buy order price.
                                    int modificationFailCount = 0;
                                    do
                                    {
                                        //Double click to highlight
                                        wait(4);
                                        mouse.pointCursor(uiElements.buyOrderBox[0], uiElements.buyOrderBox[1] + longOrderNameYOffset);
                                        mouse.doubleClick(2, 2);
                                        new SetClipboardHelper(DataFormats.Text, (bestBuyOrderPrice + .01).ToString()).Go();
                                        mouse.rightClick(2, 2);
                                        mouse.offsetCursor(uiElements.pasteOffset[0], uiElements.pasteOffset[1]);
                                        mouse.leftClick(2);
                                        new SetClipboardHelper(DataFormats.Text, "0").Go();

                                        ++modificationFailCount;
                                    } while (verifyNewBuyOrderInput(bestBuyOrderPrice, out lastOrderModified, ref longOrderNameYOffset) == false && modificationFailCount < 10);

                                    if (modificationFailCount == 10)
                                    {
                                        temp = 0;
                                    }
                                    if (offsetFlag == 1)
                                        offsetYModifier = 15;
                                    //Input buy order quantity.
                                    if (lastOrderModified)
                                    {
                                        ++buyOrdersCreated;
                                        modificationFailCount = 0;
                                        do
                                        {
                                            //Double click to highlight
                                            wait(4);
                                            mouse.pointCursor(uiElements.buyOrderQtyBox[0], uiElements.buyOrderQtyBox[1] + longOrderNameYOffset - offsetYModifier);
                                            mouse.doubleClick(6, 1);
                                            mouse.doubleClick(1, 4);
                                            Keyboard.send(buyOrderQuantity.ToString());

                                            ++modificationFailCount;
                                        } while (verifyQuantityInput(ref buyOrderQuantity, out lastOrderModified, (longOrderNameYOffset - offsetYModifier)) == false && modificationFailCount < 10);

                                        if (modificationFailCount == 10)
                                        {
                                            temp = 0;
                                        }
                                    }
                                }
                                else
                                {
                                    temp = 0;
                                }
                                ++copyFailCounter;
                            } while (string.Compare(Convert.ToString(temp), "0") == 0 && copyFailCounter < 10);
                        }
                    }
                    else
                    {
                        //autoSellerListBox.Items.Add(itemName + " not purchased. Item no longer exceeds minimum profit threshhold.");
                        //autoSellerListBox.Items.Add(bestSellOrderPrice.ToString());
                        //autoSellerListBox.Items.Add(bestBuyOrderPrice.ToString());
                        //autoSellerListBox.Items.Add("");
                    }
                    new SetClipboardHelper(DataFormats.Text, "0").Go();
                }
                if (lastOrderModified)
                {
                    if (confirmOrder(longOrderNameXOffset, longOrderNameYOffset, typeID, 0, 0) == stopAllActivity)
                        return stopAllActivity;
                    --openOrders;
                    lastOrderModified = false;
                }
                else
                {
                    //autoSellerListBox.Items.Add("Failed to buy item " + itemName);
                }
                if (activeOrderCheck == 0 || activeOrderCheck == 2)//If a new sell order needs to be placed.
                {
                    copyFailCounter = 0;
                    if (itemType == 0 && !modules.fittableModuleTypeIDs.ContainsKey(typeID))
                        offsetYModifier = uiElements.itemsViewModuleDetailExtraOffset;
                    else
                        offsetYModifier = 0;

                    double temp;
                    if (errorCheck() == stopAllActivity)
                        return stopAllActivity;
                    if (cancelOrder(0, 0, 0) == 0)
                    {
                        do
                        {
                            temp = 0;
                            //RClick on current line.
                            mouse.pointCursor(cursorPosition[0], cursorPosition[1]);
                            mouse.rightClick(1, 1);

                            //Click on Sell
                            mouse.pointCursor(cursorPosition[0] + uiElements.itemsSellItemOffset[0], cursorPosition[1] + uiElements.itemsSellItemOffset[1] + offsetYModifier);
                            mouse.leftClick(1, 1);

                            if (copyFailCounter % 3 == 2)
                            {
                                timingMultiplier += timingBackup;
                            }
                            if (copyFailCounter % 4 == 3)
                            {
                                if (offsetYModifier == 0 && itemType == 0)
                                {
                                    offsetYModifier = uiElements.itemsViewModuleDetailExtraOffset;
                                }
                                else if (itemType == 0)
                                {
                                    offsetYModifier = 0;
                                }
                            }

                            //Right click on the field
                            wait(5);
                            mouse.pointCursor(uiElements.sellOrderBox[0], uiElements.sellOrderBox[1] + longOrderNameYOffset);
                            mouse.rightClick(2, 2);

                            //Click on copy
                            mouse.offsetCursor(uiElements.copyOffset[0], uiElements.copyOffset[1]);
                            mouse.leftClick(2, 2);
                            try
                            {
                                temp = Convert.ToDouble(Clipboard.GetTextFromClip());
                            }
                            catch
                            {
                                temp = 0;
                            }

                            if (timingMultiplier != autoListerTiming)
                                timingMultiplier = autoListerTiming;

                            if ((temp - 1000) < bestBuyOrderPrice && bestBuyOrderPrice < (temp + 1000))
                            {
                                int modificationFailCount = 0;
                                do
                                {
                                    //Double click to highlight
                                    wait(4);
                                    mouse.pointCursor(uiElements.sellOrderBox[0], uiElements.sellOrderBox[1] + longOrderNameYOffset);
                                    mouse.doubleClick(2, 2);
                                    new SetClipboardHelper(DataFormats.Text, (bestSellOrderPrice - .01).ToString()).Go();
                                    mouse.rightClick(2, 2);
                                    mouse.offsetCursor(uiElements.pasteOffset[0], uiElements.pasteOffset[1]);
                                    mouse.leftClick(2);
                                    new SetClipboardHelper(DataFormats.Text, "0").Go();

                                    ++modificationFailCount;
                                } while (verifyNewSellOrderInput(bestSellOrderPrice, out lastOrderModified, longOrderNameYOffset) == false && modificationFailCount < 10);
                                if (modificationFailCount == 10)
                                {
                                    temp = 0;
                                }
                            }
                            else
                            {
                                temp = 0;
                            }
                            ++copyFailCounter;
                        } while (string.Compare(Convert.ToString(temp), "0") == 0 && copyFailCounter < 6);
                    }
                    new SetClipboardHelper(DataFormats.Text, "0").Go();
                    timingMultiplier = autoListerTiming;
                    mouse.leftClick(7);

                    if (lastOrderModified)
                    {
                        //++SellOrdersCreated;
                        cursorPosition[1] = cursorPosition[1] - uiElements.itemsLineHeight;
                        if (confirmOrder(longOrderNameXOffset, longOrderNameYOffset, typeID, 0, 0) == stopAllActivity)
                            return stopAllActivity;
                        --openOrders;
                    }
                }
                cursorPosition[1] = cursorPosition[1] + uiElements.itemsLineHeight;
            }
            return 0;
        }

        int getBuyOrderQty(ref double bestBuyOrderPrice, ref double bestSellOrderPrice)
        {
            int i;

            if (bestSellOrderPrice / bestBuyOrderPrice < 1.0736)
            {
                return -1;
            }
            else
            {
                for (i = 0; i < 4; ++i)
                {
                    if (bestBuyOrderPrice < character.quantityThreshHolds[i][0])
                    {
                        return character.quantityThreshHolds[i][1];
                    }
                }
            }
            return character.quantityThreshHolds[i][1];
        }

        private bool verifyNewBuyOrderInput(double desiredInput, out bool lastOrderModified, ref int longOrderNameYOffset)
        {
            double temp;

            //Right click on the field
            wait(1);
            mouse.pointCursor(uiElements.sellOrderBox[0], uiElements.sellOrderBox[1] + longOrderNameYOffset);
            mouse.rightClick(1, 1);

            //Click on copy
            mouse.offsetCursor(uiElements.copyOffset[0], uiElements.copyOffset[1]);
            mouse.leftClick(1, 1);

            try { temp = Convert.ToDouble(Clipboard.GetTextFromClip()); }
            catch { temp = -1; }

            if (desiredInput - 10000 < temp && temp < desiredInput + 10000 && (temp != desiredInput))
            {
                lastOrderModified = true;
                return true;
            }
            else
            {
                lastOrderModified = false;
                return false;
            }
        }

        private bool verifyNewSellOrderInput(double desiredInput, out bool lastOrderModified, int longOrderNameYOffset)
        {
            //Right click on the field
            wait(1);
            mouse.pointCursor(uiElements.sellOrderBox[0], uiElements.sellOrderBox[1] + longOrderNameYOffset);
            mouse.rightClick(1, 1);

            //Click on copy
            mouse.offsetCursor(uiElements.copyOffset[0], uiElements.copyOffset[1]);
            mouse.leftClick(1, 1);
            double temp = Convert.ToDouble(Clipboard.GetTextFromClip());
            if (desiredInput - 10000 < temp && temp < desiredInput + 10000 && (temp != desiredInput))
            {
                lastOrderModified = true;
                return true;
            }
            else
            {
                lastOrderModified = false;
                return false;
            }
        }

        private bool verifyQuantityInput(ref int quantity, out bool lastOrderModified, int longOrderNameYOffset)
        {
            //Right click on the field
            wait(1);
            mouse.pointCursor(uiElements.buyOrderQtyBox[0], uiElements.buyOrderQtyBox[1] + longOrderNameYOffset);
            mouse.rightClick(1, 1);

            //Click on copy
            mouse.offsetCursor(uiElements.copyOffset[0], uiElements.copyOffset[1]);
            mouse.leftClick(1, 2);
            int temp;
            try { temp = Convert.ToInt32(Clipboard.GetTextFromClip()); }
            catch { temp = -1; }

            if (temp == quantity)
            {
                lastOrderModified = true;
                return true;
            }
            else
            {
                lastOrderModified = false;
                return false;
            }
        }
    }
}
