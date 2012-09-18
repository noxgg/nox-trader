﻿using System;
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
using noxiousET.src.orders;

namespace noxiousET.src.guiInteraction.orders.autolister
{
    class AutoLister : OrderBot
    {
        private int terminalItemID = 0;
        private int openOrders = 0;
        private int buyOrdersCreated = 0;
        private int sellOrdersCreated = 0;
        private int result = 0;
        private static readonly int TRITANIUM_TYPE_ID = 34;
        public int freeOrders {set; get;}

        public AutoLister(ClientConfig clientConfig, UiElements uiElements, Paths paths, Character character, Modules modules, OrderAnalyzer orderAnalyzer)
            : base(clientConfig, uiElements, paths, character, modules, orderAnalyzer)
        {
            freeOrders = 0;
        }

        public int getNumberOfFreeOrders()
        {
            return freeOrders;
        }

        public int execute(Character character)
        {
            sellOrdersCreated = 0;
            buyOrdersCreated = 0;
            result = 0;
            this.character = character;
            mouse.waitDuration = timing; //TODO Sync with client setting
            if (!isEVERunningForSelectedCharacter())
            {
                return 1;
            }

            else
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                setEVEHandle(character.name);
                SetForegroundWindow(eveHandle);
                DirectoryEraser.nuke(paths.logPath);

                logger.autoListerLog(character.name);

                try { exportOrders(5, 30); }
                catch (Exception e) { throw e; }
                freeOrders = orderAnalyzer.orderSet.getNumberOfActiveOrders();

                openOrders = character.maximumOrders - orderAnalyzer.orderSet.getNumberOfActiveOrders();
                new SetClipboardHelper(DataFormats.Text, "0").Go();

                closeMarketAndItemsWindows();
                if (character.tradeItems)
                {
                    terminalItemID = 5321;
                    wait(5);
                    keyboard.send("{PGUP}");
                    wait(40);
                    result = autoList(0);
                    
                    if (result == 1)
                    {
                        mouse.waitDuration = timing;
                        orderAnalyzer.clearLastBuyOrder();
                        keyboard.send("{PGUP}");
                        freeOrders -= (buyOrdersCreated + sellOrdersCreated);
                        return 1;
                    }

                    mouse.waitDuration = timing;
                    orderAnalyzer.clearLastBuyOrder();
                    wait(1);
                    keyboard.send("{PGUP}");
                    wait(1);
                    keyboard.send("{HOME}");
                }

                if (character.tradeShips)
                {
                    terminalItemID = 2078;
                    wait(5);
                    keyboard.send("{PGDN}");
                    wait(40);
                    result = autoList(1);
                    if (result == 1)
                    {
                        mouse.waitDuration = timing;
                        orderAnalyzer.clearLastBuyOrder();
                        keyboard.send("{PGDN}");
                        freeOrders -= (buyOrdersCreated + sellOrdersCreated);
                        return 1;
                    }
                    mouse.waitDuration = timing;
                    orderAnalyzer.clearLastBuyOrder();
                    wait(1);
                    keyboard.send("{PGDN}");
                    wait(1);
                    keyboard.send("{HOME}");
                }
                cancelOrder(0, 0); //Clean up after self.. don't leave any windows open!
                stopwatch.Stop();
                logger.log(character.name + ": AL made " + sellOrdersCreated + " sells, " + buyOrdersCreated + " buys in " + stopwatch.Elapsed.ToString());
            }
            freeOrders -= (buyOrdersCreated + sellOrdersCreated);
            return 0;
        }

