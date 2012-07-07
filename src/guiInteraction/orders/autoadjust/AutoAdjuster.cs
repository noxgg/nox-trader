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

namespace noxiousET.src.guiInteraction.orders.autoadjuster
{
    class AutoAdjuster : OrderBot
    {
        int numModified;
        int numScanned;
        private int freeOrders;
        private MarketOrderio marketOrderio;
        private String fileName;

        public AutoAdjuster(ClientConfig clientConfig, UiElements uiElements, Paths paths, Character character, Modules modules, OrderAnalyzer orderAnalyzer)
            : base(clientConfig, uiElements, paths, character, modules, orderAnalyzer)
        {
            this.marketOrderio = new MarketOrderio();
            marketOrderio.path = paths.logPath;
        }

        public int getNumberOfFreeOrders()
        {
            return freeOrders;
        }

        public void execute(Character character)
        {
            if (!character.adjustBuys && !character.adjustSells)
                return;
            this.character = character;

            prepare();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (character.adjustBuys)
                adjust(EtConstants.BUY, uiElements.buyTop, uiElements.buySortByType);
            if (character.adjustSells)
                adjust(EtConstants.SELL, uiElements.sellTop, uiElements.sellSortByType);
            stopwatch.Stop();
            logger.log(character.name + ": AA scanned " + numScanned + ", adjusted " + numModified + " in " + stopwatch.Elapsed.ToString());

        }

        private void adjust(int typeToAdjust, int[] topLineCoords, int[] sortByTypeCoords)
        {
            int[] cursorPosition = new int[2];
            int visibleLines = uiElements.visLines[typeToAdjust];
            int currentTypeId = 5321;
            bool modifiedOnLastIteration = false;

            int ceiling = Convert.ToInt32(Math.Ceiling(orderAnalyzer.orderSet.getNumberOfBuysAndSells()[typeToAdjust] / Convert.ToDouble(uiElements.visLines[typeToAdjust])));
            for (int i = 0; i < ceiling; ++i)
            {
                cursorPosition[0] = topLineCoords[0];
                cursorPosition[1] = topLineCoords[1];

                for (int j = 0; j < visibleLines; j++)
                {
                    try
                    {
                        if (eveHandle != GetForegroundWindow())
                            SetForegroundWindow(eveHandle);

                        marketOrderio.fileName = executeQueryAndExportResult(3, 2, cursorPosition, ref currentTypeId);
                        orderAnalyzer.analyzeInvestment(marketOrderio.read(), Convert.ToString(character.stationid));
                        currentTypeId = orderAnalyzer.getTypeId();
                        if (modifiedOnLastIteration)
                        {
                            confirmOrder(uiElements.OrderBoxOK, 1, typeToAdjust);
                            modifiedOnLastIteration = false;
                            ++numModified;
                        }
                        if ((!orderAnalyzer.isBestOrderOwned(typeToAdjust) && shouldAdjustOrder(ref typeToAdjust)) || 
                            (orderAnalyzer.isBestOrderOwned(typeToAdjust) && isAnOverbid(typeToAdjust)))
                        {
                            openAndIdentifyModifyWindow(8, 1.1, cursorPosition, orderAnalyzer.getOwnedPrice(typeToAdjust));
                            inputValue(5, 2, uiElements.modifyOrderBox, Convert.ToString(orderAnalyzer.getPrice(typeToAdjust) + outbid(typeToAdjust)));
                            modifiedOnLastIteration = true;
                        }
                        ++numScanned;
                        cursorPosition[1] += uiElements.lineHeight;
                    }
                    catch (Exception e)
                    {
                        cursorPosition[1] += uiElements.lineHeight;
                        logger.log(e.Message);
                        errorCheck();
                    }
                }
                if (i == (ceiling - 2))
                {
                    mouse.pointAndClick(LEFT, cursorPosition[0], cursorPosition[1] - uiElements.lineHeight, 0, 40, 20);
                    for (int l = 0; l < orderAnalyzer.orderSet.getNumberOfBuysAndSells()[typeToAdjust]; ++l)
                        keyboard.send("{UP}");
                    mouse.pointAndClick(LEFT, sortByTypeCoords, 0, 20, 20);
                }
                else if (i < (ceiling - 1))
                {
                    mouse.pointAndClick(LEFT, cursorPosition[0], cursorPosition[1] - uiElements.lineHeight, 0, 40, 20);
                    for (int k = 0; k < uiElements.visLines[typeToAdjust]; ++k)
                        keyboard.send("{DOWN}");
                }

            }
            if (lastOrderModified)
                confirmOrder(uiElements.OrderBoxOK, 1, typeToAdjust);
        }

