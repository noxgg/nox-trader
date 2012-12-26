using System;
using noxiousET.src.data.characters;
using noxiousET.src.data.client;
using noxiousET.src.data.modules;
using noxiousET.src.data.paths;
using noxiousET.src.data.uidata;
using noxiousET.src.orders;

namespace noxiousET.src.guiInteraction.orders
{
    internal class OrderBot : GuiBot
    {
        protected Modules Modules;

        private const int BuyWindowOpenTries = 3;
        private const int CancelOrderTries = 9;
        private const int ValueIndex = 0;
        private const int QuantityIndex = 1;
        private const string EveDefaultBuyQuantity = "1";

        public OrderBot(ClientConfig clientConfig, EveUi eveUi, Paths paths, Character character,
                        Modules modules, OrderAnalyzer orderAnalyzer)
            : base(clientConfig, eveUi, paths, character, orderAnalyzer)
        {
            Modules = modules;
        }

        //TODO, merge with confirmOrder
        protected int CancelOrder(int[] offset)
        {
            return CancelOrder(offset[EtConstants.X], offset[EtConstants.Y]);
        }

        protected int CancelOrder(int xOffset, int yOffset)
        {
            int failCount = 0;
            string temp;
            do
            {
                Mouse.PointAndClick(Left, EveUi.OrderBoxCancel[EtConstants.X], EveUi.OrderBoxCancel[EtConstants.Y] + yOffset, 1, 1, 1);

                if (failCount > 0 && failCount % 3 == 0)
                    ErrorCheck();

                //Right click where Cancel should no longer exist.
                Mouse.PointAndClick(Double, EveUi.OrderBoxCancel[EtConstants.X] + xOffset, EveUi.OrderBoxCancel[EtConstants.Y], 0, 1, 1);

                Clipboard.SetClip(EtConstants.ClipboardNullValue);
                Keyboard.Shortcut(new[] {Keyboard.VkLcontrol}, Keyboard.VkC);
                temp = Clipboard.GetTextFromClipboard();

                ++failCount;
            } while (!temp.Contains(EtConstants.OrderWindowClosedVerificationSubstring) && failCount < CancelOrderTries);

            if (!temp.Equals(EtConstants.ClipboardNullValue))
            {
                Clipboard.SetClip(EtConstants.ClipboardNullValue);
                return 0;
            }
            return 1;
        }

        protected int CloseMarketAndHangarWindows()
        {
            Mouse.PointAndClick(Left, EveUi.MarketCloseButton, 0, 5, 5);
            Mouse.PointAndClick(Left, EveUi.HangarCloseButton, 0, 5, 0);
            return 0;
        }

        protected int[] GenerateSellWindowCoords(int typeId, int[] coords)
        {
            return GenerateAdjustedWindowCoords(typeId, coords);
        }

        protected int[] GenerateBuyWindowCoords(int typeId, int[] coords)
        {
            return GenerateAdjustedWindowCoords(typeId, coords);
        }

        private int[] GenerateAdjustedWindowCoords(int typeId, int[] coords)
        {
            if (Modules.LongNameTypeIDs.ContainsKey(typeId))
                return new int[2] { coords[EtConstants.X], coords[EtConstants.Y] + 22 };
            return coords;
        }

        protected void OpenAndIdentifyBuyWindow(int currentItem, double sellPrice)
        {
            for (int i = 0; i < BuyWindowOpenTries; i++)
            {
                CancelOrder(0, 0);
                Mouse.PointAndClick(Left, EveUi.MarketWindowDeadspace, 0,0,0);
                Mouse.PointAndClick(Left, EveUi.MarketPlaceBuyButton, 5, 2, 50);
                Mouse.PointAndClick(Double, GenerateBuyWindowCoords(currentItem, EveUi.BuyBidPriceField), 2, 4, 2);
                Keyboard.Shortcut(new[] { Keyboard.VkLcontrol }, Keyboard.VkC);
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
                /*if (i == BuyWindowOpenTries - 1)
                {
                    //TODO fix magic numbers.
                    CancelOrder(-53, 51);
                }*/
            }
            Logger.Log("Failed to open and identify the buy window!");
            Mouse.WaitDuration = Timing;
            throw new Exception("Could not open buy window");
        }

        protected void PlaceBuyOrder(int typeId, int quantity)
        {
            int[] quantityCoords = {EveUi.BuyQuantityField[0], EveUi.BuyQuantityField[1] + 5};
            Double verificationValue =
                Math.Min(OrderAnalyzer.GetSellPrice(), OrderAnalyzer.GetOwnedSellPrice()).Equals(0)
                    ? Math.Max(OrderAnalyzer.GetSellPrice(), OrderAnalyzer.GetOwnedSellPrice())
                    : Math.Min(OrderAnalyzer.GetSellPrice(), OrderAnalyzer.GetOwnedSellPrice());
            if (OrderAnalyzer.NoSellsExist)
                quantityCoords[1] -= 15;

            OpenAndIdentifyBuyWindow(typeId, verificationValue);
            //Input price
            InputValue(3, 1.4, GenerateBuyWindowCoords(typeId, EveUi.BuyBidPriceField),
                       Convert.ToString(OrderAnalyzer.GetBuyPrice() + .01));
            //Input quantity
            VerifyFieldContains(GenerateBuyWindowCoords(typeId, quantityCoords), EveDefaultBuyQuantity);
            InputValue(3, 1.4, GenerateBuyWindowCoords(typeId, quantityCoords), Convert.ToString(quantity));
            ConfirmOrder(GenerateBuyWindowCoords(typeId, EveUi.OrderBoxConfirm), 1, EtConstants.IsBuyOrder);
        }

        protected int GetBuyOrderQuantity(double bestBuyOrderPrice, double bestSellOrderPrice)
        {
            if (bestSellOrderPrice/bestBuyOrderPrice < EtVariables.MinimumProfitMargin) //TODO: FACTOR OUT HARDCODED VALUE
            {
                return -1;
            }

            int i;
            for (i = 0; i < Character.QuantityThreshHolds.Count; ++i)
            {
                if (bestBuyOrderPrice < Character.QuantityThreshHolds[i][ValueIndex])
                    return Character.QuantityThreshHolds[i][1];
            }

            return Character.QuantityThreshHolds[i - 1][QuantityIndex];
        }
    }
}