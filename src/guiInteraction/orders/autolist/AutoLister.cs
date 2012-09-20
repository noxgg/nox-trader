using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
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
        private MarketOrderio marketOrderio;
        private int openOrders = 0;
        private int totalBuyOrdersCreated = 0;
        private int totalSellOrdersCreated = 0;
        private int currentBuyOrdersCreated = 0;
        private int currentSellOrdersCreated = 0;
        public int freeOrders {set; get;}
        private const int SHIP_TERMINAL_ITEM_ID = 2078;
        private const int ITEM_TERMINAL_ID = 5321;
        private const string SHIP_HANGAR_HOTKEY = "{PGDN}";
        private const string ITEM_HANGAR_HOTKEY = "{PGUP}";
        private const int TRADE_SHIPS = 1;
        private const int TRADE_ITEMS = 0;

        public AutoLister(ClientConfig clientConfig, UiElements uiElements, Paths paths, Character character, Modules modules, OrderAnalyzer orderAnalyzer)
            : base(clientConfig, uiElements, paths, character, modules, orderAnalyzer)
        {
            this.marketOrderio = new MarketOrderio();
            marketOrderio.path = paths.logPath;
            freeOrders = 0;
        }

        public int getNumberOfFreeOrders()
        {
            return freeOrders;
        }

        public void execute(Character character)
        {
            this.character = character;
            if (!isEVERunningForSelectedCharacter())
                return;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            prepare();

            if (character.tradeItems && freeOrders > 0)
                trade(TRADE_ITEMS, ITEM_HANGAR_HOTKEY, ITEM_TERMINAL_ID);
            if (character.tradeShips && freeOrders > 0)
                trade(TRADE_SHIPS, SHIP_HANGAR_HOTKEY, SHIP_TERMINAL_ITEM_ID);

            stopwatch.Stop();

            teardown(stopwatch.Elapsed.ToString());
        }

        private void prepare()
        {
            totalSellOrdersCreated = 0;
            totalBuyOrdersCreated = 0;
            currentBuyOrdersCreated = 0;
            currentSellOrdersCreated = 0;
            mouse.waitDuration = timing; //TODO Sync with client setting

            logger.autoListerLog(character.name);

            try { exportOrders(5, 30); }
            catch (Exception e) { throw e; }
            freeOrders = orderAnalyzer.orderSet.getNumberOfActiveOrders();

            openOrders = character.maximumOrders - orderAnalyzer.orderSet.getNumberOfActiveOrders();
            new SetClipboardHelper(DataFormats.Text, "0").Go();

            orderAnalyzer.clearLastBuyOrder();

            closeMarketAndHangarWindows();
        }

        private void trade(int type, String windowHotkey, int terminalId)
        {
            openHangar(windowHotkey);
            autoList(terminalId, type);

            mouse.waitDuration = timing;
            closeMarketAndHangarWindows();

            freeOrders -= (currentBuyOrdersCreated + currentSellOrdersCreated);
            totalBuyOrdersCreated += currentBuyOrdersCreated;
            totalSellOrdersCreated += currentSellOrdersCreated;
            currentSellOrdersCreated = currentBuyOrdersCreated = 0;
        }
        private void autoList(int terminalId, int hangarType)
        {
            int buyOrderQuantity;
            int currentHangarListPosition = 0;

            while (openOrders > 0)
            {
                if (currentHangarListPosition > 18)
                {
                    mouse.pointAndClick(LEFT, uiElements.itemsTop[0], uiElements.itemsTop[1] + (currentHangarListPosition * uiElements.itemsLineHeight), 40, 1, 40);
                    for (int k = 0; k < 19; ++k)
                        keyboard.send("{DOWN}");
                    wait(20);
                    currentHangarListPosition = 0;
                }

                viewDetailsAndExportResult(17, 2, currentHangarListPosition, hangarType);
                if (orderAnalyzer.getTypeId().Equals(terminalId))
                    return;
                try
                {
                    if (!orderAnalyzer.isSomeBuyOwned() && character.adjustBuys && character.tradeHistory.ContainsKey(orderAnalyzer.getTypeId()))
                    {
                        buyOrderQuantity = getBuyOrderQty(orderAnalyzer.getBuyPrice(), orderAnalyzer.getSellPrice());
                        placeBuyOrder(orderAnalyzer.getTypeId(), buyOrderQuantity);
                        
                        //If a new sell order is created for this item, it will expect the old best buy price unless we 
                        //update it with the price of the new buy order.
                        orderAnalyzer.setOwnedBuyPrice(orderAnalyzer.getBuyPrice() + .01);
                        ++currentBuyOrdersCreated;
                    }

                    if (!orderAnalyzer.isSomeSellOwned() && character.adjustSells && character.tradeHistory.ContainsKey(orderAnalyzer.getTypeId()))//If a new sell order needs to be placed.
                    {
                        openAndIdentifySellWindow(6, 2, currentHangarListPosition, hangarType);
                        placeSellOrder();

                        ++currentSellOrdersCreated;
                        --currentHangarListPosition;
                    }
                }
                catch (Exception e)
                {
                    errorCheck();
                    logger.log("AutoLister Failure!");
                }
                ++currentHangarListPosition;
            }
        }
        private void viewDetailsAndExportResult(int tries, double timingScaleFactor, int currentHangarListPosition, int hangarType)
        {
            int lastTypeId = orderAnalyzer.getTypeId();
            if (lastTypeId.Equals(0))
                lastTypeId = 806;
            int[] lineCoords = { uiElements.itemsTop[0], uiElements.itemsTop[1] + currentHangarListPosition * uiElements.itemsLineHeight };
            int[] viewDetailsOffset = { uiElements.itemsViewDetailsOffset[0], uiElements.itemsViewDetailsOffset[1] };
            DirectoryEraser.nuke(paths.logPath);
           
            for (int i = 0; i < tries; i++)
            {
                if (i > 2)
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
                mouse.pointAndClick(RIGHT, lineCoords, 1, 1, 1);
                //View details
                mouse.offsetAndClick(LEFT, viewDetailsOffset, 0, 2, 1);
                //TODO Make variable. Normal click route often causes inadvertant double-clicks on items, causing ships to be assembled and items
                //to be fitted. This left-click in a deadzone prevents such double clicks from occuring. 
                mouse.pointAndClick(LEFT, 120, 747, 1, 1, 1);

                if (hangarType == TRADE_ITEMS)
                    if (viewDetailsOffset[1].Equals(uiElements.itemsViewDetailsOffset[1]))
                        viewDetailsOffset[1] = uiElements.itemsViewDetailsOffset[1] + uiElements.itemsViewModuleDetailExtraOffset;
                    else
                        viewDetailsOffset[1] = uiElements.itemsViewDetailsOffset[1];

                if (i % 2 == 1)
                    mouse.waitDuration = Convert.ToInt32(mouse.waitDuration * timingScaleFactor);
                //Click on Export Market info
                mouse.pointAndClick(LEFT, uiElements.exportItem, 0, 5, 3);

                List<String[]> orderData = exportOrderData(lastTypeId);
                if (orderData != null)
                {
                    orderAnalyzer.analyzeInvestment(orderData, Convert.ToString(character.stationid));
                    mouse.waitDuration = timing;
                    return;
                }
            }
            logger.log("Failed to view item details and export result.");
            throw new Exception("Failed to view item details and export result.");
        }

        private List<String[]> exportOrderData(int lastTypeId)
        {
            String fileName;
            List<String[]> result;

            fileName = marketOrderio.getNewestFileNameInDirectory(paths.logPath);
            if (fileName != null && !fileName.Contains(modules.typeNames[lastTypeId]) && !fileName.Contains("My Orders"))
            {
                marketOrderio.fileName = fileName;
                result = marketOrderio.read();
                if (result != null && result.Count > 0)
                    return result;
            }
            return null;
        }

        private void openAndIdentifySellWindow(int tries, double timingScaleFactor, int currentHangarListPosition, int hangarType)
        {
            Double clipboardValue = 0;
            Double verificationValue = Math.Max(orderAnalyzer.getOwnedBuyPrice(), orderAnalyzer.getBuyPrice());
            int[] lineCoords = { uiElements.itemsTop[0], uiElements.itemsTop[1] + currentHangarListPosition * uiElements.itemsLineHeight };
            int[] sellItemOffset = { uiElements.itemsSellItemOffset[0], uiElements.itemsSellItemOffset[1] };

            if (hangarType == TRADE_ITEMS && !modules.fittableModuleTypeIDs.ContainsKey(orderAnalyzer.getTypeId()))
            {
                sellItemOffset[1] += uiElements.itemsViewModuleDetailExtraOffset;
            }

            for (int i = 0; i < tries; i++)
            {
                cancelOrder(0, 0);
                //RClick on current line.
                mouse.pointAndClick(RIGHT, lineCoords, 0, 1, 1);
                mouse.offsetAndClick(LEFT, sellItemOffset, 0, 1, 1);

                if (i % 3 == 2)
                    mouse.waitDuration = Convert.ToInt32(mouse.waitDuration * timingScaleFactor);

                //Right click on the field
                mouse.pointAndClick(RIGHT, fixCoordsForLongTypeName(orderAnalyzer.getTypeId(), uiElements.sellOrderBox), 5, 2, 2);
                //Click on copy
                mouse.offsetAndClick(LEFT, uiElements.copyOffset, 0, 2, 2);
                try { clipboardValue = Convert.ToDouble(Clipboard.getTextFromClipboard()); }
                catch { clipboardValue = 0; }

                if (clipboardValue.Equals(verificationValue))
                {
                    mouse.waitDuration = timing;
                    return;
                }
            }
            logger.log("Failed to open and identify the sell window!");
            throw new Exception("Failed to open and identify the sell window!");
        }

        private void teardown(String timeElapsed)
        {
            cancelOrder(0, 0); //Clean up after self.. don't leave any windows open!
            logger.log(character.name + ": AL made " + totalSellOrdersCreated + " sells, " + totalBuyOrdersCreated + " buys in " + timeElapsed);
            freeOrders -= (totalBuyOrdersCreated + totalSellOrdersCreated);
        }

        private void openHangar(string hotkey)
        {
            wait(5);
            keyboard.send(hotkey);
            wait(40);
        }

        private void placeSellOrder()
        {
            inputValue(5, 2, fixCoordsForLongTypeName(orderAnalyzer.getTypeId(), uiElements.sellOrderBox), (orderAnalyzer.getSellPrice() - .01).ToString());
            confirmOrder(fixCoordsForLongTypeName(orderAnalyzer.getTypeId(), uiElements.OrderBoxOK), 1, 1);
        }

        private void resetView(int tradeType)
        {
            errorCheck();
            closeMarketAndHangarWindows();

            if (tradeType == 0)
                keyboard.send("{PGUP}");
            else
                keyboard.send("{PGDN}");

        }
    }
}
