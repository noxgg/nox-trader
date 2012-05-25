using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace noxiousET
{
    class OrderManager
    {

        List<Order>[] orders;
        int buyOrders;
        int sellOrders;

        public OrderManager(string filePathAndName, ref StreamReader file)
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

                if (parts[9].CompareTo("False") == 0) //If this is a sell order
                {
                    Order newOrder = new Order(parts[0], parts[1], parts[6], Convert.ToInt32(parts[8]), Convert.ToDouble(parts[10]), Convert.ToInt32(Convert.ToDouble(parts[12])));
                    orders[0].Add(newOrder);
                    ++sellOrders;
                }
                else //Otherwise it is a buy order
                {
                     Order newOrder;
                    try
                    {
                        newOrder = new Order(parts[0], parts[1], parts[6], Convert.ToInt32(parts[8]), Convert.ToDouble(parts[10]), Convert.ToInt32(Convert.ToDouble(parts[12])));
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
                if (typeID.CompareTo(orders[orderType][i].getTypeID()) == 0) //If this element is the target sell order.
                {
                    listPosition = i;
                    return orders[orderType][i].getOrderID();
                }
            }
            listPosition = -1;
            return "0";
        }

        //Returns 0 if there is no active order, 1 for active sell order, 2 for active buy order, 3 for both
        public int checkForActiveOrders(ref string typeID)
        {
            bool sellOrderExists = false;
            bool buyOrderExists = false;
            for (int i = 0; i < orders[0].Count(); ++i)
            {
                if (typeID.CompareTo(orders[0][i].getTypeID()) == 0) //If this element is the target sell order.
                {
                    sellOrderExists = true;
                }
            }
            for (int i = 0; i < orders[1].Count(); ++i)
            {
                if (typeID.CompareTo(orders[1][i].getTypeID()) == 0) //If this element is the target sell order.
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

        public string getOrderStation(ref int listPosition, ref int orderType)
        {
            return orders[orderType][listPosition].getStation();
        }

        public string getOrderTypeID(ref int listPosition, ref int orderType)
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

        public int[] getNumOfActiveOrders()
        {
            int[] temp = { sellOrders, buyOrders };
            return temp;
        }

        public double getOrderPrice(ref int listPosition, ref int orderType)
        {
            return orders[orderType][listPosition].getPrice();
        }
    };
}
