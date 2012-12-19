using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using noxiousET.src.data.characters;
using noxiousET.src.data.client;
using noxiousET.src.data.io;
using noxiousET.src.data.modules;
using noxiousET.src.data.paths;
using noxiousET.src.data.uielements;
using noxiousET.src.orders;

namespace noxiousET.src.guiInteraction.orders.autoinvester
{
    internal class AutoInvestor : OrderBot
    {
        private readonly MarketOrderio _marketOrderio;
        private int _itemsScanned;
        private int _ordersCreated;

        public AutoInvestor(ClientConfig clientConfig, UiElements uiElements, Paths paths, Character character,
                            Modules modules, OrderAnalyzer orderAnalyzer)
            : base(clientConfig, uiElements, paths, character, modules, orderAnalyzer)
        {
            _marketOrderio = new MarketOrderio();
            _marketOrderio.Path = paths.LogPath;
        }

        public void GetTypeForCharacterFromQuickbar(Character character, String firstItemId, String lastItemId)
        {
            Character = character;
            var newTradeHistory = new List<int>();
            int offset = 0;
            int visibleRows = UiElements.MarketWindowQuickbarVisibleRows;
            var lastVisibleRowCoords = new[]
                {
                    UiElements.MarketWindowQuickbarFirstRow[0],
                    UiElements.MarketWindowQuickbarFirstRow[1] +
                    ((visibleRows - 1)*UiElements.StandardRowHeight)
                };
            String lastTypeName = "no last type name just yet";

            ExportOrders(4, 30);

            _marketOrderio.FileName = ExecuteQueryAndExportResult(5, 1.2, offset);
            OrderAnalyzer.AnalyzeInvestment(_marketOrderio.Read(), Convert.ToString(character.StationId));
            if (OrderAnalyzer.GetTypeId() != int.Parse(firstItemId))
                throw new Exception("First type id does not match discovered type id");
            newTradeHistory.Add(OrderAnalyzer.GetTypeId());
            int previousItemId = OrderAnalyzer.GetTypeId();
            offset++;

            while (previousItemId != int.Parse(lastItemId))
            {
                if (offset > (visibleRows - 1))
                {
                    Mouse.PointAndClick(Left, lastVisibleRowCoords, 1, 1, 1);
                    Keyboard.Send("{DOWN}");
                    offset = visibleRows - 1;
                }
                DoExport(offset, visibleRows, lastVisibleRowCoords, lastTypeName);
                OrderAnalyzer.AnalyzeInvestment(_marketOrderio.Read(), Convert.ToString(character.StationId));
                newTradeHistory.Add(OrderAnalyzer.GetTypeId());
                previousItemId = OrderAnalyzer.GetTypeId();
                lastTypeName = Modules.TypeNames[previousItemId];
                offset++;
            }
            Dictionary<int, int> newTradeHistoryDictionary = newTradeHistory.ToDictionary(n => n);

            character.TradeHistory = newTradeHistoryDictionary;
        }

        private void DoExport(int offset, int visibleRows, int[] lastVisibleRowCoords, String lastTypeName)
        {
            for (int i = 0; i < 10; i++)
            {
                if (i == 9 && offset == visibleRows - 1)
                {
                    Mouse.PointAndClick(Left, lastVisibleRowCoords, 1, 1, 1);
                    Keyboard.Send("{DOWN}");
                }
                try
                {
                    _marketOrderio.FileName = ExecuteQueryAndExportResult(5, 1.2, offset);
                    return;
                }
                catch
                {
                }
            }
        }

