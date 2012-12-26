using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using noxiousET.src.data.characters;
using noxiousET.src.data.client;
using noxiousET.src.data.io;
using noxiousET.src.data.modules;
using noxiousET.src.data.paths;
using noxiousET.src.data.uidata;
using noxiousET.src.helpers;
using noxiousET.src.orders;

namespace noxiousET.src.guiInteraction.orders.autolister
{
    internal class AutoLister : OrderBot
    {
        private const int ShipTerminalItemId = 2078;
        private const int ItemTerminalId = 5321;
        private const string ShipHangarHotkey = "{PGDN}";
        private const string ItemHangarHotkey = "{PGUP}";
        private const int TradeShips = 1;
        private const int TradeItems = 0;
        private readonly MarketOrderio _marketOrderio;
        private int _currentBuyOrdersCreated;
        private int _currentSellOrdersCreated;
        private int _openOrders;
        private int _totalBuyOrdersCreated;
        private int _totalSellOrdersCreated;
        private int _currentItemType;
        private const int FittableItem = 0;
        private const int NonFittableItem = 1;
        private const int UnknownItemType = 2;
        private PixelReader _pixelReader;
        private const int ContextMenuHorizontalRuleColor = 4210752;
        private const int ContextMenuBackgroundColor = 0;

        public AutoLister(ClientConfig clientConfig, EveUi eveUi, Paths paths, Character character,
                          Modules modules, OrderAnalyzer orderAnalyzer)
            : base(clientConfig, eveUi, paths, character, modules, orderAnalyzer)
        {
            _marketOrderio = new MarketOrderio {Path = paths.LogPath};
            FreeOrders = 0;
            _pixelReader = new PixelReader();
        }

        public int FreeOrders { set; get; }

        public void Execute(Character character)
        {
            Character = character;
            if (!IsEveRunningForSelectedCharacter())
                return;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                Prepare();
            }
            catch (Exception)
            {
                Logger.Log("AL failed to prepare environment!");
                throw;
            }

            if (character.ShouldTradeItems && FreeOrders > 0)
                Trade(TradeItems, ItemHangarHotkey, ItemTerminalId);
            if (character.ShouldTradeShips && FreeOrders > 0)
                Trade(TradeShips, ShipHangarHotkey, ShipTerminalItemId);

            stopwatch.Stop();

            Teardown(stopwatch.Elapsed.ToString());
        }

        private void Prepare()
        {
            _totalSellOrdersCreated = 0;
            _totalBuyOrdersCreated = 0;
            _currentBuyOrdersCreated = 0;
            _currentSellOrdersCreated = 0;
            Mouse.WaitDuration = Timing; //TODO Sync with client setting

            Logger.AutoListerLog(Character.Name);

            ExportOrders(5, 30);
            FreeOrders = OrderAnalyzer.OrderSet.GetNumberOfActiveOrders();

            _openOrders = Character.MaximumOrders - OrderAnalyzer.OrderSet.GetNumberOfActiveOrders();
            new SetClipboardHelper(DataFormats.Text, EtConstants.ClipboardNullValue).Go();

            CloseMarketAndHangarWindows();
        }

        private void Trade(int type, String windowHotkey, int terminalId)
        {
            OpenHangar(windowHotkey);
            Wait(10);
            AutoList(terminalId, type);

            Mouse.WaitDuration = Timing;

            FreeOrders -= (_currentBuyOrdersCreated + _currentSellOrdersCreated);
            _totalBuyOrdersCreated += _currentBuyOrdersCreated;
            _totalSellOrdersCreated += _currentSellOrdersCreated;
            _currentSellOrdersCreated = _currentBuyOrdersCreated = 0;
        }

