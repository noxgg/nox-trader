using System;
using System.Collections.Generic;
using System.Windows.Forms;
using noxiousET.src.helpers;
using noxiousET.src.etevent;

//TODO no buy orders exist?
namespace noxiousET.src.orders
{
    class OrderAnalyzer
    {
        public OrderManager orderSet { set; get; }
        private string lastOrderTypeID;
        private EventDispatcher eventDispatcher;
        private bool bestSellOwned;
        private bool bestBuyOwned;
        private bool someBuyOwned;
        private bool someSellOwned;
        private double buyPrice;
        private double sellPrice;
        private double ownedBuyPrice;
        private double ownedSellPrice;
        private string typeid;

        public OrderAnalyzer()
        {
            lastOrderTypeID = "0";
            this.eventDispatcher = EventDispatcher.Instance;
            this.orderSet = new OrderManager();
        }

        public void analyzeInvestment(List<String[]> orderData, String typeid, String stationid)
        {
            analyze(ref orderData, ref typeid, ref stationid);
        }

        public void analyzeInvestment(List<String[]> orderData, String stationid)
        {
            analyze(ref orderData, ref orderData[0][2], ref stationid);
        }

        private void analyze(ref List<String[]> orderData, ref String typeid, ref String stationid)
        {
            buyPrice = 0;
            sellPrice = 0;
            ownedSellPrice = 0;
            ownedBuyPrice = 0;
            bestSellOwned = false;
            bestBuyOwned = false;
            int i = 0;
            int orderDataCount = orderData.Count;
            String[] order = i < orderDataCount ? orderData[i++] : null;
            this.typeid = order[2];
            if (typeid.CompareTo(this.typeid) != 0)
                throw new Exception("Wrong typeid");
            someSellOwned = orderSet.existsOrderOfType(Convert.ToInt32(typeid), 0);
            someBuyOwned = orderSet.existsOrderOfType(Convert.ToInt32(typeid), 1);
            //Find first order at target station
            while (order != null && order[7].CompareTo("False") == 0 && order[10].CompareTo(stationid) != 0)
                order = i < orderDataCount ? orderData[i++] : null;

            //If this order is a sell order and if it is competing at the target station
            if (order != null && order[7].CompareTo("False") == 0 && order[10].CompareTo(stationid) == 0)
            {
                //Is this an owned order?
                if (orderSet.isOrderOwned(order[4], 0))
                {
                    ownedSellPrice = Convert.ToDouble(order[0]);
                    bestSellOwned = true;
                }
                sellPrice = Convert.ToDouble(order[0]);
            }
            else
            {
                bestSellOwned = false;
                sellPrice = 0;
            }

            //If this is the best sell order, fetch the next nest competitive price
            if (bestSellOwned)
            {
                order = i < orderDataCount ? orderData[i++] : null;
                while (order != null && order[7].CompareTo("False") == 0 && order[10].CompareTo(stationid) != 0)
                    order = i < orderDataCount ? orderData[i++] : null;
                if (order != null && order[7].CompareTo("False") == 0 && order[10].CompareTo(stationid) == 0)
                    sellPrice = Convert.ToDouble(order[0]);
                else
                    sellPrice = 0;
            }


            //Jump over any remaining sell orders
            while (order != null && order[7].CompareTo("False") == 0)
            { 
                if (orderSet.isOrderOwned(order[4], 0))
                    ownedSellPrice = Convert.ToDouble(order[0]);
                order = i < orderDataCount ? orderData[i++] : null;
            }

            //Find first order that is competing at target station
            while (order != null && !isCompetingBuyOrder(Convert.ToInt32(order[3]), Convert.ToInt32(order[13]), order[10], stationid))
                order = i < orderDataCount ? orderData[i++] : null;

            if (order != null && order[7].CompareTo("True") == 0 && isCompetingBuyOrder(Convert.ToInt32(order[3]), Convert.ToInt32(order[13]), order[10], stationid))
            {
                //Is this an owned order?
                if (orderSet.isOrderOwned(order[4], 1))
                {
                    ownedBuyPrice = Convert.ToDouble(order[0]);
                    bestBuyOwned = true;
                }
                buyPrice = Convert.ToDouble(order[0]);
            }
            else
            {
                bestBuyOwned = false;
                buyPrice = 0;
            }

            //If this is the best sell order, fetch the next best price.
            if (bestBuyOwned)
            {
                order = i < orderDataCount ? orderData[i++] : null;
                while (order != null && !isCompetingBuyOrder(Convert.ToInt32(order[3]), Convert.ToInt32(order[13]), order[10], stationid))
                    order = i < orderDataCount ? orderData[i++] : null;
                if (order != null && order[7].CompareTo("True") == 0 && isCompetingBuyOrder(Convert.ToInt32(order[3]), Convert.ToInt32(order[13]), order[10], stationid))
                    buyPrice = Convert.ToDouble(order[0]);
                else
                    buyPrice = 0;
            }

            while (order != null && ownedBuyPrice == 0)
            {
                if (orderSet.isOrderOwned(order[4], 1))
                    ownedBuyPrice = Convert.ToDouble(order[0]);
                order = i < orderDataCount ? orderData[i++] : null;
            }

            if (sellPrice <= 0 && buyPrice <= 0) //If there are no orders to compare against, return
                return;
            else if (sellPrice <= 0)//If there are no active sell orders, set sell to 1.5x best buy
                sellPrice = buyPrice * 1.5;
            else if (buyPrice <= 0)//If there are no active buy orders, set buy tp 1/2 best sell
                buyPrice = sellPrice / 2;
            return;
        }



