using System;
using System.Diagnostics;
using System.Collections.Generic;
using noxiousET.src.data.characters;
using noxiousET.src.data.client;
using noxiousET.src.data.io;
using noxiousET.src.data.modules;
using noxiousET.src.data.paths;
using noxiousET.src.data.uielements;
using noxiousET.src.etevent;
using noxiousET.src.orders;

namespace noxiousET.src.guiInteraction.orders.autoinvester
{
    class AutoInvestor : OrderBot
    {
        MarketOrderio marketOrderio;
        private int ordersCreated;
        private int itemsScanned;

        public AutoInvestor(ClientConfig clientConfig, UiElements uiElements, Paths paths, Character character, Modules modules, OrderAnalyzer orderAnalyzer)
            : base(clientConfig, uiElements, paths, character, modules, orderAnalyzer)
        {
            this.marketOrderio = new MarketOrderio();
            marketOrderio.path = paths.logPath;
        }

        public void getTypeForCharacterFromQuickbar(Character character, String firstItemId, String lastItemId)
        {
            this.character = character;
            int previousItemId = int.Parse(firstItemId);
            List<int> newTradeHistory = new List<int>();
            int offset = 0;
            int visibleRows = uiElements.marketWindowQuickbarVisibleRows;
            int[] lastVisibleRowCoords = new int[] { uiElements.marketWindowQuickbarFirstRow[0], uiElements.marketWindowQuickbarFirstRow[1] + ((visibleRows - 1) * uiElements.lineHeight) };
            String lastTypeName = "no last type name just yet";

            exportOrders(4, 30);

            marketOrderio.fileName = executeQueryAndExportResult(5, 1.2, lastTypeName, offset);
            orderAnalyzer.analyzeInvestment(marketOrderio.read(), Convert.ToString(character.stationid));
            if (orderAnalyzer.getTypeId() != int.Parse(firstItemId))
                throw new Exception("First type id does not match discovered type id");
            newTradeHistory.Add(orderAnalyzer.getTypeId());
            previousItemId = orderAnalyzer.getTypeId();
            offset++;

            while (previousItemId != int.Parse(lastItemId))
            {
                if (offset > (visibleRows - 1))
                {
                    mouse.pointAndClick(LEFT, lastVisibleRowCoords, 1, 1, 1);
                    keyboard.send("{DOWN}");
                    offset = visibleRows - 1;
                }
                    doExport(offset, visibleRows, lastVisibleRowCoords, lastTypeName);
                    orderAnalyzer.analyzeInvestment(marketOrderio.read(), Convert.ToString(character.stationid));
                    newTradeHistory.Add(orderAnalyzer.getTypeId());
                    previousItemId = orderAnalyzer.getTypeId();
                    lastTypeName = modules.typeNames[previousItemId];
                    offset++;
            }
            Dictionary<int, int> newTradeHistoryDictionary = new Dictionary<int, int>();
            foreach (int n in newTradeHistory)
            {
                newTradeHistoryDictionary.Add(n, n);
            }

            character.tradeHistory = newTradeHistoryDictionary;
        }

        private void doExport(int offset, int visibleRows, int[] lastVisibleRowCoords, String lastTypeName)
        {

            for (int i = 0; i < 10; i++)
            {
                if (i == 9 && offset == visibleRows - 1)
                {
                    mouse.pointAndClick(LEFT, lastVisibleRowCoords, 1, 1, 1);
                    keyboard.send("{DOWN}");
                }
                try
                {
                    marketOrderio.fileName = executeQueryAndExportResult(5, 1.2, lastTypeName, offset);
                    return;
                }
                catch (Exception)
                {
                }

            }
        }

