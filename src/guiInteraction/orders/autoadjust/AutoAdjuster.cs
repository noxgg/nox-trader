using System;
using System.Collections.Generic;
using System.Diagnostics;
using noxiousET.src.data.characters;
using noxiousET.src.data.client;
using noxiousET.src.data.io;
using noxiousET.src.data.modules;
using noxiousET.src.data.paths;
using noxiousET.src.data.uielements;
using noxiousET.src.orders;

namespace noxiousET.src.guiInteraction.orders.autoadjuster
{
    internal class AutoAdjuster : OrderBot
    {
        private readonly MarketOrderio _marketOrderio;
        private readonly OrderReviewer _orderReviewer;
        private String _fileName;
        private int _freeOrders;
        private int _numModified;
        private int _numScanned;

        public AutoAdjuster(ClientConfig clientConfig, UiElements uiElements, Paths paths, Character character,
                            Modules modules, OrderAnalyzer orderAnalyzer, OrderReviewer orderReviewer)
            : base(clientConfig, uiElements, paths, character, modules, orderAnalyzer)
        {
            _orderReviewer = orderReviewer;
            _marketOrderio = new MarketOrderio {Path = paths.LogPath};
        }

        public int GetNumberOfFreeOrders()
        {
            return _freeOrders;
        }