        private bool isCompetingBuyOrder(int range, int jumps, string stationid, string targetStationid)
        {
            if (range == -1 && stationid.CompareTo(targetStationid) == 0 || range - jumps >= 0)//If this is my station and the order has a station range
                return true;
            return false;
        }

        public int getTypeId()
        {
            return Convert.ToInt32(typeid);
        }

        public double getBuyPrice()
        {
            return buyPrice;
        }

        public double getSellPrice()
        {
            return sellPrice;
        }

        public double getPrice(int type)
        {
            if (type == EtConstants.BUY)
                return buyPrice;
            else
                return sellPrice;
        }

        public double getOwnedBuyPrice()
        {
            return ownedBuyPrice;
        }

        public double getOwnedSellPrice()
        {
            return ownedSellPrice;
        }

        public double getOwnedPrice(int type)
        {
            if (type == EtConstants.BUY)
                return ownedBuyPrice;
            else
                return ownedSellPrice;
        }

        public bool isSomeBuyOwned()
        {
            return someBuyOwned;
        }

        public bool isSomeSellOwned()
        {
            return someSellOwned;
        }

        public bool isBestBuyOwned()
        {
            return bestBuyOwned;
        }

        public bool isBestSellOwned()
        {
            return bestSellOwned;
        }

        public bool isBestOrderOwned(int type)
        {
            if (type == EtConstants.BUY)
                return bestBuyOwned;
            else
                return bestSellOwned;
        }

        public double findBestBuyAndSell(out double bestSellOrderPrice, out double bestBuyOrderPrice, out string itemName, out int typeID, string path, ref int terminalItemID, string stationID, int fileNameTrimLength, ref int offsetFlag)
        {
            itemName = "";
            bestBuyOrderPrice = bestSellOrderPrice = 0;
            typeID = 0;

            FileHandler file = new FileHandler(path);
            if (file.openNewestFile(path) == -1) return -1;
            string line;
            string[] parts = { "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" };
            string[] topSellOrder = { "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1" };
            string[] topBuyOrder = { "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1" };
            int[] numOfActiveOrders = orderSet.getNumberOfBuysAndSells();
            itemName = file.getItemName(fileNameTrimLength);
            if (itemName.CompareTo("[Parse Error]") == 0)//If we're still looking at the buy orders file, return as if it were a previous order.
            {
                file.close();
                return -2;
            }

            line = file.readLine(); //Skip header row

            line = file.readLine();
            parts = line.Split(',');

            if (Convert.ToInt32(parts[2]) == terminalItemID)//There are no more items to scan.
            {
                file.close();
                return -4;
            }

            typeID = Convert.ToInt32(parts[2]);
            //If we're still looking at the last buy order, do nothing. 
            if (String.Compare(lastOrderTypeID, parts[2]) == 0)
            {
                file.close();
                return -2;
            }

            //Now update last order to be the current order. 
            lastOrderTypeID = string.Copy(parts[2]);

            int activeOrderCheck = orderSet.checkForActiveOrders(Convert.ToInt32(parts[2]));

            if (activeOrderCheck == 3) //If there is no reason to do anything, because there are currently both sell and buy orders, do nothing.
            {
                file.close();
                return 3;
            }

            //Find the most competitive sell order.
            if (parts[7].CompareTo("False") == 0) //If there is at least one sell order
            {
                if (stationID.CompareTo(parts[10]) == 0) //If this order is competing at my station
                {
                    topSellOrder = line.Split(',');
                }
                else
                {
                    while (parts[7].CompareTo("False") == 0 && (line = file.readLine()) != null)
                    {
                        parts = line.Split(',');

                        if (parts[7].CompareTo("False") == 0 && stationID.CompareTo(parts[10]) == 0 && topSellOrder[0].CompareTo("-1") == 0) //If this order is competing at my station
                        {
                            topSellOrder = line.Split(',');
                            break;
                        }
                    }
                }
            }
            //Find the most competitive buy order.
            while (parts[7].CompareTo("False") == 0 && (line = file.readLine()) != null)
            {
                parts = line.Split(','); //Iterate past all sell orders.
            }
            if (line != null && isCompetingBuyOrder(Convert.ToInt32(parts[3]), Convert.ToInt32(parts[13]), parts[10], stationID)) //If this order is competing at my station
            {
                topBuyOrder = line.Split(',');//null if there are no buy orders
            }
            else
            {
                while ((line = file.readLine()) != null)
                {
                    parts = line.Split(',');

                    if (isCompetingBuyOrder(Convert.ToInt32(parts[3]), Convert.ToInt32(parts[13]), parts[10], stationID)) //If this order is competing at my station
                    {
                        topBuyOrder = line.Split(',');
                        break;
                    }
                }
            }

            file.close();

            bestSellOrderPrice = Convert.ToDouble(topSellOrder[0]);
            bestBuyOrderPrice = Convert.ToDouble(topBuyOrder[0]);

            if (bestSellOrderPrice <= 0 && bestBuyOrderPrice <= 0) //If there are no orders to compare against, return code 4.
            {
                return 4;
            } 
            if (bestSellOrderPrice <= 0)//If there are no active sell orders, sell for twice the best buy order.
            {
                offsetFlag = 1;
                bestSellOrderPrice = bestBuyOrderPrice * 1.5;
            }
            if (bestBuyOrderPrice <= 0)//If there are no active buy orders, buy for half the best sell order.
            {
                bestBuyOrderPrice = bestSellOrderPrice / 2;
            }

            return activeOrderCheck;
        }

        public int clearLastBuyOrder()
        {
            lastOrderTypeID = "0";
            return 0;
        }
    };
}