        private void AutoList(int terminalId, int hangarType)
        {
            int currentHangarListPosition = 0;
            int consecutiveFailures = 0;

            while (_openOrders > 0)
            {
                _currentItemType = UnknownItemType;
                //TODO Remove hardcoded value
                if (currentHangarListPosition > 18)
                {
                    Mouse.PointAndClick(Left, EveUi.HangarFirstRow[0],
                                        EveUi.HangarFirstRow[1] +
                                        (currentHangarListPosition*EveUi.HangarRowHeight), 40, 1, 40);
                    for (int k = 0; k < 19; ++k)
                        Keyboard.Send("{DOWN}");
                    Wait(20);
                    currentHangarListPosition = 0;
                }
                try
                {
                    ViewDetailsAndExportResult(10, 1.5, currentHangarListPosition, hangarType);
                    if (OrderAnalyzer.GetTypeId().Equals(terminalId))
                        return;

                    if (!OrderAnalyzer.IsSomeBuyOwned() && Character.ShouldAdjustBuys &&
                        Character.TradeHistory.ContainsKey(OrderAnalyzer.GetTypeId()))
                    {
                        int buyOrderQuantity = GetBuyOrderQuantity(OrderAnalyzer.GetBuyPrice(),
                                                                   OrderAnalyzer.GetSellPrice());
                        if (buyOrderQuantity > 0)
                        {
                            PlaceBuyOrder(OrderAnalyzer.GetTypeId(), buyOrderQuantity);
                            //If a new sell order is created for this item, it will expect the old best buy price unless we 
                            //update it with the price of the new buy order.
                            OrderAnalyzer.SetOwnedBuyPrice(OrderAnalyzer.GetBuyPrice() + .01);
                            OrderAnalyzer.OrderSet.AddOrder(OrderAnalyzer.GetTypeId(), EtConstants.Buy);
                            --_openOrders;
                            ++_currentBuyOrdersCreated;
                        }
                    }

                    if (!OrderAnalyzer.IsSomeSellOwned() && Character.ShouldAdjustSells &&
                        Character.TradeHistory.ContainsKey(OrderAnalyzer.GetTypeId()))
                        //If a new sell order needs to be placed.
                    {
                        OpenAndIdentifySellWindow(6, 2, currentHangarListPosition, hangarType);
                        PlaceSellOrder();
                        OrderAnalyzer.OrderSet.AddOrder(OrderAnalyzer.GetTypeId(), EtConstants.Sell);
                        --_openOrders;
                        ++_currentSellOrdersCreated;
                        --currentHangarListPosition;
                    }
                    consecutiveFailures = 0;
                }
                catch (Exception e)
                {
                    ++consecutiveFailures;
                    if (consecutiveFailures > 4)
                        return;
                    Mouse.WaitDuration = Timing;
                    ErrorCheck();
                    Logger.Log("AutoLister Failure! " + e.Message);
                }
                ++currentHangarListPosition;
            }
        }

        private int OpenContextMenuAndGetItemType(int[] rowCoords)
        {
            Mouse.PointAndClick(Right, rowCoords, 1, 1, 20);
            int color = _pixelReader.GetPixelColor(rowCoords[0] + 75, rowCoords[1] + 31);
            if (color.Equals(ContextMenuBackgroundColor))
            {
                return NonFittableItem;
            }
            if (color.Equals(ContextMenuHorizontalRuleColor))
            {
                 return FittableItem;
            }
            return UnknownItemType;
        }


        private void ViewDetailsAndExportResult(int tries, double timingScaleFactor, int currentHangarListPosition,
                                                int hangarType)
        {
            int lastTypeId = OrderAnalyzer.GetTypeId();
            if (lastTypeId.Equals(0))
                lastTypeId = 806;
            int[] lineCoords = {
                                   EveUi.HangarFirstRow[0],
                                   EveUi.HangarFirstRow[1] + currentHangarListPosition*EveUi.HangarRowHeight
                               };
            DirectoryEraser.Nuke(Paths.LogPath);
            
            for (int i = 0; i < tries; i++)
            {
                if (i > 2)
                {
                    int errorCode = GetError();
                    ErrorCheck();
                    if (errorCode == EtConstants.AlertItemDestruction || errorCode == EtConstants.AlertImplantsLostOnDeath)
                    {
                        Mouse.PointAndClick(Left, EveUi.HangarFirstRow, 1, 1, 1);
                    }
                }
                //Open Context Menu, and use its structure to determine what kind of item we're looking at.
                _currentItemType = OpenContextMenuAndGetItemType(lineCoords);
                Mouse.OffsetAndClick(Left, GenerateViewDetailsOffset(), 0, 2, 1);

                
                if (i%2 == 1)
                    Mouse.WaitDuration = Convert.ToInt32(Mouse.WaitDuration*timingScaleFactor);
                //Click on Export Market info
                if (!_currentItemType.Equals(UnknownItemType))
                {
                    Mouse.PointAndClick(Left, EveUi.MarketExportButton, 12, 5, 3);

                    List<String[]> orderData = ExportOrderData();
                    if (orderData != null && OrderAnalyzer.IsNewOrderData(orderData))
                    {
                        OrderAnalyzer.AnalyzeInvestment(orderData, Convert.ToString(Character.StationId));
                        Mouse.WaitDuration = Timing;
                        return;
                    }
                }
            }
            Logger.Log("Failed to view item details and export result.");
            Mouse.WaitDuration = Timing;
            throw new Exception("Failed to view item details and export result.");
        }

