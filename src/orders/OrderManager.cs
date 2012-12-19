using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace noxiousET.src.orders
{
    internal class OrderManager
    {
        private int _buyOrders;
        private List<Order>[] _orders;
        private int _sellOrders;

        public void CreateOrderSet(string filePathAndName, ref StreamReader file, Dictionary<int, int> tradeHistory)
        {
            _orders = new List<Order>[2];
            _orders[0] = new List<Order>();
            _orders[1] = new List<Order>();
            _buyOrders = 0;
            _sellOrders = 0;

            string line = file.ReadLine();
            while ((line = file.ReadLine()) != null)
            {
                string[] parts = line.Split(',');

                //TODO DEPRECIATE?
                //if (!tradeHistory.ContainsKey(typeID))
                //    tradeHistory.Add(typeID, typeID);

                if (parts[9].Equals("False")) //If this is a sell order
                {
                    var newOrder = new Order(parts[0], Convert.ToInt32(parts[1]), parts[6], Convert.ToInt32(parts[8]),
                                             Convert.ToDouble(parts[10]), Convert.ToInt32(Convert.ToDouble(parts[12])));
                    _orders[0].Add(newOrder);
                    ++_sellOrders;
                }
                else //Otherwise it is a buy order
                {
                    try
                    {
                        var newOrder = new Order(parts[0], Convert.ToInt32(parts[1]), parts[6],
                                                 Convert.ToInt32(parts[8]), Convert.ToDouble(parts[10]),
                                                 Convert.ToInt32(Convert.ToDouble(parts[12])));
                        _orders[1].Add(newOrder);
                        ++_buyOrders;
                    }
                    catch
                    {
                        file.Close();
                        return;
                    }
                }
            }
            file.Close();
            File.Delete(filePathAndName);
        }

        public void AddOrder(int typeID, int orderType)
        {
            _orders[orderType].Add(new Order(typeID));
        }

        public string GetOrderIDandListPosition(ref string typeID, ref int orderType, out int listPosition)
        {
            for (int i = 0; i < _orders[orderType].Count(); ++i)
            {
                if (Convert.ToInt32(typeID) == _orders[orderType][i].GetTypeId())
                    //If this element is the target sell order.
                {
                    listPosition = i;
                    return Convert.ToString(_orders[orderType][i].GetOrderId());
                }
            }
            listPosition = -1;
            return "0";
        }

        //Returns 0 if there is no active order, 1 for active sell order, 2 for active buy order, 3 for both
        public int CheckForActiveOrders(int typeID)
        {
            bool sellOrderExists = false;
            bool buyOrderExists = false;
            for (int i = 0; i < _orders[0].Count(); ++i)
            {
                if (typeID == _orders[0][i].GetTypeId()) //If this element is the target sell order.
                {
                    sellOrderExists = true;
                }
            }
            for (int i = 0; i < _orders[1].Count(); ++i)
            {
                if (typeID == _orders[1][i].GetTypeId()) //If this element is the target sell order.
                {
                    buyOrderExists = true;
                }
            }
            return sellOrderExists && buyOrderExists ? 3 : (buyOrderExists ? 2 : (sellOrderExists ? 1 : 0));
        }

        public bool ExistsOrderOfType(int typeid, int type)
        {
            return _orders[type].Any(o => o.GetTypeId() == typeid);
        }

        public bool IsOrderOwned(String orderid, int type)
        {
            return _orders[type].Any(o => o.GetOrderId().Equals(orderid));
        }

        public int GetListPosition(ref string typeID, ref int orderType)
        {
            for (int i = 0; i < _orders[orderType].Count(); ++i)
            {
                if (typeID.CompareTo(_orders[orderType][i].GetTypeId()) == 0)
                    //If this element is the target sell order.
                {
                    return i;
                }
            }
            return -1;
        }

        public String GetOrderStation(ref int listPosition, ref int orderType)
        {
            return _orders[orderType][listPosition].GetStationid();
        }

        public int GetOrderTypeID(ref int listPosition, ref int orderType)
        {
            return _orders[orderType][listPosition].GetTypeId();
        }

        public int IncrementOrderRuns(ref int listPosition, ref int orderType)
        {
            _orders[orderType][listPosition].IncrementRuns();
            return 0;
        }

        public int GetOrderRuns(ref int listPosition, ref int orderType)
        {
            return _orders[orderType][listPosition].GetRuns();
        }

        public int[] GetNumberOfBuysAndSells()
        {
            int[] temp = {_sellOrders, _buyOrders};
            return temp;
        }

        public int GetNumberOfActiveOrders()
        {
            return _sellOrders + _buyOrders;
        }

        public double GetOrderPrice(ref int listPosition, ref int orderType)
        {
            return _orders[orderType][listPosition].GetPrice();
        }
    };
}