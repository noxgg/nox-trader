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
        private string _typeId;

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

        public bool IsNewOrderData(List<String[]> orderData)
        {
            return !orderData[0][EtConstants.OrderDataColumnTypeId].Equals(_typeId);
        }

        private void Analyze(ref List<String[]> orderData, ref String expectedTypeId, ref String stationId)
        {
            _buyPrice = 0;
            _sellPrice = 0;
            _ownedSellPrice = 0;
            _ownedBuyPrice = 0;
            _bestSellOwned = false;
            _bestBuyOwned = false;
            int i = 0;
            int orderDataCount = orderData.Count;

            String[] orderRow = i < orderDataCount ? orderData[i++] : null;
            _typeId = orderRow[EtConstants.OrderDataColumnTypeId];
            if (!expectedTypeId.Equals(_typeId))
                throw new Exception("Wrong typeid");

            _someSellOwned = OrderSet.ExistsOrderOfType(expectedTypeId, EtConstants.Sell);
            _someBuyOwned = OrderSet.ExistsOrderOfType(expectedTypeId, EtConstants.Buy);

            //Navigate past irrelevant orders that aren't competing at the current station.
            while (orderRow != null && IsNonCompetitiveSellOrder(orderRow, stationId))
            {
                orderRow = i < orderDataCount ? orderData[i++] : null;
            }

            if (orderRow != null && IsCompetitiveSellOrder(orderRow, stationId))
            {
                _sellPrice = Convert.ToDouble(orderRow[EtConstants.OrderDataColumnPrice]);
                //Is this an owned order?
                if (OrderSet.IsOrderOwned(orderRow[EtConstants.OrderDataColumnOrderId], EtConstants.Sell))
                {
                    _ownedSellPrice = _sellPrice;
                    _bestSellOwned = true;
                }
            }
            else
            {
                _bestSellOwned = false;
                _sellPrice = 0;
            }

            //If the best sell order is owned, fetch the next most competitive price
            if (_bestSellOwned)
            {
                orderRow = i < orderDataCount ? orderData[i++] : null;

                while (orderRow != null && IsNonCompetitiveSellOrder(orderRow, stationId))
                    orderRow = i < orderDataCount ? orderData[i++] : null;

                if (orderRow != null && IsCompetitiveSellOrder(orderRow, stationId))
                    _sellPrice = Convert.ToDouble(orderRow[EtConstants.OrderDataColumnPrice]);
                else
                    _sellPrice = 0;
            }


            //Jump over any remaining sell orders
            while (orderRow != null && orderRow[EtConstants.OrderDataColumnIsBuyOrder].Equals("False"))
            {
                if (OrderSet.IsOrderOwned(orderRow[EtConstants.OrderDataColumnOrderId], EtConstants.Sell))
                    _ownedSellPrice = Convert.ToDouble(orderRow[EtConstants.OrderDataColumnPrice]);

                orderRow = i < orderDataCount ? orderData[i++] : null;
            }

            //Find first order that is competing at target station
            while (orderRow != null &&  !IsCompetetiveBuyOrder(orderRow, stationId))
                orderRow = i < orderDataCount ? orderData[i++] : null;

            if (orderRow != null 
                && orderRow[EtConstants.OrderDataColumnIsBuyOrder].Equals("True") 
                && IsCompetetiveBuyOrder(orderRow, stationId))
            {
                _buyPrice = Convert.ToDouble(orderRow[EtConstants.OrderDataColumnPrice]);
                //Is this an owned order?
                if (OrderSet.IsOrderOwned(orderRow[EtConstants.OrderDataColumnOrderId], EtConstants.Buy))
                {
                    _ownedBuyPrice = _buyPrice;
                    _bestBuyOwned = true;
                }
            }
            else
            {
                _bestBuyOwned = false;
                _buyPrice = 0;
            }

            //If the best buy order is owned, fetch the next best price.
            if (_bestBuyOwned)
            {
                orderRow = i < orderDataCount ? orderData[i++] : null;

                while (orderRow != null && !IsCompetetiveBuyOrder(orderRow, stationId))
                    orderRow = i < orderDataCount ? orderData[i++] : null;

                if (orderRow != null
                    && orderRow[EtConstants.OrderDataColumnIsBuyOrder].Equals("True")
                    && IsCompetetiveBuyOrder(orderRow, stationId))
                {
                    _buyPrice = Convert.ToDouble(orderRow[EtConstants.OrderDataColumnPrice]);
                }
                else
                {
                    _buyPrice = 0;
                }
            }

            while (orderRow != null && _ownedBuyPrice.Equals(0))
            {
                if (OrderSet.IsOrderOwned(orderRow[EtConstants.OrderDataColumnOrderId], EtConstants.Buy))
                    _ownedBuyPrice = Convert.ToDouble(orderRow[EtConstants.OrderDataColumnPrice]);

                orderRow = i < orderDataCount ? orderData[i++] : null;
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

        private static bool IsCompetetiveBuyOrder(string[] orderRow, string targetStationid)
        {
            int range = Convert.ToInt32(orderRow[EtConstants.OrderDataColumnRange]);
            int jumps = Convert.ToInt32(orderRow[EtConstants.OrderDataColumnJumps]);
            //If this is my station and the order has a station range
            return range.Equals(EtConstants.StationRange) && orderRow[EtConstants.OrderDataColumnStationId].Equals(targetStationid) 
                || range - jumps >= 0;
        }

        private Boolean IsNonCompetitiveSellOrder(string[] orderRow, string targetStationId)
        {
            return orderRow[EtConstants.OrderDataColumnIsBuyOrder].Equals("False")
                   && !orderRow[EtConstants.OrderDataColumnStationId].Equals(targetStationId);
        }

        private Boolean IsCompetitiveSellOrder(string[] orderRow, string targetStationId)
        {
            return orderRow[EtConstants.OrderDataColumnIsBuyOrder].Equals("False")
                   && orderRow[EtConstants.OrderDataColumnStationId].Equals(targetStationId);
        }

        public int GetTypeId()
        {
            return Convert.ToInt32(_typeId);
        }

        public double GetBuyPrice()
        {
            return _buyPrice;
        }

        public double GetSellPrice()
        {
            return _sellPrice;
        }

        public double GetPrice(bool isBuyOrder)
        {
            return isBuyOrder ? _buyPrice : _sellPrice;
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

        public double GetOwnedPrice(bool isBuyOrder)
        {
            return isBuyOrder ? _ownedBuyPrice : _ownedSellPrice;
        }

        public bool IsSomeBuyOwned()
        {
            return _someBuyOwned;
        }

        public bool IsSomeSellOwned()
        {
            return _someSellOwned;
        }

        public bool IsSomeOrderOwned()
        {
            return _someBuyOwned || _someSellOwned;
        }

        public bool IsBestBuyOwned()
        {
            return _bestBuyOwned;
        }

        public bool IsBestSellOwned()
        {
            return _bestSellOwned;
        }

        public bool IsBestOrderOwned(bool isBuyOrder)
        {
            return isBuyOrder ? _bestBuyOwned : _bestSellOwned;
        }
    };
}