        private int[] GenerateViewDetailsOffset()
        {
            if (_currentItemType.Equals(FittableItem))
            {
                return new[]
                           {
                               EveUi.HangarContextMenuViewDetailsOffset[0],
                               EveUi.HangarContextMenuViewDetailsOffset[1] +
                               EveUi.HangarContextMenuExtraYOffsetForModules
                           };
            }
            return EveUi.HangarContextMenuViewDetailsOffset;
        }

        private List<String[]> ExportOrderData()
        {
            string fileName = _marketOrderio.GetNewestFileNameInDirectory(Paths.LogPath);
            if (fileName != null && !fileName.Contains("My Orders"))
            {
                _marketOrderio.FileName = fileName;
                List<String[]> result = _marketOrderio.Read();
                if (result != null && result.Count > 0)
                    return result;
            }
            return null;
        }

        private void OpenAndIdentifySellWindow(int tries, double timingScaleFactor, int currentHangarListPosition,
                                               int hangarType)
        {
            Double verificationValue = Math.Max(OrderAnalyzer.GetOwnedBuyPrice(), OrderAnalyzer.GetBuyPrice());
            int[] lineCoords = {
                                   EveUi.HangarFirstRow[0],
                                   EveUi.HangarFirstRow[1] + currentHangarListPosition*EveUi.HangarRowHeight
                               };
            int[] sellItemOffset = {
                                       EveUi.HangarContextMenuSellItemOffset[EtConstants.X],
                                       EveUi.HangarContextMenuSellItemOffset[EtConstants.Y]
                                   };

            if (_currentItemType.Equals(FittableItem))
            {
                sellItemOffset[EtConstants.Y] += EveUi.HangarContextMenuExtraYOffsetForModules;
            }

            for (int i = 0; i < tries; i++)
            {
                CancelOrder(0, 0);
                //RClick on current line.
                Mouse.PointAndClick(Right, lineCoords, 0, 1, 1);

                Mouse.OffsetAndClick(Left, sellItemOffset, 0, 1, 1);

                if (i%3 == 2)
                    Mouse.WaitDuration = Convert.ToInt32(Mouse.WaitDuration*timingScaleFactor);

                //Right click on the field
                Mouse.PointAndClick(Double,
                                    GenerateSellWindowCoords(OrderAnalyzer.GetTypeId(), EveUi.SellBidPriceField), 
                                    50, 2, 2);
                //Click on copy
                Keyboard.Shortcut(new[] { Keyboard.VkLcontrol }, Keyboard.VkC);
                Double clipboardValue;
                try
                {
                    clipboardValue = Convert.ToDouble(Clipboard.GetTextFromClipboard());
                }
                catch
                {
                    clipboardValue = 0;
                }

                if (Math.Abs(verificationValue - clipboardValue) < 1)
                {
                    Mouse.WaitDuration = Timing;
                    return;
                }
            }
            Logger.Log("Failed to open and identify the sell window!");
            throw new Exception("Failed to open and identify the sell window!");
        }

        private void Teardown(String timeElapsed)
        {
            CancelOrder(0, 0); //Clean up after self.. don't leave any windows open!
            Logger.Log(Character.Name + ": AL made " + _totalSellOrdersCreated + " sells, " + _totalBuyOrdersCreated +
                       " buys in " + timeElapsed);
            FreeOrders -= (_totalBuyOrdersCreated + _totalSellOrdersCreated);
        }

        private void OpenHangar(string hotkey)
        {
            Wait(5);
            Keyboard.Send(hotkey);
            Wait(40);
        }

        private void PlaceSellOrder()
        {
            InputValue(5, 1.4, GenerateSellWindowCoords(OrderAnalyzer.GetTypeId(), EveUi.SellBidPriceField),
                       (OrderAnalyzer.GetSellPrice() - .01).ToString());
            ConfirmOrder(GenerateSellWindowCoords(OrderAnalyzer.GetTypeId(), EveUi.OrderBoxConfirm), 1, EtConstants.IsSellOrder);
        }

        private void ResetView(int tradeType)
        {
            ErrorCheck();
            CloseMarketAndHangarWindows();

            if (tradeType == 0)
                Keyboard.Send("{PGUP}");
            else
                Keyboard.Send("{PGDN}");
        }
    }
}