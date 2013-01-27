using System;
using System.Collections.Generic;
using System.Diagnostics;
using noxiousET.src.data.characters;
using noxiousET.src.data.client;
using noxiousET.src.data.io;
using noxiousET.src.data.modules;
using noxiousET.src.data.paths;
using noxiousET.src.data.uidata;
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
        private const int AdjusterFailureLimit = 4;

        public AutoAdjuster(ClientConfig clientConfig, EveUi eveUi, Paths paths, Character character,
                            Modules modules, OrderAnalyzer orderAnalyzer, OrderReviewer orderReviewer)
            : base(clientConfig, eveUi, paths, character, modules, orderAnalyzer)
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
                Adjust(true, EveUi.WalletBuyListFirstRow, EveUi.WalletBuyListSortByType, EveUi.WalletVisibleRows[EtConstants.Buy]);
            if (character.ShouldAdjustSells)
                Adjust(false, EveUi.WalletSellListFirstRow, EveUi.WalletSellListSortByType, EveUi.WalletVisibleRows[EtConstants.Sell]);
            
            stopwatch.Stop();
            Logger.Log(character.Name + ": AA scanned " + _numScanned + ", adjusted " + _numModified + " in " +
                       stopwatch.Elapsed.ToString());
        }

        private void Adjust(bool isBuyOrder, int[] firstRowCoords, int[] sortByTypeCoords, int rowsPerPage)
        {
            int consecutiveFailures = 0;
            int rowsToCheck = OrderAnalyzer.OrderSet.GetNumberOfOrders(isBuyOrder);
            int currentTypeId = 0;
            bool orderRequiresConfirmation = false;

            int numberOfPages = Convert.ToInt32(Math.Ceiling(rowsToCheck / Convert.ToDouble(rowsPerPage)));
            var currentRowCoords = new int[2];
            currentRowCoords[EtConstants.X] = firstRowCoords[EtConstants.X];

            for (int i = 0; i < numberOfPages; ++i)
            {
                currentRowCoords[EtConstants.Y] = firstRowCoords[EtConstants.Y];

                //We don't need to check every row on the last page.
                if (i.Equals(numberOfPages - 1))
                {
                    rowsPerPage = rowsToCheck % rowsPerPage;
                }

                for (int j = 0; j < rowsPerPage; j++)
                {
                    try
                    {
                        if (EveHandle != GetForegroundWindow())
                            SetForegroundWindow(EveHandle);

                        ExecuteQueryAndExportResult(currentRowCoords, currentTypeId, isBuyOrder);
                        currentTypeId = OrderAnalyzer.GetTypeId();

                        //Confirm orders after executing the next query to avoid lag caused by the market window re-querying market data
                        //after confirming an order for an item that is currently being viewed.
                        if (orderRequiresConfirmation)
                        {
                            ConfirmOrder(EveUi.OrderBoxConfirm, 1, isBuyOrder);
                            orderRequiresConfirmation = false;
                            ++_numModified;
                        }

                        if (ShouldUpdate(isBuyOrder))
                        {
                            UpdateOrder(currentRowCoords, isBuyOrder);
                            orderRequiresConfirmation = true;
                        }
                        else if (_orderReviewer.ShouldCancel(Character.Name, OrderAnalyzer.GetTypeId(), isBuyOrder))
                        {
                            cancelExistingOrder(currentRowCoords);
                        }

                        ++_numScanned;
                        consecutiveFailures = 0;
                    }
                    catch (Exception e)
                    {
                        ++consecutiveFailures;
                        Logger.Log(e.Message);
                        ErrorCheck();

                        if (consecutiveFailures > AdjusterFailureLimit)
                            return;
                    }
                    currentRowCoords[EtConstants.Y] += EveUi.StandardRowHeight;
                }
                currentRowCoords[EtConstants.Y] -= EveUi.StandardRowHeight;
                //Scroll back to top and flip ordering to hit last few items if at the end if we're at the second to last page.
                if (i == (numberOfPages - 2))
                {
                    ErrorCheck();
                    Mouse.PointAndClick(Left, currentRowCoords, 0, 40,
                                        20);
                    for (int l = 0; l < rowsToCheck; ++l)
                        Keyboard.Send("{UP}");
                    Wait(20);
                    Mouse.PointAndClick(Left, sortByTypeCoords, 0, 20, 20);
                }
                //Otherwise move to the next page if there are more pages.
                else if (i < (numberOfPages - 2))
                {
                    ErrorCheck();
                    Mouse.PointAndClick(Left, currentRowCoords, 0, 40,
                                        20);
                    for (int k = 0; k < rowsPerPage; ++k)
                        Keyboard.Send("{DOWN}");
                    Wait(20);
                }
            }
            if (orderRequiresConfirmation)
                ConfirmOrder(EveUi.OrderBoxConfirm, 1, isBuyOrder);
            Wait(20);
        }

        private bool ShouldUpdate(bool isBuyOrder)
        {
            return !OrderAnalyzer.IsBestOrderOwned(isBuyOrder) && ShouldAdjustOrder(isBuyOrder) ||
                   OrderAnalyzer.IsBestOrderOwned(isBuyOrder) && IsAnOverbid(isBuyOrder);
        }

        private void UpdateOrder(int[] rowCoordinates, bool isBuyOrder)
        {
            OpenAndIdentifyModifyWindow(10, 1.2, rowCoordinates, OrderAnalyzer.GetOwnedPrice(isBuyOrder));
            InputValue(5, 1.4, EveUi.ModifyBidField, Convert.ToString(OrderAnalyzer.GetPrice(isBuyOrder) + Outbid(isBuyOrder)));
            _orderReviewer.RemoveOrderRequiringReview(Character.Name, OrderAnalyzer.GetTypeId(), isBuyOrder);
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
                Mouse.OffsetAndClick(Left, EveUi.ContextMenuCancelOrderOffset, 1, 1, 1);
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

        private Boolean IsAnOverbid(bool isBuyOrder)
        {
            if (isBuyOrder)
            {
                return OrderAnalyzer.GetOwnedBuyPrice() - OrderAnalyzer.GetBuyPrice() >= 1;
            }
            return OrderAnalyzer.GetSellPrice() - OrderAnalyzer.GetOwnedSellPrice() >= 1;
        }

        private static double Outbid(bool isBuyOrder)
        {
            return isBuyOrder ? .01 : -.01;
        }

        private void OpenAndIdentifyModifyWindow(int tries, double timingScaleFactor, int[] coords, double fieldShouldContain)
        {
            Clipboard.SetClip(EtConstants.ClipboardNullValue);
            for (int i = 0; i < tries; i++)
            {
                Mouse.PointAndClick(Right, coords, 1, 1, 1);
                Mouse.OffsetAndClick(Left, EveUi.ContextMenuModifyOrderOffset, 1, 1, 1);
                Mouse.PointAndClick(Double, EveUi.ModifyBidField, 1, 1, 1);
                Keyboard.Shortcut(new[] { Keyboard.VkLcontrol }, Keyboard.VkC);
                Clipboard.GetTextFromClipboard();
                try
                {
                    double result = Convert.ToDouble(Clipboard.GetTextFromClipboard());
                    if (result < fieldShouldContain + 10 && result > fieldShouldContain - 10)
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

        private bool ShouldAdjustOrder(bool isBuyOrder)
        {
            if (isBuyOrder)
            {
                if (OrderAnalyzer.GetSellPrice() < 0 ||
                    OrderAnalyzer.GetBuyPrice() <
                    (OrderAnalyzer.GetSellPrice() - (OrderAnalyzer.GetSellPrice() - OrderAnalyzer.GetOwnedBuyPrice())/2))
                    return true;
            }
            else
            {
                if (OrderAnalyzer.GetSellPrice() >
                    (OrderAnalyzer.GetBuyPrice() + (OrderAnalyzer.GetOwnedSellPrice() - OrderAnalyzer.GetBuyPrice())/2))
                    return true;
            }
            if (OverrideShouldAdjust(isBuyOrder))
            {
                return true;
            }

            Logger.AutoListerLog("AL not adjusting " + Modules.TypeNames[Convert.ToInt32(OrderAnalyzer.GetTypeId())]);
            Logger.AutoListerLog("Best Sell: " + OrderAnalyzer.GetBuyPrice());
            Logger.AutoListerLog("Best Buy: " + OrderAnalyzer.GetSellPrice());
            if (isBuyOrder)
                Logger.AutoListerLog("Target buy order: " + OrderAnalyzer.GetOwnedBuyPrice());
            else
                Logger.AutoListerLog("Target sell order: " + OrderAnalyzer.GetOwnedSellPrice());
            return false;
        }

        private Boolean OverrideShouldAdjust(bool isBuyOrder)
        {
            return (_orderReviewer.ShouldUpdate(Character.Name, OrderAnalyzer.GetTypeId(), isBuyOrder) &&
                    !SiginificantPriceChangeDetected(isBuyOrder));
        }

        private Boolean SiginificantPriceChangeDetected(bool isBuyOrder)
        {
            double newPrice = OrderAnalyzer.GetPrice(isBuyOrder);
            double oldPrice = _orderReviewer.GetPrice(Character.Name, OrderAnalyzer.GetTypeId(), isBuyOrder);
            return Math.Abs((newPrice - oldPrice)/oldPrice) > .02;
        }

        private void ExecuteQueryAndExportResult(int[] cursorPosition, int lastTypeId, bool isBuyOrder)
        {
            _marketOrderio.FileName = ExecuteQueryAndExportResult(3, 1.5, cursorPosition, ref lastTypeId);
            List<String[]> orderData = _marketOrderio.Read();
            OrderAnalyzer.AnalyzeInvestment(orderData, Convert.ToString(Character.StationId));

            if (!OrderAnalyzer.IsBestOrderOwned(isBuyOrder) && !ShouldAdjustOrder(isBuyOrder))
            {
                double ownedPrice = OrderAnalyzer.GetOwnedPrice(isBuyOrder);
                _orderReviewer.AddOrderRequiringReview(Character.StationId.ToString(), orderData,
                                                       ownedPrice.ToString(), Character.Name,
                                                       Modules.TypeNames[OrderAnalyzer.GetTypeId()]);
            }

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
                Mouse.PointAndClick(Left, EveUi.MarketExportButton, 7, 1, 5);
                _fileName = _marketOrderio.GetNewestFileNameInDirectory(Paths.LogPath);
                try
                {
                    if (_fileName != null &&
                        Convert.ToInt32(_marketOrderio.ReadFirstEntryNoDelete(Paths.LogPath, _fileName)[EtConstants.OrderDataColumnTypeId]) !=
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
                if (i % 2 == 1)
                {
                    ErrorCheck();
                }
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

            Mouse.PointAndClick(Left, EveUi.WalletSellListSortByType, 40, 0, 0);
            Mouse.PointAndClick(Left, EveUi.WalletBuyListSortByType, 40, 0, 0);
            _freeOrders = Character.MaximumOrders - OrderAnalyzer.OrderSet.GetNumberOfActiveOrders();
        }
    }
}