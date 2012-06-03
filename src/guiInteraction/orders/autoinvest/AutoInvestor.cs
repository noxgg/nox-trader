using System;
using System.Diagnostics;
using noxiousET.src.data.characters;
using noxiousET.src.data.client;
using noxiousET.src.data.io;
using noxiousET.src.data.modules;
using noxiousET.src.data.paths;
using noxiousET.src.data.uielements;
using noxiousET.src.etevent;

namespace noxiousET.src.guiInteraction.orders.autoinvester
{
    class AutoInvestor : OrderBot
    {
        MarketOrderio marketOrderio;
        private int ordersCreated;
        private int itemsScanned;

        public AutoInvestor(ClientConfig clientConfig, UiElements uiElements, Paths paths, Character character, Modules modules, EventDispatcher eventDispatcher)
            : base(clientConfig, uiElements, paths, character, modules, eventDispatcher)
        {
            this.marketOrderio = new MarketOrderio();
            marketOrderio.path = paths.logPath;
        }

        public void execute(Character character)
        {
            this.character = character;
            itemsScanned = 0;
            ordersCreated = 0;
            if (character.tradeQueue.Count > 0)
            {
                try
                {
                    orderSet = exportOrders(3, 30);
                    if (orderSet.getNumberOfActiveOrders() >= character.maximumOrders)
                        return;
                    orderAnalyzer.orderSet = this.orderSet;
                    prepareEnvironment();
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    createInvestments();
                    stopwatch.Stop();
                    logger.log(character.name + ": AI scanned " + itemsScanned + " and made " + ordersCreated + " buys " + stopwatch.Elapsed.ToString());
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
                    closeMarketAndItemsWindows();
                    wait(20);
                    Keyboard.send("{HOME}");
                    mouse.pointAndClick(LEFT, uiElements.bringMarketWindowToFront, 20, 1, 20);
                    mouse.pointAndClick(LEFT, uiElements.marketSearchTab, 1, 1, 1);
                    inputValue(uiElements.marketSearchInputBox, "test");
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
            int freeOrders = character.maximumOrders - orderSet.getNumberOfActiveOrders();
            int currentItem;
            int quantity = 0;

            for (int i = 0; i < queueSize; i++)
            {
                currentItem = character.tradeQueue.Peek();
                if (orderSet.existsOrderOfType(currentItem, 1))
                    //If there's already an active buy order this typeid doesn't belong in the queue.
                    character.tradeQueue.Dequeue();
                else if (orderSet.existsOrderOfType(currentItem, 0))
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
                        //execute a query for the target item
                        inputValue(uiElements.marketSearchInputBox, modules.typeNames[currentItem]);
                        marketOrderio.fileName = executeQueryAndExportResult(modules.typeNames[currentItem]);
                        orderAnalyzer.analyzeInvestment(marketOrderio.read(), Convert.ToString(currentItem), Convert.ToString(character.stationid));
                        //Uses data from orderAnalyzer.analyzeInvestment to decide if a buy order should be made
                        quantity = getBuyOrderQty(orderAnalyzer.getBuyPrice(), orderAnalyzer.getSellPrice());
                        
                        if (!orderAnalyzer.isSomeBuyOwned() && quantity > 0)
                        {
                            openAndIdentifyBuyWindow(currentItem, orderAnalyzer.getSellPrice());
                            //Input price
                            inputValue(fixCoordsForLongTypeName(currentItem, uiElements.buyOrderBox), Convert.ToString(orderAnalyzer.getBuyPrice() + .01));
                            //Input quantity
                            inputValue(fixCoordsForLongTypeName(currentItem, uiElements.buyOrderQtyBox), Convert.ToString(quantity));
                            confirmOrder(fixCoordsForLongTypeName(currentItem, uiElements.OrderBoxOK), 1, 1);
                            character.tradeQueue.Dequeue();
                            freeOrders--;
                            ordersCreated++;
                        }
                        else
                        {
                            //Move this item to the end of the line. Maybe it will become a
                            //profitable investment again later
                            character.tradeQueue.Enqueue(character.tradeQueue.Dequeue());
                        }

                    }
                    catch (Exception e)
                    {
                        //If an error occured at any stage, just move item to end of the line and move on.
                        character.tradeQueue.Enqueue(character.tradeQueue.Dequeue());
                        logger.log(e.Message);
                    }
                    itemsScanned++;
                    if (freeOrders == 0)
                        return;
                }
            }
            return;
        }

        private void inputValue(int[] coords, string value)
        {
            for (int i = 0; i < 4; i++)
            {
                mouse.pointAndClick(DOUBLE, coords, 4, 2, 2);
                Clipboard.setClip(value);
                mouse.click(RIGHT, 2, 2);
                mouse.offsetAndClick(LEFT, uiElements.pasteOffset, 0, 2, 0);
                if (verifyInput(coords, value))
                    return;
                mouse.waitDuration *= 2;
            }
            mouse.waitDuration = timing;
            throw new Exception("Failed to input value " + value);
        }

        private bool verifyInput(int[] coords, string desiredValue)
        {
            mouse.pointAndClick(RIGHT, coords, 1, 1, 1);
            mouse.offsetAndClick(LEFT, uiElements.copyOffset, 1, 1, 1);

            try
            {
                if (desiredValue.Equals(Clipboard.getTextFromClipboard()))
                    return true;
            }
            catch
            {
                return false;
            }
            return false;
        }

        private String executeQueryAndExportResult(String targetTypeName)
        {
            String fileName;
            DirectoryEraser.nuke(paths.logPath);
            if (marketOrderio.getNumberOfFilesInDirectory(paths.logPath) != 0)
                throw new Exception("Could not clean log path directory");

            for (int i = 0; i < 5; i++)
            {
                mouse.pointAndClick(LEFT, uiElements.marketSearchExecute, 1, 1, 1);
                mouse.pointAndClick(LEFT, uiElements.marketSearchResult, 1, 1, 1);
                mouse.pointAndClick(LEFT, uiElements.exportItem, 10, 1, 10);
                fileName = marketOrderio.getNewestFileNameInDirectory(paths.logPath);
                if (fileName != null && fileName.Contains(targetTypeName))
                    return fileName;
                mouse.waitDuration *= 2;
            }
            mouse.waitDuration = timing;
            throw new Exception("Could not export query result");
        }
    }
}