        public void Execute(Character character)
        {
            if (!character.ShouldAdjustBuys && !character.ShouldAdjustSells)
                return;
            Character = character;

            try
            {
                Prepare();
            }
            catch (Exception)
            {
                Logger.Log("AA failed to prepare environment!");
                throw;
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            if (character.ShouldAdjustBuys)
                Adjust(EtConstants.Buy, UiElements.WalletBuyListFirstRow, UiElements.WalletBuyListSortByType);
            if (character.ShouldAdjustSells)
                Adjust(EtConstants.Sell, UiElements.WalletSellListFirstRow, UiElements.WalletSellListSortByType);
            stopwatch.Stop();
            Logger.Log(character.Name + ": AA scanned " + _numScanned + ", adjusted " + _numModified + " in " +
                       stopwatch.Elapsed.ToString());
        }

        private void Adjust(int typeToAdjust, int[] topLineCoords, int[] sortByTypeCoords)
        {
            ConsecutiveFailures = 0;
            var cursorPosition = new int[2];
            int visibleLines = UiElements.WalletVisibleRows[typeToAdjust];
            int currentTypeId = 5321;
            bool modifiedOnLastIteration = false;

            int ceiling =
                Convert.ToInt32(
                    Math.Ceiling(OrderAnalyzer.OrderSet.GetNumberOfBuysAndSells()[typeToAdjust]/
                                 Convert.ToDouble(UiElements.WalletVisibleRows[typeToAdjust])));
            for (int i = 0; i < ceiling; ++i)
            {
                cursorPosition[0] = topLineCoords[0];
                cursorPosition[1] = topLineCoords[1];

                for (int j = 0; j < visibleLines; j++)
                {
                    try
                    {
                        if (EveHandle != GetForegroundWindow())
                            SetForegroundWindow(EveHandle);

                        _marketOrderio.FileName = ExecuteQueryAndExportResult(3, 2, cursorPosition, ref currentTypeId);
                        List<String[]> orderData = _marketOrderio.Read();
                        OrderAnalyzer.AnalyzeInvestment(orderData, Convert.ToString(Character.StationId));
                        currentTypeId = OrderAnalyzer.GetTypeId();
                        if (modifiedOnLastIteration)
                        {
                            ConfirmOrder(UiElements.OrderBoxConfirm, 1, typeToAdjust);
                            modifiedOnLastIteration = false;
                            ++_numModified;
                        }
                        if (!OrderAnalyzer.IsBestOrderOwned(typeToAdjust) && ShouldAdjustOrder(ref typeToAdjust) ||
                            OrderAnalyzer.IsBestOrderOwned(typeToAdjust) && IsAnOverbid(typeToAdjust))
                        {
                            OpenAndIdentifyModifyWindow(10, 1.2, cursorPosition,
                                                        OrderAnalyzer.GetOwnedPrice(typeToAdjust));
                            InputValue(5, 1.4, UiElements.ModifyBidField,
                                       Convert.ToString(OrderAnalyzer.GetPrice(typeToAdjust) + Outbid(typeToAdjust)));
                            modifiedOnLastIteration = true;
                            _orderReviewer.RemoveOrderRequiringReview(Character.Name, OrderAnalyzer.GetTypeId(),
                                                                      typeToAdjust);
                        }
                        else if (_orderReviewer.ShouldCancel(Character.Name, OrderAnalyzer.GetTypeId(), typeToAdjust))
                        {
                            cancelExistingOrder(cursorPosition);
                        }
                        else if (!OrderAnalyzer.IsBestOrderOwned(typeToAdjust) && !ShouldAdjustOrder(ref typeToAdjust))
                        {
                            double ownedPrice = OrderAnalyzer.GetOwnedPrice(typeToAdjust);
                            _orderReviewer.AddOrderRequiringReview(Character.StationId.ToString(), orderData,
                                                                   ownedPrice.ToString(), Character.Name,
                                                                   Modules.TypeNames[OrderAnalyzer.GetTypeId()]);
                        }
                        ++_numScanned;
                        cursorPosition[1] += UiElements.StandardRowHeight;
                        ConsecutiveFailures = 0;
                    }
                    catch (Exception e)
                    {
                        ++ConsecutiveFailures;
                        if (ConsecutiveFailures > 4)
                            return;
                        cursorPosition[1] += UiElements.StandardRowHeight;
                        Logger.Log(e.Message);
                        ErrorCheck();
                    }
                }
                if (i == (ceiling - 2))
                {
                    ErrorCheck();
                    Mouse.PointAndClick(Left, cursorPosition[0], cursorPosition[1] - UiElements.StandardRowHeight, 0, 40,
                                        20);
                    for (int l = 0; l < OrderAnalyzer.OrderSet.GetNumberOfBuysAndSells()[typeToAdjust]; ++l)
                        Keyboard.Send("{UP}");
                    Wait(20);
                    Mouse.PointAndClick(Left, sortByTypeCoords, 0, 20, 20);
                }
                else if (i < (ceiling - 1))
                {
                    ErrorCheck();
                    Mouse.PointAndClick(Left, cursorPosition[0], cursorPosition[1] - UiElements.StandardRowHeight, 0, 40,
                                        20);
                    for (int k = 0; k < UiElements.WalletVisibleRows[typeToAdjust]; ++k)
                        Keyboard.Send("{DOWN}");
                    Wait(20);
                }
            }
            if (LastOrderModified)
                ConfirmOrder(UiElements.OrderBoxConfirm, 1, typeToAdjust);
            Wait(20);
        }

        private void cancelExistingOrder(int[] rowCoords)
        {
            //identifyCancelWindow(rowCoords);
            //AlertConfirmButton();
        }

        private void IdentifyCancelWindow(int[] rowCoords)
        {
            for (int i = 0; i < 5; i++)
            {
                Mouse.PointAndClick(Right, rowCoords, 1, 1, 1);
                Mouse.OffsetAndClick(Left, UiElements.ContextMenuCancelOrderOffset, 1, 1, 1);
                Mouse.WaitDuration *= 2;
                if (GetError() == 13)
                {
                    Mouse.WaitDuration = Timing;
                    return;
                }
            }
            Mouse.WaitDuration = Timing;
            throw new Exception("Failed to identify cancellation window.");
        }

        private Boolean IsAnOverbid(int typeToAdjust)
        {
            if (typeToAdjust == EtConstants.Buy)
            {
                return OrderAnalyzer.GetOwnedBuyPrice() - OrderAnalyzer.GetBuyPrice() >= 1;
            }
            return OrderAnalyzer.GetSellPrice() - OrderAnalyzer.GetOwnedSellPrice() >= 1;
        }

        private static double Outbid(int typeToAdjust)
        {
            return typeToAdjust == EtConstants.Buy ? .01 : -.01;
        }

        private void OpenAndIdentifyModifyWindow(int tries, double timingScaleFactor, int[] coords, double price)
        {
            for (int i = 0; i < tries; i++)
            {
                Mouse.PointAndClick(Right, coords, 1, 1, 1);
                Mouse.OffsetAndClick(Left, UiElements.ContextMenuModifyOrderOffset, 1, 1, 1);
                Mouse.PointAndClick(Right, UiElements.ModifyBidField, 1, 1, 1);
                Mouse.OffsetAndClick(Left, UiElements.ContextMenuCopyOffset[0], UiElements.ContextMenuCopyOffset[1], 1,
                                     1, 1);
                try
                {
                    double result = Convert.ToDouble(Clipboard.GetTextFromClipboard());
                    if (result < price + 1000 && result > price - 1000)
                    {
                        Mouse.WaitDuration = Timing;
                        return;
                    }
                }
                catch
                {
                }
                Mouse.WaitDuration = Convert.ToInt32(Mouse.WaitDuration*timingScaleFactor);
            }
            Mouse.WaitDuration = Timing;
            throw new Exception("Could not find modification window.");
        }

        private bool ShouldAdjustOrder(ref int typeToAdjust)
        {
            if (typeToAdjust == EtConstants.Sell)
            {
                if (OrderAnalyzer.GetSellPrice() >
                    (OrderAnalyzer.GetBuyPrice() + (OrderAnalyzer.GetOwnedSellPrice() - OrderAnalyzer.GetBuyPrice())/2))
                    return true;
            }
            else
            {
                if (OrderAnalyzer.GetSellPrice() < 0 ||
                    OrderAnalyzer.GetBuyPrice() <
                    (OrderAnalyzer.GetSellPrice() - (OrderAnalyzer.GetSellPrice() - OrderAnalyzer.GetOwnedBuyPrice())/2))
                    return true;
            }
            if (OverrideShouldAdjust(typeToAdjust))
            {
                return true;
            }

            Logger.AutoListerLog("AL not adjusting " + Modules.TypeNames[Convert.ToInt32(OrderAnalyzer.GetTypeId())]);
            Logger.AutoListerLog("Best Sell: " + OrderAnalyzer.GetBuyPrice());
            Logger.AutoListerLog("Best Buy: " + OrderAnalyzer.GetSellPrice());
            if (typeToAdjust == EtConstants.Sell)
                Logger.AutoListerLog("Target sell order: " + OrderAnalyzer.GetOwnedSellPrice());
            else
                Logger.AutoListerLog("Target buy order: " + OrderAnalyzer.GetOwnedBuyPrice());
            return false;
        }

        private Boolean OverrideShouldAdjust(int typeToAdjust)
        {
            return (_orderReviewer.ShouldUpdate(Character.Name, OrderAnalyzer.GetTypeId(), typeToAdjust) &&
                    !SiginificantPriceChangeDetected(typeToAdjust));
        }

        private Boolean SiginificantPriceChangeDetected(int typeToAdjust)
        {
            double newPrice = OrderAnalyzer.GetPrice(typeToAdjust);
            double oldPrice = _orderReviewer.GetPrice(Character.Name, OrderAnalyzer.GetTypeId(), typeToAdjust);
            return Math.Abs((newPrice - oldPrice)/oldPrice) > .02;
        }

        private String ExecuteQueryAndExportResult(int tries, double timingScaleFactor, int[] cursorPosition,
                                                   ref int lastTypeId)
        {
            DirectoryEraser.Nuke(Paths.LogPath);
            if (_marketOrderio.GetNumberOfFilesInDirectory(Paths.LogPath) != 0)
                throw new Exception("Could not clean log path directory");
            int i;
            for (i = 0; i < tries; i++)
            {
                Mouse.PointAndClick(Double, cursorPosition, 0, 1, 0);
                Mouse.PointAndClick(Left, UiElements.MarketExportButton, 5, 1, 5);
                _fileName = _marketOrderio.GetNewestFileNameInDirectory(Paths.LogPath);
                try
                {
                    if (_fileName != null &&
                        Convert.ToInt32(_marketOrderio.ReadFirstEntryNoDelete(Paths.LogPath, _fileName)[2]) !=
                        lastTypeId)
                    {
                        Mouse.WaitDuration = Timing;
                        return _fileName;
                    }
                }
                catch (NullReferenceException e)
                {
                    Logger.Log(e.Message);
                }
                ErrorCheck();
                Mouse.WaitDuration = Convert.ToInt32(Mouse.WaitDuration*timingScaleFactor);
            }
            Logger.Log("Iterations ran out of tries @ " + i);
            Mouse.WaitDuration = Timing;
            throw new Exception("Could not export query result");
        }

        private void Prepare()
        {
            _freeOrders = 0;
            _numModified = 0;
            _numScanned = 0;
            _fileName = null;

            SetEveHandle(Character.Name);
            SetForegroundWindow(EveHandle);
            DirectoryEraser.Nuke(Paths.LogPath);
            ExportOrders(3, 30);

            Mouse.PointAndClick(Left, UiElements.WalletSellListSortByType, 40, 0, 0);
            Mouse.PointAndClick(Left, UiElements.WalletBuyListSortByType, 40, 0, 0);
            _freeOrders = Character.MaximumOrders - OrderAnalyzer.OrderSet.GetNumberOfActiveOrders();
        }
    }
}