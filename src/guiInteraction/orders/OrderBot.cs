using System;
using noxiousET.src.data.characters;
using noxiousET.src.data.client;
using noxiousET.src.data.modules;
using noxiousET.src.data.paths;
using noxiousET.src.data.uielements;
using noxiousET.src.orders;

namespace noxiousET.src.guiInteraction.orders
{
    internal class OrderBot : GuiBot
    {
        protected int ConsecutiveFailures;
        protected Modules Modules;

        public OrderBot(ClientConfig clientConfig, UiElements uiElements, Paths paths, Character character,
                        Modules modules, OrderAnalyzer orderAnalyzer)
            : base(clientConfig, uiElements, paths, character, orderAnalyzer)
        {
            Modules = modules;
            ConsecutiveFailures = 0;
        }

        //TODO, merge with confirmOrder
        protected int CancelOrder(int[] offset)
        {
            return CancelOrder(offset[0], offset[1]);
        }

        protected int CancelOrder(int xOffset, int yOffset)
        {
            int failCount = 0;
            string temp;
            do
            {
                Mouse.PointAndClick(Left, UiElements.OrderBoxCancel[0], UiElements.OrderBoxCancel[1] + yOffset, 1, 1, 1);

                if (failCount > 0 && failCount%3 == 0)
                    ErrorCheck();

                //Right click where OK should no longer exist.
                Mouse.PointAndClick(Right, UiElements.OrderBoxCancel[0] + xOffset, UiElements.OrderBoxCancel[1], 0, 1, 1);
                //Click on copy
                Mouse.OffsetAndClick(Left, UiElements.ChatCopyOffset, 0, 1, 1);

                temp = Clipboard.GetTextFromClipboard();
                ++failCount;
            } while (temp.Equals("0") && failCount < 9);

            LastOrderModified = false;

            if (!temp.Equals("0"))
            {
                Clipboard.SetClip("0");
                return 0;
            }
            return 1;
        }

        protected int CloseMarketAndHangarWindows()
        {
            Mouse.PointAndClick(Left, UiElements.MarketCloseButton, 0, 5, 5);
            Mouse.PointAndClick(Left, UiElements.HangarCloseButton, 0, 5, 0);
            return 0;
        }

        protected int[] FixCoordsForLongTypeName(int typeId, int[] coords)
        {
            if (Modules.LongNameTypeIDs.ContainsKey(typeId))
                return new int[2] {coords[0], coords[1] + 22};
            return coords;
        }

        protected void OpenAndIdentifyBuyWindow(int currentItem, double sellPrice)
        {
            for (int i = 0; i < 5; i++)
            {
                CancelOrder(0, 0);
                Mouse.PointAndClick(Left, UiElements.MarketPlaceBuyButton, 5, 2, 15);
                Mouse.PointAndClick(Right, FixCoordsForLongTypeName(currentItem, UiElements.BuyBidPriceField), 2, 4, 2);
                Mouse.OffsetAndClick(Left, UiElements.ContextMenuCopyOffset, 2, 2, 2);
                Mouse.OffsetAndClick(Left, 428, 419, 0, 0, 0);
                try
                {
                    double result = Convert.ToDouble(Clipboard.GetTextFromClipboard());
                    if (result < sellPrice + 1000 && result > sellPrice - 1000)
                    {
                        Mouse.WaitDuration = Timing;
                        return;
                    }
                    Mouse.WaitDuration *= 2;
                }
                catch
                {
                    Mouse.WaitDuration *= 2;
                }
                if (i == 6)
                {
                    CancelOrder(-53, 51);
                }
            }
            Logger.Log("Failed to open and identify the buy window!");
            Mouse.WaitDuration = Timing;
            throw new Exception("Could not open buy window");
        }

        protected void PlaceBuyOrder(int typeId, int quantity)
        {
            int[] quantityCoords = {UiElements.BuyQuantityField[0], UiElements.BuyQuantityField[1] + 5};
            Double verificationValue =
                Math.Min(OrderAnalyzer.GetSellPrice(), OrderAnalyzer.GetOwnedSellPrice()).Equals(0)
                    ? Math.Max(OrderAnalyzer.GetSellPrice(), OrderAnalyzer.GetOwnedSellPrice())
                    : Math.Min(OrderAnalyzer.GetSellPrice(), OrderAnalyzer.GetOwnedSellPrice());
            if (OrderAnalyzer.NoSellsExist)
                quantityCoords[1] -= 15;

            OpenAndIdentifyBuyWindow(typeId, verificationValue);
            //Input price
            InputValue(5, 1.4, FixCoordsForLongTypeName(typeId, UiElements.BuyBidPriceField),
                       Convert.ToString(OrderAnalyzer.GetBuyPrice() + .01));
            //Input quantity
            InputValue(3, 2, FixCoordsForLongTypeName(typeId, quantityCoords), Convert.ToString(quantity));
            ConfirmOrder(FixCoordsForLongTypeName(typeId, UiElements.OrderBoxConfirm), 1, 1);
        }

        protected int GetBuyOrderQuantity(double bestBuyOrderPrice, double bestSellOrderPrice)
        {
            if (bestSellOrderPrice/bestBuyOrderPrice < 1.0736) //TODO: FACTOR OUT HARDCODED VALUE
            {
                return -1;
            }

            int i;
            for (i = 0; i < Character.QuantityThreshHolds.Count; ++i)
            {
                if (bestBuyOrderPrice < Character.QuantityThreshHolds[i][0])
                    return Character.QuantityThreshHolds[i][1];
            }

            return Character.QuantityThreshHolds[i - 1][1];
        }
    }
}