        public void execute(Character character)
        {
            this.character = character;
            itemsScanned = 0;
            ordersCreated = 0;
            if (character.tradeQueue.Count < 1)
            {
                foreach (int item in character.tradeHistory.Keys)
                {
                    character.tradeQueue.Enqueue(item);
                }
            }
            if (character.tradeQueue.Count > 0)
            {
                try
                {
                    exportOrders(4, 30);
                    if (orderAnalyzer.orderSet.getNumberOfActiveOrders() >= character.maximumOrders)
                        return;
                    prepareEnvironment();
                }
                catch (Exception e)
                {
                    logger.log("AI failed to prepare environment!");
                    throw e;
                }


                try
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    createInvestments();
                    stopwatch.Stop();
                    logger.log(character.name + ": AI scanned " + itemsScanned + " and made " + ordersCreated + " buys " + stopwatch.Elapsed.ToString());

                    //If we failed to create any orders, the queue is full of items that aren't worth trading. Flush it and enqueue all known items.
                    if (ordersCreated == 0)
                    {
                        character.tradeQueue = new System.Collections.Generic.Queue<int>();
                        foreach (int item in character.tradeHistory.Keys)
                        {
                            character.tradeQueue.Enqueue(item);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.log("Auto investor failed to complete run.");
                    logger.logError(e.Message);
                }
            }
            else
            {
                logger.log(character.name + " has no items in the queue for Auto Investor to process. Auto Investor aborted.");
            }

        }

        private void prepareEnvironment()
        {
            try
            {
                if (isEVERunningForSelectedCharacter())
                {
                    mouse.pointAndClick(LEFT, uiElements.bringMarketWindowToFront, 50, 1, 50);
                    mouse.pointAndClick(LEFT, uiElements.marketWindowQuickbarFirstRow, 1, 1, 1);
                }
                else
                {
                    throw new Exception("Auto investor ould not find EVE Client for selected character.");
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }


        private void createInvestments()
        {
            int consecutiveFailures = 0;
            int currentPosition = 0;
            int initialPosition = currentPosition;
            int size;
            int freeOrders = character.maximumOrders - orderAnalyzer.orderSet.getNumberOfActiveOrders();
            int currentTypeId;
            int quantity = 0;
            String expectedTypeName;
            String foundTypeName = "no types found so far!";
            int currentOffset = currentPosition;
            int visibleRows = uiElements.marketWindowQuickbarVisibleRows;
            List<int> tradeQueue = modules.getTypeIdsAlphabetizedByItemName(character.tradeHistory.Keys);
            size = tradeQueue.Count;

            while (currentOffset > visibleRows)
            {
                currentOffset -= visibleRows;
                goToNextQuickbarPage();
            }
            if (size - currentPosition <= visibleRows)
            {
                goToNextQuickbarPage();
                currentOffset = visibleRows - (size - currentPosition);
            }
            do
            {
                if (currentPosition == size)
                {
                    currentPosition = 0;
                    goToFirstQuickbarPage(size);
                    currentOffset = 0;
                }
                else if (currentOffset > (visibleRows - 1))
                {
                    goToNextQuickbarPage();
                    if (size - currentPosition <= visibleRows)
                        currentOffset = visibleRows - (size - currentPosition);
                    else
                        currentOffset = 0;
                } 
                expectedTypeName = modules.typeNames[tradeQueue[currentPosition]];
                currentTypeId = tradeQueue[currentPosition];
                if (orderAnalyzer.orderSet.checkForActiveOrders(tradeQueue[currentPosition]) == 0)
                {
                    try
                    {
                        marketOrderio.fileName = executeQueryAndExportResult(5, 1.2, expectedTypeName, currentOffset);
                        orderAnalyzer.analyzeInvestment(marketOrderio.read(), Convert.ToString(character.stationid));
                        foundTypeName = modules.typeNames[orderAnalyzer.getTypeId()];
                        if (foundTypeName.Equals(expectedTypeName) && !orderAnalyzer.isSomeBuyOwned() && !orderAnalyzer.isSomeSellOwned())
                        {
                            //Uses data from orderAnalyzer.analyzeInvestment to decide if a buy order should be made
                            quantity = getBuyOrderQty(orderAnalyzer.getBuyPrice(), orderAnalyzer.getSellPrice());

                            if (quantity > 0)
                            {
                                openAndIdentifyBuyWindow(currentTypeId, orderAnalyzer.getSellPrice());
                                placeBuyOrder(currentTypeId, quantity);
                                freeOrders--;
                                ordersCreated++;
                                //logger.log(modules.typeNames[currentTypeId] + " should create buy order.");
                            }
                            else if (foundTypeName.CompareTo(expectedTypeName) > 0)
                            {
                                currentOffset -= 2;
                                currentPosition--;
                            }
                            else if (foundTypeName.CompareTo(expectedTypeName) < 0)
                            {
                                currentPosition--;
                            }
                            else
                            {
                                //logger.log(modules.typeNames[currentTypeId] + " should create buy order, but quantity was 0.");
                            }

                        }
                        consecutiveFailures = 0;
                    }
                    catch (Exception e)
                    {
                        ++consecutiveFailures;
                        if (consecutiveFailures > 4)
                            return;
                        logger.log(e.Message);
                    }
                }
                ++currentPosition;
                ++currentOffset;
                itemsScanned++;
                if (freeOrders == 0)
                    return;
            } while (currentPosition != initialPosition);
        }

        private void goToNextQuickbarPage()
        {
            int visibleRows = uiElements.marketWindowQuickbarVisibleRows;
            mouse.pointAndClick(LEFT, uiElements.marketWindowQuickbarFirstRow[0], uiElements.marketWindowQuickbarFirstRow[1] + ((visibleRows - 1) * uiElements.lineHeight), 30, 1, 30);
            for (int i = 0; i < visibleRows; i++)
            {
                keyboard.send("{DOWN}");
            }
            wait(30);
        }

        private void goToFirstQuickbarPage(int numberOfEntries)
        {
            mouse.drag(uiElements.marketWindowQuickbarScrollbarBottom, uiElements.marketWindowQuickbarScrollbarTop, 20, 20, 20);
        }

        private String executeQueryAndExportResult(int tries, double timingScaleFactor, String lastTypeName, int offSet)
        {
            String fileName;
            DirectoryEraser.nuke(paths.logPath);
            if (marketOrderio.getNumberOfFilesInDirectory(paths.logPath) != 0)
                throw new Exception("Could not clean log path directory");

            for (int i = 0; i < tries; i++)
            {
                mouse.pointAndClick(LEFT, uiElements.marketWindowQuickbarFirstRow[0], uiElements.marketWindowQuickbarFirstRow[1] + (offSet * uiElements.lineHeight), 1, 1, 1);
                mouse.pointAndClick(LEFT, uiElements.exportItem, 10, 1, 10);
                fileName = marketOrderio.getNewestFileNameInDirectory(paths.logPath);
                if (fileName != null)
                {
                    mouse.waitDuration = timing;
                    return fileName;
                }
                //logger.log("Found last in file name/null file name.");
                mouse.waitDuration = Convert.ToInt32(mouse.waitDuration * timingScaleFactor);
                if (i > 2)
                    errorCheck();
            }
            mouse.waitDuration = timing;
            throw new Exception("Could not export query result");
        }
    }
}
