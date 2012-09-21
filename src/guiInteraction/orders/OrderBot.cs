using System;
using noxiousET.src.etevent;
using noxiousET.src.data.characters;
using noxiousET.src.data.client;
using noxiousET.src.data.modules;
using noxiousET.src.data.paths;
using noxiousET.src.data.uielements;
using noxiousET.src.orders;

namespace noxiousET.src.guiInteraction.orders
{
    class OrderBot : GuiBot
    {
        protected int consecutiveFailures;
        protected Modules modules;

        public OrderBot(ClientConfig clientConfig, UiElements uiElements, Paths paths, Character character, Modules modules, OrderAnalyzer orderAnalyzer)
            : base(clientConfig, uiElements, paths, character, orderAnalyzer)
        {
            this.modules = modules;
            consecutiveFailures = 0;
        }

        //TODO, merge with confirmOrder
        protected int cancelOrder(int[] offset)
        {
            return cancelOrder(offset[0], offset[1]);
        }
        protected int cancelOrder(int xOffset, int yOffset)
        {
            int failCount = 0;
            string temp = "0";
            do
            {
                mouse.pointAndClick(LEFT, uiElements.OrderBoxCancel[0], uiElements.OrderBoxCancel[1] + yOffset, 1, 1, 1);

                if (failCount > 0 && failCount % 3 == 0)
                        errorCheck();

                //Right click where OK should no longer exist.
                mouse.pointAndClick(RIGHT, uiElements.OrderBoxCancel[0] + xOffset, uiElements.OrderBoxCancel[1], 0, 1, 1);
                //Click on copy
                mouse.offsetAndClick(LEFT, uiElements.confirmationCopyOffset, 0, 1, 1);

                temp = Clipboard.getTextFromClipboard();
                ++failCount;
            } while (string.Compare(temp, "0") == 0 && failCount < 9);
            lastOrderModified = false;
            if (string.Compare(temp, "0") != 0)
            {
                Clipboard.setClip("0");
                return 0;
            }
            else
                return 1;
        }

        protected int closeMarketAndHangarWindows()
        {
            mouse.pointAndClick(LEFT, uiElements.closeMarketWindow, 0, 5, 5);
            mouse.pointAndClick(LEFT, uiElements.closeItems, 0, 5, 0);
            return 0;
        }

        protected int[] fixCoordsForLongTypeName(int typeId, int[] coords)
        {
            if (modules.longNameTypeIDs.ContainsKey(typeId))
                return new int[2] { coords[0], coords[1] + 22 };
            return coords;
        }

        protected void openAndIdentifyBuyWindow(int currentItem, double sellPrice)
        {
            double result;

            for (int i = 0; i < 7; i++)
            {
                cancelOrder(0,0);
                mouse.pointAndClick(LEFT, uiElements.placeBuyOrder, 5, 2, 15);
                mouse.pointAndClick(RIGHT, fixCoordsForLongTypeName(currentItem, uiElements.buyOrderBox), 2, 4, 2);
                mouse.offsetAndClick(LEFT, uiElements.copyOffset, 2, 2, 2);
                mouse.offsetAndClick(LEFT, 428, 419, 0, 0, 0);
                try
                {
                    result = Convert.ToDouble(Clipboard.getTextFromClipboard());
                    if (result < sellPrice + 1000 && result > sellPrice - 1000)
                    {
                        mouse.waitDuration = timing;
                        return;
                    }
                    mouse.waitDuration *= 2;
                    result = 0;
                }
                catch
                {
                    mouse.waitDuration *= 2;
                    result = 0;
                }
                if (i == 6)
                {
                    cancelOrder(-53, 51);
                }
            }
            mouse.waitDuration = timing;
            throw new Exception("Could not open buy window");

        }

        protected void placeBuyOrder(int typeId, int quantity)
        {
            int[] quantityCoords = { uiElements.buyOrderQtyBox[0], uiElements.buyOrderQtyBox[1] };
            Double verificationValue = Math.Min(orderAnalyzer.getSellPrice(), orderAnalyzer.getOwnedSellPrice()).Equals(0) ? Math.Max(orderAnalyzer.getSellPrice(), orderAnalyzer.getOwnedSellPrice()) : Math.Min(orderAnalyzer.getSellPrice(), orderAnalyzer.getOwnedSellPrice());
            if (orderAnalyzer.noSellsExist)
                quantityCoords[1] -= 15;

            openAndIdentifyBuyWindow(typeId, verificationValue);
            //Input price
            inputValue(5, 2, fixCoordsForLongTypeName(typeId, uiElements.buyOrderBox), Convert.ToString(orderAnalyzer.getBuyPrice() + .01));
            //Input quantity
            inputValue(3, 2, fixCoordsForLongTypeName(typeId, quantityCoords), Convert.ToString(quantity));
            confirmOrder(fixCoordsForLongTypeName(typeId, uiElements.OrderBoxOK), 1, 1);
        }

        protected int getBuyOrderQty(double bestBuyOrderPrice, double bestSellOrderPrice)
        {
            int i;
            if (bestSellOrderPrice / bestBuyOrderPrice < 1.0736) //TODO: FACTOR OUT HARDCODED VALUE
            {
                return -1;
            }
            else
            {
                for (i = 0; i < character.quantityThreshHolds.Count; ++i)
                {
                    if (bestBuyOrderPrice < character.quantityThreshHolds[i][0])
                        return character.quantityThreshHolds[i][1];
                }
            }
            return character.quantityThreshHolds[i - 1][1];
        }
    }
}
