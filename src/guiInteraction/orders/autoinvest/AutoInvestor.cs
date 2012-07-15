using System;
using System.Diagnostics;
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

        public void execute(Character character)
        {
            this.character = character;
            itemsScanned = 0;
            ordersCreated = 0;
            if (character.tradeQueue.Count < 1)
            {
                foreach (int item in character.tradeHistory.Values)
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
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    createInvestments();
                    stopwatch.Stop();
                    logger.log(character.name + ": AI scanned " + itemsScanned + " and made " + ordersCreated + " buys " + stopwatch.Elapsed.ToString());

                    //If we failed to create any orders, the queue is full of items that aren't worth trading. Flush it and enqueue all known items.
                    if (ordersCreated == 0)
                    {
                        character.tradeQueue = new System.Collections.Generic.Queue<int>();
                        foreach (int item in character.tradeHistory.Values)
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
                    mouse.pointAndClick(LEFT, uiElements.marketSearchTab, 1, 1, 1);
                    inputValue(5, 2, uiElements.marketSearchInputBox, "test");
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

        private void openAndIdentifyBuyWindow(int currentItem, double sellPrice)
        {
            double result;
            for (int i = 0; i < 5; i++)
            {
                mouse.pointAndClick(LEFT, uiElements.placeBuyOrder, 1, 1, 6);
                mouse.pointAndClick(RIGHT, fixCoordsForLongTypeName(currentItem, uiElements.buyOrderBox), 0, 4, 2);
                mouse.offsetAndClick(LEFT, uiElements.copyOffset, 0, 2, 2);

                try
                {
                    result = Convert.ToDouble(Clipboard.getTextFromClipboard());
                    if (result < sellPrice + 1000 && result > sellPrice - 1000)
                    {
                        mouse.waitDuration = timing;
                        return;
                    }
                }
                catch
                {
                    mouse.waitDuration *= 2;
                    result = 0;
                }
            }
            mouse.waitDuration = timing;
            throw new Exception("Could not open buy window");

        }


        private void createInvestments()
        {
            int queueSize = character.tradeQueue.Count;
            int freeOrders = character.maximumOrders - orderAnalyzer.orderSet.getNumberOfActiveOrders();
            int currentItem;
            int quantity = 0;
            bool shouldIterateThroughExistingQueryResult = false;
            int existingQueryResultIteration = 0;

            for (int i = 0; i < queueSize; i++)
            {
                currentItem = character.tradeQueue.Peek();
                if (orderAnalyzer.orderSet.existsOrderOfType(currentItem, 1))
                    //If there's already an active buy order this typeid doesn't belong in the queue.
                    character.tradeQueue.Dequeue();
                else if (orderAnalyzer.orderSet.existsOrderOfType(currentItem, 0))
                    //If there is no active buy order, but there is an active sell order, move this item to
                    //end of queue (this is a possible error state [if the autolister failed to make a buy
                    //order, or if the autolister failed to enqueue an item that wasn't profitable anymore]
                    //but it could just be that the autolister still needs to make a new buy order for an
                    //item in the character's inventory)
                    character.tradeQueue.Enqueue(character.tradeQueue.Dequeue());
                else
                {
                    try
                    {
                        if (!shouldIterateThroughExistingQueryResult)
                            inputValue(5, 2, uiElements.marketSearchInputBox, modules.typeNames[currentItem]);
                        marketOrderio.fileName = executeQueryAndExportResult(5, 2, modules.typeNames[currentItem], existingQueryResultIteration);
                        orderAnalyzer.analyzeInvestment(marketOrderio.read(), Convert.ToString(currentItem), Convert.ToString(character.stationid));
                        //Uses data from orderAnalyzer.analyzeInvestment to decide if a buy order should be made
                        quantity = getBuyOrderQty(orderAnalyzer.getBuyPrice(), orderAnalyzer.getSellPrice());
                        
                        if (!orderAnalyzer.isSomeBuyOwned() && quantity > 0)
                        {
                            openAndIdentifyBuyWindow(currentItem, orderAnalyzer.getSellPrice());
                            //Input price
                            inputValue(5, 2, fixCoordsForLongTypeName(currentItem, uiElements.buyOrderBox), Convert.ToString(orderAnalyzer.getBuyPrice() + .01));
                            //Input quantity
                            inputValue(5, 2, fixCoordsForLongTypeName(currentItem, uiElements.buyOrderQtyBox), Convert.ToString(quantity));
                            confirmOrder(fixCoordsForLongTypeName(currentItem, uiElements.OrderBoxOK), 1, 1);
                            character.tradeQueue.Dequeue();
                            freeOrders--;
                            ordersCreated++;
                            shouldIterateThroughExistingQueryResult = false;
                        }
                        else
                        {
                            //Move this item to the end of the line. Maybe it will become a
                            //profitable investment again later
                            character.tradeQueue.Enqueue(character.tradeQueue.Dequeue());
                        }
                        existingQueryResultIteration = 0;

                    }
                    catch (Exception e)
                    {
                        //If an error occured at any stage, just move item to end of the line and move on.
                        if (modules.typeNames[orderAnalyzer.getTypeId()].Contains(modules.typeNames[currentItem]) && existingQueryResultIteration < 10)
                        {
                            shouldIterateThroughExistingQueryResult = true;
                            ++existingQueryResultIteration;
                        }
                        else
                        {
                            shouldIterateThroughExistingQueryResult = false;
                            character.tradeQueue.Enqueue(character.tradeQueue.Dequeue());
                            existingQueryResultIteration = 0;
                        }
                        logger.log(e.Message);
                    }
                    itemsScanned++;
                    if (freeOrders == 0)
                        return;
                }
            }
            return;
        }

        private String executeQueryAndExportResult(int tries, double timingScaleFactor, String targetTypeName, int existingQueryResultIteration)
        {
            String fileName;
            DirectoryEraser.nuke(paths.logPath);
            if (marketOrderio.getNumberOfFilesInDirectory(paths.logPath) != 0)
                throw new Exception("Could not clean log path directory");

            for (int i = 0; i < tries; i++)
            {
                if (existingQueryResultIteration == 0)
                    mouse.pointAndClick(LEFT, uiElements.marketSearchExecute, 1, 1, 1);
                mouse.pointAndClick(LEFT, uiElements.marketSearchResult[0], uiElements.marketSearchResult[1] + existingQueryResultIteration*uiElements.itemsLineHeight, 1, 1, 1);
                mouse.pointAndClick(LEFT, uiElements.exportItem, 10, 1, 10);
                fileName = marketOrderio.getNewestFileNameInDirectory(paths.logPath);
                if (fileName != null && fileName.Contains(targetTypeName))
                    return fileName;
                mouse.waitDuration = Convert.ToInt32(mouse.waitDuration * timingScaleFactor);
            }
            mouse.waitDuration = timing;
            throw new Exception("Could not export query result");
        }
    }
}
