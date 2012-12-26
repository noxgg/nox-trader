using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using noxiousET.src.data.characters;
using noxiousET.src.data.client;
using noxiousET.src.data.io;
using noxiousET.src.data.modules;
using noxiousET.src.data.paths;
using noxiousET.src.data.uidata;
using noxiousET.src.orders;

namespace noxiousET.src.guiInteraction.orders.autoinvester
{
    internal class AutoInvestor : OrderBot
    {
        private readonly MarketOrderio _marketOrderio;
        private int _itemsScanned;
        private int _ordersCreated;
        private const int InvestorFailureLimit = 4;

        public AutoInvestor(ClientConfig clientConfig, EveUi ui, Paths paths, Character character,
                            Modules modules, OrderAnalyzer orderAnalyzer)
            : base(clientConfig, ui, paths, character, modules, orderAnalyzer)
        {
            _marketOrderio = new MarketOrderio();
            _marketOrderio.Path = paths.LogPath;
        }

        public void Execute(Character character)
        {
            Character = character;
            _itemsScanned = 0;
            _ordersCreated = 0;

            PrepareEnvironment(character);

            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                CreateInvestments();

                stopwatch.Stop();
                Logger.Log(character.Name + ": AI scanned " + _itemsScanned + " and made " + _ordersCreated +
                           " buys " + stopwatch.Elapsed.ToString());
            }
            catch (Exception e)
            {
                Logger.Log("Auto investor failed to complete run.");
                Logger.LogError(e.Message);
            }
        }

        private void PrepareEnvironment(Character character)
        {
            if (IsEveRunningForSelectedCharacter())
            {
                //Bring the market window to the front
                Mouse.PointAndClick(Left, EveUi.MarketWindowDeadspace, 50, 1, 50);
                Mouse.PointAndClick(Left, EveUi.MarketWindowQuickbarFirstRow, 1, 1, 1);
            }
            else
            {
                Logger.Log("AI failed to prepare environment!");
                throw new Exception("Auto investor ould not find EVE Client for selected character.");
            }

            ExportOrders(4, 30);
            if (OrderAnalyzer.OrderSet.GetNumberOfActiveOrders() >= character.MaximumOrders)
            {
                throw new Exception(
                    "This character already has the maximum number of orders! Cannot create additional investments");
            }
        }

        private int PaginateToFirstItem(int currentPosition, int visibleRows, int size)
        {
            int currentOffset = currentPosition;
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

            return currentOffset;
        }

        private void CreateInvestments()
        {
            int consecutiveFailures = 0;
            int currentPosition = 0;
            int count = 0;
            int freeOrders = Character.MaximumOrders - OrderAnalyzer.OrderSet.GetNumberOfActiveOrders();
            int visibleRows = EveUi.MarketWindowQuickbarVisibleRows;
            List<int> tradeQueue = Modules.GetTypeIdsAlphabetizedByItemName(Character.TradeHistory.Keys);
            int size = tradeQueue.Count;

            int currentOffset = PaginateToFirstItem(currentPosition, visibleRows, size);

            do
            {
                if (currentPosition == size)
                {
                    currentPosition = 0;
                    GoToFirstQuickbarPage();
                    currentOffset = 0;
                    Wait(4);
                }
                else if (currentOffset > (visibleRows - 1))
                {
                    GoToNextQuickbarPage();
                    if (size - currentPosition <= visibleRows)
                        currentOffset = visibleRows - (size - currentPosition);
                    else
                        currentOffset = 0;
                    Wait(4);
                }

                String expectedTypeName = Modules.TypeNames[tradeQueue[currentPosition]];
                int currentTypeId = tradeQueue[currentPosition];

                if (!OrderAnalyzer.OrderSet.ExistsAnyOrder(tradeQueue[currentPosition]))
                {
                    try
                    {
                        _marketOrderio.FileName = ExecuteQueryAndExportResult(5, 1.2, currentOffset);
                        OrderAnalyzer.AnalyzeInvestment(_marketOrderio.Read(), Convert.ToString(Character.StationId));
                        String foundTypeName = Modules.TypeNames[OrderAnalyzer.GetTypeId()];

                        if (foundTypeName.Equals(expectedTypeName) && !OrderAnalyzer.IsSomeOrderOwned())
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
                                consecutiveFailures = 0;
                            }
                            else
                            {
                                //logger.log(modules.typeNames[currentTypeId] + " should create buy order, but quantity was 0.");
                            }
                        consecutiveFailures = 0;
                        }
                        else if (foundTypeName.CompareTo(expectedTypeName) > 0)
                        {
                            currentOffset -= 2;
                            currentPosition--;
                            count--;
                            consecutiveFailures++;
                        }
                        else if (foundTypeName.CompareTo(expectedTypeName) < 0)
                        {
                            currentPosition--;
                            count--;
                            consecutiveFailures++;
                        }
                    }
                    catch (Exception e)
                    {
                        ++consecutiveFailures;
                        if (consecutiveFailures > InvestorFailureLimit)
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
            int visibleRows = EveUi.MarketWindowQuickbarVisibleRows;
            Mouse.PointAndClick(Left, CalculateRowCoords(visibleRows - 1), 30, 1, 30);
            for (int i = 0; i < visibleRows; i++)
            {
                Keyboard.Send("{DOWN}");
            }
            Wait(30);
        }

        private int[] CalculateRowCoords(int offset)
        {
            return new[]
                       {
                           EveUi.MarketWindowQuickbarFirstRow[EtConstants.X],
                           EveUi.MarketWindowQuickbarFirstRow[EtConstants.Y] + offset*EveUi.StandardRowHeight
                       };
        }

        private void GoToFirstQuickbarPage()
        {
            Mouse.Drag(EveUi.MarketWindowQuickbarScrollbarBottom, EveUi.MarketWindowQuickbarScrollbarTop, 20,
                       20, 20);
        }

        private string ExecuteQueryAndExportResult(int tries, double timingScaleFactor, int offSet)
        {
            DirectoryEraser.Nuke(Paths.LogPath);
            if (_marketOrderio.GetNumberOfFilesInDirectory(Paths.LogPath) != 0)
                throw new Exception("Could not clean log path directory");

            for (int i = 0; i < tries; i++)
            {
                Mouse.PointAndClick(Left, CalculateRowCoords(offSet), 1, 1, 1);
                Mouse.PointAndClick(Left, EveUi.MarketExportButton, 10, 1, 10);
                String fileName = _marketOrderio.GetNewestFileNameInDirectory(Paths.LogPath);
                if (fileName != null)
                {
                    Mouse.WaitDuration = Timing;
                    return fileName;
                }
                Mouse.WaitDuration = Convert.ToInt32(Mouse.WaitDuration*timingScaleFactor);
                if (i > 2)
                    ErrorCheck();
            }
            Mouse.WaitDuration = Timing;
            throw new Exception("Could not export query result");
        }


        //For synching with character's quickbar.
        public void GetTypeForCharacterFromQuickbar(Character character, String firstItemId, String lastItemId)
        {
            Character = character;
            var newTradeHistory = new List<int>();
            int offset = 0;
            int visibleRows = EveUi.MarketWindowQuickbarVisibleRows;
            var lastVisibleRowCoords = new[]
                {
                    EveUi.MarketWindowQuickbarFirstRow[0],
                    EveUi.MarketWindowQuickbarFirstRow[1] +
                    ((visibleRows - 1)*EveUi.StandardRowHeight)
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
    }
}