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
        private int buyQuantity;

        public OrderAnalyzer(EventDispatcher eventDispatcher)
        {
            lastOrderTypeID = "0";
            this.eventDispatcher = eventDispatcher;
        }

        public void analyzeInvestment(List<String[]> orderData, String typeid, String stationid)
        {
            analyze(orderData, typeid, stationid);
        }

        private void analyze(List<String[]> orderData, String typeid, String stationid)
        {
            int i = 0;
            int orderDataCount = orderData.Count;
            String[] order = i < orderDataCount ? orderData[i++] : null;
            if (typeid.CompareTo(order[2]) != 0)
            {
                throw new Exception("Wrong typeid");
            }
            someSellOwned = orderSet.existsOrderOfType(Convert.ToInt32(typeid), 0);
            someBuyOwned = orderSet.existsOrderOfType(Convert.ToInt32(typeid), 1);
            //Find first order at target station
            while (order != null && order[7].CompareTo("False") == 0 && order[10].CompareTo(stationid) != 0)
                order = i < orderDataCount ? orderData[i++] : null;

            //If this order is a sell order and if it is competing at the target station
            if (order != null && order[7].CompareTo("False") == 0 && order[10].CompareTo(stationid) == 0)
            {
                //Is this an owned order?
                bestSellOwned = orderSet.isOrderOwned(order[4], 0);

                sellPrice = Convert.ToDouble(order[0]);
            }
            else
            {
                bestSellOwned = false;
                sellPrice = 0;
            }

            //Jump over any remaining sell orders
            while (order != null && order[7].CompareTo("False") == 0)
                order = i < orderDataCount ? orderData[i++] : null;

            //Find first order that is competing at target station
            while (order != null && !isCompetingBuyOrder(Convert.ToInt32(order[3]), Convert.ToInt32(order[13]), order[10], stationid))
                order = i < orderDataCount ? orderData[i++] : null;

            if (order != null && order[7].CompareTo("True") == 0 && isCompetingBuyOrder(Convert.ToInt32(order[3]), Convert.ToInt32(order[13]), order[10], stationid))
            {
                //Is this an owned order?
                bestBuyOwned = orderSet.isOrderOwned(order[4], 1);

                buyPrice = Convert.ToDouble(order[0]);
            }
            else
            {
                bestBuyOwned = false;
                buyPrice = 0;
            }

            if (sellPrice <= 0 && buyPrice <= 0) //If there are no orders to compare against, return code 4.
                return;
            else if (sellPrice <= 0)//If there are no active sell orders, sell for twice the best buy order.
                sellPrice = buyPrice * 1.5;
            else if (buyPrice <= 0)//If there are no active buy orders, buy for half the best sell order.
                buyPrice = sellPrice / 2;
            return;
        }



        private bool isCompetingBuyOrder(int range, int jumps, string stationid, string targetStationid)
        {
            if (range == -1 && stationid.CompareTo(targetStationid) == 0 || range - jumps >= 0)//If this is my station and the order has a station range
                return true;
            return false;
        }

        public double getBuyPrice()
        {
            return buyPrice;
        }

        public int getBuyQuantity()
        {
            return buyQuantity;
        }

        public double getSellPrice()
        {
            return sellPrice;
        }

        public bool isSomeBuyOwned()
        {
            return someBuyOwned;
        }

        public bool isSomeSellOwned()
        {
            return someSellOwned;
        }

        public double findBestBuyAndSell(ref OrderManager orders, out double bestSellOrderPrice, out double bestBuyOrderPrice, out string itemName, out int typeID, string path, ref int terminalItemID, string stationID, int fileNameTrimLength, ref int offsetFlag)
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
            int[] numOfActiveOrders = orders.getNumberOfActiveBuysAndActiveSells();
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

            int activeOrderCheck = orders.checkForActiveOrders(ref parts[2]);

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
            else if (bestSellOrderPrice <= 0)//If there are no active sell orders, sell for twice the best buy order.
            {
                offsetFlag = 1;
                bestSellOrderPrice = bestBuyOrderPrice * 1.5;
            }
            else if (bestBuyOrderPrice <= 0)//If there are no active buy orders, buy for half the best sell order.
            {
                bestBuyOrderPrice = bestSellOrderPrice / 2;
            }

            return activeOrderCheck;
        }

        public double getNewPriceForOrder(ref OrderManager orders, ref int orderType, int run, ref double orderPrice, string path, ref int typeID, int fileNameTrimLength)
        {
            FileHandler file = new FileHandler(path);
            if (file.openNewestFile(path) == -1) return -1;

            string line;
            string[] parts = { "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" };

            string itemName = file.getItemName(fileNameTrimLength);

            string[] topSellOrder = { "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1" };
            string[] topBuyOrder = { "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1" };

            line = file.readLine(); //Skip header row

            line = file.readLine();
            parts = line.Split(',');
            int listPosition = -1;
            
            typeID = Convert.ToInt32(parts[2]);
            string myOrderID = orders.getOrderIDandListPosition(ref parts[2], ref orderType, out listPosition);
            if (listPosition == -1 || String.Compare(lastOrderTypeID, Convert.ToString(orders.getOrderTypeID(ref listPosition, ref orderType))) == 0)
            {
                file.close();
                return -2;
            }

            if (String.Compare(lastOrderTypeID, Convert.ToString(orders.getOrderTypeID(ref listPosition, ref orderType))) == 0)
            {
                file.close();
                return -2;
            }

            lastOrderTypeID = string.Copy(Convert.ToString(orders.getOrderTypeID(ref listPosition, ref orderType)));

            orderPrice = orders.getOrderPrice(ref listPosition, ref orderType);

            if (listPosition >= 0) //If the order was found in the database.
            {

                string stationID = Convert.ToString(orders.getOrderStation(ref listPosition, ref orderType));//Get the station ID.
                if (orders.getOrderRuns(ref listPosition, ref orderType) < run)//If this order has not yet been udpated on this run.
                {
                    orders.incrementOrderRuns(ref listPosition, ref orderType);

                    //Find the most competitive sell order.
                    if (parts[7].CompareTo("False") == 0) //If there is at least one sell order
                    {
                        if (myOrderID.CompareTo(parts[4]) == 0) //If best sell order is my order, do nothing.
                        {
                            file.close();
                            return 0;
                        }
                        else if (stationID.CompareTo(parts[10]) == 0) //If this order is competing at my station
                        {
                            topSellOrder = line.Split(',');
                        }

                        while (parts[7].CompareTo("False") == 0 && (line = file.readLine()) != null)
                        {
                            parts = line.Split(',');
                            if (myOrderID.CompareTo(parts[4]) == 0 && topSellOrder[0].CompareTo("-1") == 0) //If best sell order is my order, do nothing.
                            {
                                file.close();
                                return 0;
                            }
                            else if (parts[7].CompareTo("False") == 0 && stationID.CompareTo(parts[10]) == 0 && topSellOrder[0].CompareTo("-1") == 0) //If this order is competing at my station
                            {
                                topSellOrder = line.Split(',');
                                break;
                            }
                        }
                    }
                    //Find the most competitive buy order.
                    while (parts[7].CompareTo("False") == 0 && (line = file.readLine()) != null)
                    {
                        parts = line.Split(',');
                    }
                    if (myOrderID.CompareTo(parts[4]) == 0) //If best buy order is my order, do nothing.
                    {
                        file.close();
                        return 0;
                    }

                    else if (isCompetingBuyOrder(Convert.ToInt32(parts[3]), Convert.ToInt32(parts[13]), parts[10], stationID)) //If this order is competing at my station
                    {
                        topBuyOrder = line.Split(',');
                    }
                    else
                    {
                        while ((line = file.readLine()) != null)
                        {
                            parts = line.Split(',');
                            if (myOrderID.CompareTo(parts[4]) == 0) //If best sell order is my order, do nothing.
                            {
                                file.close();
                                return 0;
                            }
                            else if (isCompetingBuyOrder(Convert.ToInt32(parts[3]), Convert.ToInt32(parts[13]), parts[10], stationID)) //If this order is competing at my station
                            {
                                topBuyOrder = line.Split(',');
                                break;
                            }
                        }
                    }
                    file.close();

                    double topSellPrice = Convert.ToDouble(topSellOrder[0]);
                    double topBuyPrice = Convert.ToDouble(topBuyOrder[0]);
                    double myOrderPrice = orders.getOrderPrice(ref listPosition, ref orderType);
                    if (orderType == 0) //if this is a sell order
                    {
                        if (topSellPrice > (topBuyPrice + (myOrderPrice - topBuyPrice) / 2))
                        {
                            //logModification(ref modifiedOrdersListView, ref run, ref itemName, "Sell", ref topSellPrice, ref topBuyPrice, ref myOrderPrice, Convert.ToDouble(topSellOrder[0]) - .01);
                            return (Convert.ToDouble(topSellOrder[0]) - .01); //The caller should adjust the order price to .01 ISK less than the best sell order.
                        }
                        {
                            exceptionSignificantPriceChange("Sell", "Significant price change detected.", ref myOrderPrice, ref topSellPrice, ref topBuyPrice);
                        }
                    }
                    else //Otherwise it is a buy order
                    {
                        if (topSellPrice >= 0 && topSellPrice - (topBuyPrice + topBuyPrice * 0.007406 + topSellPrice * 0.007406 + topSellPrice * 0.005) <= topBuyPrice * .005)
                        {
                            exceptionSignificantPriceChange("Buy", "Item no longer profitable.", ref myOrderPrice, ref topSellPrice, ref topBuyPrice);
                        }
                        else if (topSellPrice < 0 || topBuyPrice < (topSellPrice - (topSellPrice - myOrderPrice) / 2))
                        {
                            //logModification(ref modifiedOrdersListView, ref run, ref itemName, "Buy", ref topSellPrice, ref topBuyPrice, ref myOrderPrice, Convert.ToDouble(topSellOrder[0]) + .01);
                            return (Convert.ToDouble(topBuyOrder[0]) + .01); //The caller should adjust the order price to .01 ISK more than best buy order.
                        }
                        else
                        {
                            exceptionSignificantPriceChange("Buy", "Significant price change detected.", ref myOrderPrice, ref topSellPrice, ref topBuyPrice);
                        }
                    }
                }
            }
            file.close();
            return 0;
        }

        private int logModification(ref ListView modifiedOrdersListView, ref int run, ref string itemName, string orderType, ref double topSellPrice, ref double topBuyPrice, ref double myOrderPrice, double newPrice)
        {
            ListViewItem item = modifiedOrdersListView.Items.Add(run.ToString());
            item.SubItems.Add(itemName);
            item.SubItems.Add("Sell");
            item.SubItems.Add(topSellPrice.ToString());
            item.SubItems.Add(topBuyPrice.ToString());
            item.SubItems.Add(myOrderPrice.ToString());
            item.SubItems.Add(newPrice.ToString());
            modifiedOrdersListView.EnsureVisible(modifiedOrdersListView.Items.Count - 1);
            return 0;
        }

        private int exceptionSignificantPriceChange(string orderType, string errorMessage, ref double myOrderPrice, ref double topSellPrice, ref double topBuyPrice)
        {
            eventDispatcher.autoListerLog(orderType + " order of price " + myOrderPrice + " not adjusted.");
            eventDispatcher.autoListerLog(errorMessage);
            eventDispatcher.autoListerLog("Best Sell Price: " + topSellPrice);
            eventDispatcher.autoListerLog("Best Buy Price: " + topBuyPrice);
            eventDispatcher.autoListerLog(" ");
            return 0;
        }

        private string getFileNameFromText(string fileName)
        {
            string temp = fileName;
            int length = temp.Length;
            temp.Remove(0, 10);
            temp.Remove((length - 22), 22);
            return temp;
        }

        public int clearLastBuyOrder()
        {
            lastOrderTypeID = "0";
            return 0;
        }
    };
}