        private Boolean isAnOverbid(int typeToAdjust)
        {
            if (typeToAdjust == EtConstants.BUY)
            {
                if (orderAnalyzer.getOwnedBuyPrice() - orderAnalyzer.getBuyPrice() >= 1)
                {
                    return true;
                }
            }
            else
            {
                if (orderAnalyzer.getSellPrice() - orderAnalyzer.getOwnedSellPrice() >= 1)
                {
                    return true;
                }
            }
            return false;
        }

        private double outbid(int typeToAdjust)
        {
            if (typeToAdjust == EtConstants.BUY)
                return .01;
            else
                return -.01;
        }

        private void openAndIdentifyModifyWindow(int tries, double timingScaleFactor, int[] coords, double price)
        {
            double result;
            for (int i = 0; i < tries; i++)
            {
                mouse.pointAndClick(RIGHT, coords, 1, 1, 1);
                mouse.offsetAndClick(LEFT, uiElements.modifyOffset, 1, 1, 1);
                mouse.pointAndClick(RIGHT, uiElements.modifyOrderBox, 1, 1, 1);
                mouse.offsetAndClick(LEFT, uiElements.copyOffset, 1, 1, 1);
                try
                {
                    result = Convert.ToDouble(Clipboard.getTextFromClipboard());
                    if (result < price + 1000 && result > price - 1000)
                    {
                        mouse.waitDuration = timing;
                        return;
                    }
                }
                catch
                {
                    mouse.waitDuration = Convert.ToInt32(mouse.waitDuration * timingScaleFactor);
                    result = 0;
                }
                mouse.waitDuration = Convert.ToInt32(mouse.waitDuration * timingScaleFactor);
            }
            mouse.waitDuration = timing;
            throw new Exception("Could not find modification window.");
        }

        private bool shouldAdjustOrder(ref int typeToAdjust)
        {
            if (typeToAdjust == EtConstants.SELL)
            {
                if (orderAnalyzer.getSellPrice() > (orderAnalyzer.getBuyPrice() + (orderAnalyzer.getOwnedSellPrice() - orderAnalyzer.getBuyPrice()) / 2))
                    return true;
            }
            else
            {
                if (orderAnalyzer.getSellPrice() < 0 || orderAnalyzer.getBuyPrice() < (orderAnalyzer.getSellPrice() - (orderAnalyzer.getSellPrice() - orderAnalyzer.getOwnedBuyPrice()) / 2))
                    return true;
            }
            logger.autoListerLog("AL not adjusting " + modules.typeNames[Convert.ToInt32(orderAnalyzer.getTypeId())]);
            logger.autoListerLog("Best Sell: " + orderAnalyzer.getBuyPrice());
            logger.autoListerLog("Best Buy: " + orderAnalyzer.getSellPrice());
            if (typeToAdjust == EtConstants.SELL)
                logger.autoListerLog("Target sell order: " + orderAnalyzer.getOwnedSellPrice());
            else
                logger.autoListerLog("Target buy order: " + orderAnalyzer.getOwnedBuyPrice());
            return false;
        }

        private String executeQueryAndExportResult(int tries, double timingScaleFactor, int[] cursorPosition, ref int lastTypeId)
        {
            DirectoryEraser.nuke(paths.logPath);
            if (marketOrderio.getNumberOfFilesInDirectory(paths.logPath) != 0)
                throw new Exception("Could not clean log path directory");
            int i;
            for (i = 0; i < tries; i++)
            {
                mouse.pointAndClick(DOUBLE, cursorPosition, 0, 1, 0);
                mouse.pointAndClick(LEFT, uiElements.exportItem, 5, 1, 5);
                fileName = marketOrderio.getNewestFileNameInDirectory(paths.logPath);
                if (fileName != null && Convert.ToInt32(marketOrderio.readFirstEntryNoDelete(paths.logPath, fileName)[2]) != lastTypeId)
                {
                    logger.log("Iterations " + i);
                    return fileName;
                }
                errorCheck();
                mouse.waitDuration = Convert.ToInt32(mouse.waitDuration * timingScaleFactor);
            }
            logger.log("Iterations ran out of tries @ " + i);
            mouse.waitDuration = timing;
            throw new Exception("Could not export query result");
        }

        private void prepare()
        {
            freeOrders = 0;
            try
            {
                setEVEHandle(character.name);
                SetForegroundWindow(eveHandle);
                DirectoryEraser.nuke(paths.logPath);
                exportOrders(3, 30);
            }
            catch (Exception e)
            {
                throw e;
            }
            numModified = 0;
            numScanned = 0;
            fileName = null;
            mouse.pointAndClick(LEFT, uiElements.sellSortByType, 40, 0, 0);
            mouse.pointAndClick(LEFT, uiElements.buySortByType, 40, 0, 0);
            freeOrders = character.maximumOrders - orderAnalyzer.orderSet.getNumberOfActiveOrders();
        }
    }
}