        private int autoList(int itemType)
        {
            string itemName = "null";
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
            int itemSoldOutModifier = 0;


            string directory = paths.logPath;

            cursorPosition[0] = uiElements.itemsTop[0];
            cursorPosition[1] = uiElements.itemsTop[1];
            consecutiveFailures = 0;
            wait(10);
            while (openOrders > 0 && consecutiveFailures < 5)
            {
                if (cursorPosition[1] > uiElements.itemsTop[1] + uiElements.lineHeight * 19)
                {
                    cursorPosition[1] -= uiElements.lineHeight;
                    mouse.pointAndClick(LEFT, cursorPosition, 40,1,40);
                    for (int k = 0; k < 19; ++k)
                        keyboard.send("{DOWN}");
                    wait(20);
                    cursorPosition[1] = uiElements.itemsTop[1];
                }

                bestSellOrderPrice = bestBuyOrderPrice = readFailCounter = copyFailCounter = buyOrderQuantity = longOrderNameXOffset = longOrderNameYOffset = offsetFlag = itemSoldOutModifier = 0;
                activeOrderCheck = -1;

                if (itemType == 0)
                    offsetYModifier = uiElements.itemsViewModuleDetailExtraOffset;
                else
                    offsetYModifier = 0;
                if (Directory.GetFiles(directory).Length > 0)
                    DirectoryEraser.nuke(directory);

                do
                {
                    if ((activeOrderCheck == -1 || activeOrderCheck == -2)) //Try view details again
                    {
                        if (readFailCounter > 2)
                        {
                            int errorCode = getError();
                            if (errorCode == 10 || errorCode == 12)
                            {
                                errorCheck();
                                mouse.pointAndClick(LEFT, uiElements.itemsTop, 1, 1, 1);
                            }
                            errorCheck();
                        }
                        //RClick current line
                        mouse.pointAndClick(RIGHT, cursorPosition, 1, 1, 1);
                        //View details
                        mouse.offsetAndClick(LEFT, uiElements.itemsViewDetailsOffset[0], uiElements.itemsViewDetailsOffset[1] + offsetYModifier, 0, 2, 1);
                        //TODO Make variable. Normal click route often causes inadvertant double-clicks on items, causing ships to be assembled and items
                        //to be fitted. This left-click in a deadzone prevents such double clicks from occuring. 
                        mouse.pointAndClick(LEFT, 120, 747, 1, 1, 1);
                        if (itemType == 0)
                            offsetYModifier = (offsetYModifier == 0 && readFailCounter != 8) ? uiElements.itemsViewModuleDetailExtraOffset : 0;
                        if (readFailCounter % 2 == 1)
                            mouse.waitDuration *= 2;
                    }
                    //Click on Export Market info
                    mouse.pointAndClick(LEFT, uiElements.exportItem, 0, 5, 3);

                    activeOrderCheck = orderAnalyzer.findBestBuyAndSell(out bestSellOrderPrice, out bestBuyOrderPrice, out typeID, paths.logPath, ref terminalItemID, Convert.ToString(character.stationid), ref offsetFlag);
                    if (typeID != 0)
                        itemName = modules.typeNames[typeID];
                    ++readFailCounter;
                } while ((activeOrderCheck == -1 || activeOrderCheck == -2) && readFailCounter < 8);

                if (readFailCounter >= 17)
                {
                    ++consecutiveFailures; 
                    logger.logError("Failed to check an item. Retry limit exceeded.");
                }
                else
                    consecutiveFailures = 0;
                mouse.waitDuration = timing;
                if ((activeOrderCheck == -1 || activeOrderCheck == -2) && consecutiveFailures == 3)
                {
                    return 1;
                }
                else if (activeOrderCheck == -4)
                    return 0;
                else if (consecutiveFailures > 0)
                    resetView(itemType);

                if (modules.longNameTypeIDs.ContainsKey(typeID))
                {
                    longOrderNameXOffset = 253;
                    longOrderNameYOffset = 22;
                }

                offsetYModifier = 0;
                if ((activeOrderCheck == 0 || activeOrderCheck == 1) && character.adjustBuys && character.tradeHistory.ContainsKey(typeID))//If a new buy order needs to be placed.
                {
                    double temp;
                    buyOrderQuantity = getBuyOrderQty(bestBuyOrderPrice, bestSellOrderPrice);

                    if (buyOrderQuantity > 0)
                    {
                        temp = 0;
                        if (cancelOrder(0, 0) == 0)
                        {
                            do
                            {

                                //Click on place buy order
                                mouse.pointAndClick(LEFT, uiElements.placeBuyOrder, 0, 1, 6);
                                //Right click on the field
                                mouse.pointAndClick(RIGHT, uiElements.buyOrderBox[0], uiElements.buyOrderBox[1] + longOrderNameYOffset, 0, 4, 2);
                                //Click on copy
                                mouse.offsetAndClick(LEFT, uiElements.copyOffset, 0, 2, 2);

                                try {  temp = Convert.ToDouble(Clipboard.getTextFromClipboard()); }
                                catch  {  temp = 0; }

                                //If this is the correct item. 
                                if (temp < bestSellOrderPrice + 1000 && temp > bestSellOrderPrice - 1000)
                                {
                                    //Input buy order price.
                                    int modificationFailCount = 0;
                                    do
                                    {
                                        //Double click to highlight
                                        mouse.pointAndClick(DOUBLE, uiElements.buyOrderBox[0], uiElements.buyOrderBox[1] + longOrderNameYOffset, 4, 2, 2);
                                        Clipboard.setClip((bestBuyOrderPrice + .01).ToString());
                                        mouse.click(RIGHT, 2, 2);
                                        mouse.offsetAndClick(LEFT, uiElements.pasteOffset, 0, 2, 0);
                                        Clipboard.setClip("0");

                                        ++modificationFailCount;
                                    } while (verifyNewOrderInput(uiElements.buyOrderBox, bestBuyOrderPrice, out lastOrderModified, ref longOrderNameYOffset) == false && modificationFailCount < 10);

                                    if (modificationFailCount == 10)
                                        temp = 0;
                                    if (offsetFlag == 1)
                                        itemSoldOutModifier = 15;
                                    //Input buy order quantity.
                                    if (lastOrderModified)
                                    {
                                        ++buyOrdersCreated;
                                        modificationFailCount = 0;
                                        do
                                        {
                                            //Double click to highlight
                                            mouse.pointAndClick(DOUBLE, uiElements.buyOrderQtyBox[0], uiElements.buyOrderQtyBox[1] + longOrderNameYOffset - itemSoldOutModifier, 4, 6, 1);
                                            mouse.click(DOUBLE, 1, 4);
                                            keyboard.send(buyOrderQuantity.ToString());

                                            ++modificationFailCount;
                                        } while (verifyQuantityInput(ref buyOrderQuantity, out lastOrderModified, longOrderNameYOffset - itemSoldOutModifier) == false && modificationFailCount < 10);

                                        if (modificationFailCount == 10)
                                            temp = 0;
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
                        if (!character.tradeQueue.Contains(typeID))
                            character.tradeQueue.Enqueue(typeID);
                        logger.autoAdjusterLog(itemName + " not purchased. Item no longer exceeds minimum profit threshhold.");
                        logger.autoAdjusterLog(bestSellOrderPrice.ToString());
                        logger.autoAdjusterLog(bestBuyOrderPrice.ToString());
                        logger.autoAdjusterLog("");
                    }
                    new SetClipboardHelper(DataFormats.Text, "0").Go();


                    if (lastOrderModified)
                    {
                        confirmOrder(new int[2] {uiElements.OrderBoxOK[0], uiElements.OrderBoxOK[1] + longOrderNameYOffset - itemSoldOutModifier }, 1, 1);
                        --openOrders;
                        lastOrderModified = false;
                    }
                    else
                    {
                        if (activeOrderCheck == 0 || activeOrderCheck == 2)
                        {
                            if (!character.tradeQueue.Contains(typeID))
                                character.tradeQueue.Enqueue(typeID);
                        }
                        logger.autoAdjusterLog("Failed to buy item " + itemName);
                        logger.autoAdjusterLog("");
                    }
                }
                if ((activeOrderCheck == 0 || activeOrderCheck == 2) && character.tradeHistory.ContainsKey(typeID))//If a new sell order needs to be placed.
                {
                    copyFailCounter = 0;
                    if (itemType == 0 && !modules.fittableModuleTypeIDs.ContainsKey(typeID))
                        offsetYModifier = uiElements.itemsViewModuleDetailExtraOffset;
                    else
                        offsetYModifier = 0;

                    double temp = 0;
                        do
                        {
                            if (cancelOrder(0, 0) == 0)
                            {
                                temp = 0;
                                //RClick on current line.
                                mouse.pointAndClick(RIGHT, cursorPosition, 0, 1, 1);
                                mouse.pointAndClick(LEFT, cursorPosition[0] + uiElements.itemsSellItemOffset[0], cursorPosition[1] + uiElements.itemsSellItemOffset[1] + offsetYModifier, 0, 1, 1);

                                if (copyFailCounter % 3 == 2)
                                    mouse.waitDuration *= 2;
                                if (copyFailCounter % 4 == 3)
                                {
                                    if (offsetYModifier == 0 && itemType == 0)
                                        offsetYModifier = uiElements.itemsViewModuleDetailExtraOffset;
                                    else if (itemType == 0)
                                        offsetYModifier = 0;
                                }

                                //Right click on the field
                                mouse.pointAndClick(RIGHT, uiElements.sellOrderBox[0], uiElements.sellOrderBox[1] + longOrderNameYOffset, 5, 2, 2);
                                //Click on copy
                                mouse.offsetAndClick(LEFT, uiElements.copyOffset, 0, 2, 2);
                                try{ temp = Convert.ToDouble(Clipboard.getTextFromClipboard()); }
                                catch{ temp = 0; }

                                mouse.waitDuration = timing;
                                if ((temp - 1000) < bestBuyOrderPrice && bestBuyOrderPrice < (temp + 1000))
                                {
                                    int modificationFailCount = 0;
                                    do
                                    {
                                        //Double click to highlight
                                        mouse.pointAndClick(DOUBLE, uiElements.sellOrderBox[0], uiElements.sellOrderBox[1] + longOrderNameYOffset, 4, 2, 2);
                                        Clipboard.setClip((bestSellOrderPrice - .01).ToString());
                                        mouse.click(RIGHT, 2, 2);
                                        mouse.offsetAndClick(LEFT, uiElements.pasteOffset, 0, 2, 0);
                                        Clipboard.setClip("0");

                                        ++modificationFailCount;
                                    } while (verifyNewOrderInput(bestSellOrderPrice, out lastOrderModified, ref longOrderNameYOffset) == false && modificationFailCount < 10);
                                    if (modificationFailCount == 10)
                                        temp = 0;
                                }
                                else
                                {
                                    temp = 0;
                                }
                                ++copyFailCounter;
                            }
                        } while (string.Compare(Convert.ToString(temp), "0") == 0 && copyFailCounter < 6);
                    Clipboard.setClip("0");
                    mouse.waitDuration = timing;
                    mouse.click(LEFT,7, 0);

                    if (lastOrderModified)
                    {
                        ++sellOrdersCreated;
                        cursorPosition[1] = cursorPosition[1] - uiElements.itemsLineHeight;
                        confirmOrder(fixCoordsForLongTypeName(typeID, uiElements.OrderBoxOK), 1, 0);
                        --openOrders;
                    }
                }
                cursorPosition[1] = cursorPosition[1] + uiElements.itemsLineHeight;
            }
            return 0;
        }

        private bool verifyNewOrderInput(double desiredInput, out bool lastOrderModified, ref int longOrderNameYOffset)
        {
            double temp;

            //Right click on the field
            mouse.pointAndClick(RIGHT, uiElements.sellOrderBox[0], uiElements.sellOrderBox[1] + longOrderNameYOffset, 1, 1, 1);
            //Click on copy
            mouse.offsetAndClick(LEFT, uiElements.copyOffset, 0, 1, 1);

            try { temp = Convert.ToDouble(Clipboard.getTextFromClipboard()); }
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

        private bool verifyNewOrderInput(int[] orderBoxCoords, double desiredInput, out bool lastOrderModified, ref int offset)
        {
            double temp;

            //Right click on the field
            mouse.pointAndClick(RIGHT, orderBoxCoords[0], orderBoxCoords[1] + offset, 1, 1, 1);
            //Click on copy
            mouse.offsetAndClick(LEFT, uiElements.copyOffset, 0, 1, 1);

            try { temp = Convert.ToDouble(Clipboard.getTextFromClipboard()); }
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

        private bool verifyQuantityInput(ref int quantity, out bool lastOrderModified, int longOrderNameYOffset)
        {

            //Right click on the field
            mouse.pointAndClick(RIGHT, uiElements.buyOrderQtyBox[0], uiElements.buyOrderQtyBox[1] + longOrderNameYOffset, 1, 1, 1);
            //Click on copy
            mouse.offsetAndClick(LEFT, uiElements.copyOffset, 0, 1, 2);

            int temp;
            try { temp = Convert.ToInt32(Clipboard.getTextFromClipboard()); }
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

        private void resetView(int tradeType)
        {
            errorCheck();
            closeMarketAndItemsWindows();

            if (tradeType == 0)
                keyboard.send("{PGUP}");
            else
                keyboard.send("{PGDN}");

        }
    }
}
