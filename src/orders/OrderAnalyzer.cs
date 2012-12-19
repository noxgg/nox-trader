using System;
using System.Collections.Generic;

//TODO no buy orders exist?

namespace noxiousET.src.orders
{
    internal class OrderAnalyzer
    {
        private bool _bestBuyOwned;
        private bool _bestSellOwned;
        private double _buyPrice;
        private double _ownedBuyPrice;
        private double _ownedSellPrice;
        private double _sellPrice;
        private bool _someBuyOwned;
        private bool _someSellOwned;
        private string _typeid;

        public OrderAnalyzer()
        {
            OrderSet = new OrderManager();
        }

        public OrderManager OrderSet { set; get; }

        public Boolean NoSellsExist { set; get; }
        public Boolean NoBuysExist { set; get; }

        public void AnalyzeInvestment(List<String[]> orderData, String typeid, String stationid)
        {
            Analyze(ref orderData, ref typeid, ref stationid);
        }

        public void AnalyzeInvestment(List<String[]> orderData, String stationid)
        {
            Analyze(ref orderData, ref orderData[0][2], ref stationid);
        }

        private void Analyze(ref List<String[]> orderData, ref String typeId, ref String stationId)
        {
            _buyPrice = 0;
            _sellPrice = 0;
            _ownedSellPrice = 0;
            _ownedBuyPrice = 0;
            _bestSellOwned = false;
            _bestBuyOwned = false;
            int i = 0;
            int orderDataCount = orderData.Count;
            String[] order = i < orderDataCount ? orderData[i++] : null;
            _typeid = order[2];
            if (!typeId.Equals(0))
                throw new Exception("Wrong typeid");
            _someSellOwned = OrderSet.ExistsOrderOfType(Convert.ToInt32(typeId), 0);
            _someBuyOwned = OrderSet.ExistsOrderOfType(Convert.ToInt32(typeId), 1);
            //Find first order at target station
            while (order != null && order[7].Equals("False") && !order[10].Equals(stationId))
                order = i < orderDataCount ? orderData[i++] : null;

            //If this order is a sell order and if it is competing at the target station
            if (order != null && order[7].Equals("False") && order[10].Equals(stationId))
            {
                //Is this an owned order?
                if (OrderSet.IsOrderOwned(order[4], 0))
                {
                    _ownedSellPrice = Convert.ToDouble(order[0]);
                    _bestSellOwned = true;
                }
                _sellPrice = Convert.ToDouble(order[0]);
            }
            else
            {
                _bestSellOwned = false;
                _sellPrice = 0;
            }

            //If this is the best sell order, fetch the next nest competitive price
            if (_bestSellOwned)
            {
                order = i < orderDataCount ? orderData[i++] : null;
                while (order != null && order[7].Equals("False") && !order[10].Equals(stationId))
                    order = i < orderDataCount ? orderData[i++] : null;
                if (order != null && order[7].Equals("False") && order[10].Equals(stationId))
                    _sellPrice = Convert.ToDouble(order[0]);
                else
                    _sellPrice = 0;
            }


            //Jump over any remaining sell orders
            while (order != null && order[7].Equals("False"))
            {
                if (OrderSet.IsOrderOwned(order[4], 0))
                    _ownedSellPrice = Convert.ToDouble(order[0]);
                order = i < orderDataCount ? orderData[i++] : null;
            }

            //Find first order that is competing at target station
            while (order != null &&
                   !IsCompetingBuyOrder(Convert.ToInt32(order[3]), Convert.ToInt32(order[13]), order[10], stationId))
                order = i < orderDataCount ? orderData[i++] : null;

            if (order != null && order[7].Equals("True") &&
                IsCompetingBuyOrder(Convert.ToInt32(order[3]), Convert.ToInt32(order[13]), order[10], stationId))
            {
                //Is this an owned order?
                if (OrderSet.IsOrderOwned(order[4], 1))
                {
                    _ownedBuyPrice = Convert.ToDouble(order[0]);
                    _bestBuyOwned = true;
                }
                _buyPrice = Convert.ToDouble(order[0]);
            }
            else
            {
                _bestBuyOwned = false;
                _buyPrice = 0;
            }

            //If this is the best sell order, fetch the next best price.
            if (_bestBuyOwned)
            {
                order = i < orderDataCount ? orderData[i++] : null;
                while (order != null &&
                       !IsCompetingBuyOrder(Convert.ToInt32(order[3]), Convert.ToInt32(order[13]), order[10], stationId))
                    order = i < orderDataCount ? orderData[i++] : null;
                if (order != null && order[7].Equals("True") &&
                    IsCompetingBuyOrder(Convert.ToInt32(order[3]), Convert.ToInt32(order[13]), order[10], stationId))
                    _buyPrice = Convert.ToDouble(order[0]);
                else
                    _buyPrice = 0;
            }

            while (order != null && _ownedBuyPrice.Equals(0))
            {
                if (OrderSet.IsOrderOwned(order[4], 1))
                    _ownedBuyPrice = Convert.ToDouble(order[0]);
                order = i < orderDataCount ? orderData[i++] : null;
            }

            if (_sellPrice <= 0 && _buyPrice <= 0) //If there are no orders to compare against, return
            {
                NoSellsExist = NoBuysExist = true;
            }
            else if (_sellPrice <= 0) //If there are no active sell orders, set sell to 1.5x best buy
            {
                NoSellsExist = true;
                _sellPrice = _buyPrice*1.5;
            }
            else if (_buyPrice <= 0) //If there are no active buy orders, set buy tp 1/2 best sell
            {
                NoBuysExist = true;
                _buyPrice = _sellPrice/2;
            }
        }

        private static bool IsCompetingBuyOrder(int range, int jumps, string stationid, string targetStationid)
        {
            //If this is my station and the order has a station range
            return range == -1 && stationid.Equals(targetStationid) || range - jumps >= 0;
        }

        public int GetTypeId()
        {
            return Convert.ToInt32(_typeid);
        }

        public double GetBuyPrice()
        {
            return _buyPrice;
        }

        public double GetSellPrice()
        {
            return _sellPrice;
        }

        public double GetPrice(int type)
        {
            return type == EtConstants.Buy ? _buyPrice : _sellPrice;
        }

        public double GetOwnedBuyPrice()
        {
            return _ownedBuyPrice;
        }

        public double GetOwnedSellPrice()
        {
            return _ownedSellPrice;
        }

        public void SetOwnedBuyPrice(double ownedBuyPrice)
        {
            _ownedBuyPrice = ownedBuyPrice;
        }

        public double GetOwnedPrice(int type)
        {
            return type == EtConstants.Buy ? _ownedBuyPrice : _ownedSellPrice;
        }

        public bool IsSomeBuyOwned()
        {
            return _someBuyOwned;
        }

        public bool IsSomeSellOwned()
        {
            return _someSellOwned;
        }

        public bool IsBestBuyOwned()
        {
            return _bestBuyOwned;
        }

        public bool IsBestSellOwned()
        {
            return _bestSellOwned;
        }

        public bool IsBestOrderOwned(int type)
        {
            return type == EtConstants.Buy ? _bestBuyOwned : _bestSellOwned;
        }
    };
}