        public void Execute(Character character)
        {
            Character = character;
            _itemsScanned = 0;
            _ordersCreated = 0;
            if (character.TradeQueue.Count < 1)
            {
                foreach (int item in character.TradeHistory.Keys)
                {
                    character.TradeQueue.Enqueue(item);
                }
            }
            if (character.TradeQueue.Count > 0)
            {
                try
                {
                    ExportOrders(4, 30);
                    if (OrderAnalyzer.OrderSet.GetNumberOfActiveOrders() >= character.MaximumOrders)
                        return;
                    PrepareEnvironment();
                }
                catch (Exception e)
                {
                    Logger.Log("AI failed to prepare environment!");
                    throw e;
                }


                try
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    CreateInvestments();
                    stopwatch.Stop();
                    Logger.Log(character.Name + ": AI scanned " + _itemsScanned + " and made " + _ordersCreated +
                               " buys " + stopwatch.Elapsed.ToString());

                    //If we failed to create any orders, the queue is full of items that aren't worth trading. Flush it and enqueue all known items.
                    if (_ordersCreated == 0)
                    {
                        character.TradeQueue = new Queue<int>();
                        foreach (int item in character.TradeHistory.Keys)
                        {
                            character.TradeQueue.Enqueue(item);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Log("Auto investor failed to complete run.");
                    Logger.LogError(e.Message);
                }
            }
            else
            {
                Logger.Log(character.Name +
                           " has no items in the queue for Auto Investor to process. Auto Investor aborted.");
            }
        }

        private void PrepareEnvironment()
        {
            try
            {
                if (IsEveRunningForSelectedCharacter())
                {
                    Mouse.PointAndClick(Left, UiElements.MarketWindowDeadspace, 50, 1, 50);
                    Mouse.PointAndClick(Left, UiElements.MarketWindowQuickbarFirstRow, 1, 1, 1);
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


        private void CreateInvestments()
        {
            ConsecutiveFailures = 0;
            int currentPosition = 0;
            int count = 0;
            int freeOrders = Character.MaximumOrders - OrderAnalyzer.OrderSet.GetNumberOfActiveOrders();
            int currentOffset = currentPosition;
            int visibleRows = UiElements.MarketWindowQuickbarVisibleRows;
            List<int> tradeQueue = Modules.GetTypeIdsAlphabetizedByItemName(Character.TradeHistory.Keys);
            int size = tradeQueue.Count;

            while (currentOffset > visibleRows)
            {
                currentOffset -= visibleRows;
                GoToNextQuickbarPage();
            }
            if (size - currentPosition <= visibleRows)
            {
                GoToNextQuickbarPage();
                currentOffset = visibleRows - (size - currentPosition);
            }
            do
            {
                if (currentPosition == size)
                {
                    currentPosition = 0;
                    GoToFirstQuickbarPage();
                    currentOffset = 0;
                }
                else if (currentOffset > (visibleRows - 1))
                {
                    GoToNextQuickbarPage();
                    if (size - currentPosition <= visibleRows)
                        currentOffset = visibleRows - (size - currentPosition);
                    else
                        currentOffset = 0;
                }
                String expectedTypeName = Modules.TypeNames[tradeQueue[currentPosition]];
                int currentTypeId = tradeQueue[currentPosition];
                if (OrderAnalyzer.OrderSet.CheckForActiveOrders(tradeQueue[currentPosition]) == 0)
                {
                    try
                    {
                        _marketOrderio.FileName = ExecuteQueryAndExportResult(5, 1.2, currentOffset);
                        OrderAnalyzer.AnalyzeInvestment(_marketOrderio.Read(), Convert.ToString(Character.StationId));
                        String foundTypeName = Modules.TypeNames[OrderAnalyzer.GetTypeId()];
                        if (foundTypeName.Equals(expectedTypeName) && !OrderAnalyzer.IsSomeBuyOwned() &&
                            !OrderAnalyzer.IsSomeSellOwned())
                        {
                            //Uses data from orderAnalyzer.analyzeInvestment to decide if a buy order should be made
                            int quantity = GetBuyOrderQuantity(OrderAnalyzer.GetBuyPrice(), OrderAnalyzer.GetSellPrice());

                            if (quantity > 0)
                            {
                                OpenAndIdentifyBuyWindow(currentTypeId, OrderAnalyzer.GetSellPrice());
                                PlaceBuyOrder(currentTypeId, quantity);
                                freeOrders--;
                                _ordersCreated++;
                                //logger.log(modules.typeNames[currentTypeId] + " should create buy order.");
                            }
                            else if (foundTypeName.CompareTo(expectedTypeName) > 0)
                            {
                                currentOffset -= 2;
                                currentPosition--;
                                count--;
                            }
                            else if (foundTypeName.CompareTo(expectedTypeName) < 0)
                            {
                                currentPosition--;
                                count--;
                            }
                            else
                            {
                                //logger.log(modules.typeNames[currentTypeId] + " should create buy order, but quantity was 0.");
                            }
                        }
                        ConsecutiveFailures = 0;
                    }
                    catch (Exception e)
                    {
                        ++ConsecutiveFailures;
                        if (ConsecutiveFailures > 4)
                            return;
                        Logger.Log(e.Message);
                    }
                }
                ++currentPosition;
                ++currentOffset;
                ++count;
                _itemsScanned++;
                if (freeOrders == 0)
                    return;
            } while (count < size);
        }

        private void GoToNextQuickbarPage()
        {
            int visibleRows = UiElements.MarketWindowQuickbarVisibleRows;
            Mouse.PointAndClick(Left, UiElements.MarketWindowQuickbarFirstRow[0],
                                UiElements.MarketWindowQuickbarFirstRow[1] +
                                ((visibleRows - 1)*UiElements.StandardRowHeight), 30, 1, 30);
            for (int i = 0; i < visibleRows; i++)
            {
                Keyboard.Send("{DOWN}");
            }
            Wait(30);
        }

        private void GoToFirstQuickbarPage()
        {
            Mouse.Drag(UiElements.MarketWindowQuickbarScrollbarBottom, UiElements.MarketWindowQuickbarScrollbarTop, 20,
                       20, 20);
        }

        private string ExecuteQueryAndExportResult(int tries, double timingScaleFactor, int offSet)
        {
            DirectoryEraser.Nuke(Paths.LogPath);
            if (_marketOrderio.GetNumberOfFilesInDirectory(Paths.LogPath) != 0)
                throw new Exception("Could not clean log path directory");

            for (int i = 0; i < tries; i++)
            {
                Mouse.PointAndClick(Left, UiElements.MarketWindowQuickbarFirstRow[0],
                                    UiElements.MarketWindowQuickbarFirstRow[1] + (offSet*UiElements.StandardRowHeight),
                                    1, 1, 1);
                Mouse.PointAndClick(Left, UiElements.MarketExportButton, 10, 1, 10);
                String fileName = _marketOrderio.GetNewestFileNameInDirectory(Paths.LogPath);
                if (fileName != null)
                {
                    Mouse.WaitDuration = Timing;
                    return fileName;
                }
                //logger.log("Found last in file name/null file name.");
                Mouse.WaitDuration = Convert.ToInt32(Mouse.WaitDuration*timingScaleFactor);
                if (i > 2)
                    ErrorCheck();
            }
            Mouse.WaitDuration = Timing;
            throw new Exception("Could not export query result");
        }
    }
}