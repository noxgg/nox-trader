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

        private const int MyOrdersColumnOrderId = 0;
        private const int MyOrdersColumnTypeId = 1;
        private const int MyOrdersColumnCharacterId = 2;
        private const int MyOrdersColumnCharacterName = 3;
        private const int MyOrdersColumnRegionId = 4;
        private const int MyOrdersColumnRegionName = 5;
        private const int MyOrdersColumnStationId = 6;
        private const int MyOrdersColumnStationName = 7;
        private const int MyOrdersColumnRange = 8;
        private const int MyOrdersColumnIsBuyOrder = 9;
        private const int MyOrdersColumnPrice = 10;
        private const int MyOrdersColumnVolumeEntered = 11;
        private const int MyOrdersColumnVolumeRemaining = 12;
        private const int MyOrdersColumnIssueDate = 13;
        private const int MyOrdersColumnOrderState = 14;
        private const int MyOrdersColumnMinimumVolume = 15;
        private const int MyOrdersColumnAccountId = 16;
        private const int MyOrdersColumnDuration = 17;
        private const int MyOrdersColumnIsCorp = 18;
        private const int MyOrdersColumnSolarSystemid = 19;
        private const int MyOrdersColumnSolarSystemName = 20;
        private const int MyOrdersColumnEscrow = 21;
        private const int NumberOfMyOrdersColumns = 23;

        public void CreateOrderSet(string filePathAndName, ref StreamReader file, Dictionary<int, int> tradeHistory)
        {
            _orders = new List<Order>[2];
            _orders[EtConstants.Sell] = new List<Order>();
            _orders[EtConstants.Buy] = new List<Order>();
            _buyOrders = 0;
            _sellOrders = 0;

            string orderLine = file.ReadLine();

            //Does the header have the correct number of columns?
            if (orderLine != null && !orderLine.Split(',').Count().Equals(NumberOfMyOrdersColumns))
                throw new Exception("The current file doesn't appear to be a character's order list.");
            

            while ((orderLine = file.ReadLine()) != null)
            {
                string[] orderRow = orderLine.Split(',');
                
                var newOrder = new Order(orderRow[MyOrdersColumnOrderId], 
                    Convert.ToInt32(orderRow[MyOrdersColumnTypeId]), 
                    orderRow[MyOrdersColumnStationId], 
                    Convert.ToInt32(orderRow[MyOrdersColumnRange]),
                    Convert.ToDouble(orderRow[MyOrdersColumnPrice]), 
                    Convert.ToInt32(Convert.ToDouble(orderRow[MyOrdersColumnVolumeRemaining])));

                if (orderRow[MyOrdersColumnIsBuyOrder].Equals("False")) //If this is a sell order
                {
                    _orders[EtConstants.Sell].Add(newOrder);
                    ++_sellOrders;
                }
                else //Otherwise it is a buy order
                {
                    _orders[EtConstants.Buy].Add(newOrder);
                    ++_buyOrders;
                }
            }
            file.Close();
            File.Delete(filePathAndName);
        }

        public void AddOrder(int typeID, int orderType)
        {
            _orders[orderType].Add(new Order(typeID));
        }

        //TODO refactor this terrible function
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

        public bool ExistsAnyOrder(int typeId)
        {
            return CheckForActiveOrders(typeId) > 0;
        }

        public bool ExistsOrderOfType(int typeid, int type)
        {
            return _orders[type].Any(o => o.GetTypeId() == typeid);
        }

        public bool ExistsOrderOfType(string typeid, int type)
        {
            int typeId = int.Parse(typeid);
            return _orders[type].Any(o => o.GetTypeId() == typeId);
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

        public int GetNumberOfOrders(bool getBuys)
        {
            return getBuys ? _buyOrders : _sellOrders;
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