using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace noxiousET.src.orders
{
    class OrderManager
    {

        List<Order>[] orders;
        int buyOrders;
        int sellOrders;
        int typeID;

        public OrderManager()
        {
        }

        public void createOrderSet(string filePathAndName, ref StreamReader file, Dictionary<int, int> tradeHistory)
        {
            orders = new List<Order>[2];
            orders[0] = new List<Order>();
            orders[1] = new List<Order>();
            string line;
            string[] parts;
            buyOrders = 0;
            sellOrders = 0;

            line = file.ReadLine(); //Skip header row
            while ((line = file.ReadLine()) != null)
            {
                parts = line.Split(',');

                typeID = Convert.ToInt32(parts[1]);
                if (!tradeHistory.ContainsKey(typeID))
                    tradeHistory.Add(typeID, typeID);

                if (parts[9].CompareTo("False") == 0) //If this is a sell order
                {
                    Order newOrder = new Order(parts[0], Convert.ToInt32(parts[1]), parts[6], Convert.ToInt32(parts[8]), Convert.ToDouble(parts[10]), Convert.ToInt32(Convert.ToDouble(parts[12])));
                    orders[0].Add(newOrder);
                    ++sellOrders;
                }
                else //Otherwise it is a buy order
                {
                    Order newOrder;
                    try
                    {
                        newOrder = new Order(parts[0], Convert.ToInt32(parts[1]), parts[6], Convert.ToInt32(parts[8]), Convert.ToDouble(parts[10]), Convert.ToInt32(Convert.ToDouble(parts[12])));
                    }
                    catch
                    {
                        file.Close();
                        return;
                    }
                    orders[1].Add(newOrder);
                    ++buyOrders;
                }
            }
            file.Close();
            File.Delete(filePathAndName);
        }

        public string getOrderIDandListPosition(ref string typeID, ref int orderType, out int listPosition)
        {
            for (int i = 0; i < orders[orderType].Count(); ++i)
            {
                if (Convert.ToInt32(typeID) == orders[orderType][i].getTypeID()) //If this element is the target sell order.
                {
                    listPosition = i;
                    return Convert.ToString(orders[orderType][i].getOrderID());
                }
            }
            listPosition = -1;
            return "0";
        }

        //Returns 0 if there is no active order, 1 for active sell order, 2 for active buy order, 3 for both
        public int checkForActiveOrders(int typeID)
        {
            bool sellOrderExists = false;
            bool buyOrderExists = false;
            for (int i = 0; i < orders[0].Count(); ++i)
            {
                if (typeID == orders[0][i].getTypeID()) //If this element is the target sell order.
                {
                    sellOrderExists = true;
                }
            }
            for (int i = 0; i < orders[1].Count(); ++i)
            {
                if (typeID == orders[1][i].getTypeID()) //If this element is the target sell order.
                {
                    buyOrderExists = true;
                }
            }
            if (sellOrderExists && buyOrderExists)
            {
                return 3;
            }
            else if (buyOrderExists)
            {
                return 2;
            }
            else if (sellOrderExists)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public bool existsOrderOfType(int typeid, int type)
        {
            foreach (Order o in orders[type])
            {
                if (o.getTypeID() == typeid)
                    return true;
            }
            return false;
        }

        public bool isOrderOwned(String orderid, int type)
        {
            foreach (Order o in orders[type])
            {
                if (o.getOrderID().CompareTo(orderid) == 0)
                    return true;
            }
            return false;
        }

        public int getListPosition(ref string typeID, ref int orderType)
        {
            for (int i = 0; i < orders[orderType].Count(); ++i)
            {
                if (typeID.CompareTo(orders[orderType][i].getTypeID()) == 0) //If this element is the target sell order.
                {
                    return i;
                }
            }
            return -1;
        }

        public String getOrderStation(ref int listPosition, ref int orderType)
        {
            return orders[orderType][listPosition].getStationid();
        }

        public int getOrderTypeID(ref int listPosition, ref int orderType)
        {
            return orders[orderType][listPosition].getTypeID();
        }

        public int incrementOrderRuns(ref int listPosition, ref int orderType)
        {
            orders[orderType][listPosition].incrementRuns();
            return 0;
        }

        public int getOrderRuns(ref int listPosition, ref int orderType)
        {
            return orders[orderType][listPosition].getRuns();
        }

        public int[] getNumberOfBuysAndSells()
        {
            int[] temp = { sellOrders, buyOrders };
            return temp;
        }

        public int getNumberOfActiveOrders()
        {
            return sellOrders + buyOrders;
        }

        public double getOrderPrice(ref int listPosition, ref int orderType)
        {
            return orders[orderType][listPosition].getPrice();
        }
    };
}
