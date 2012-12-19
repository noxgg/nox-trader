using System;
using System.Collections.Generic;
using noxiousET.src.data.accounts;

namespace noxiousET.src.data.characters
{
    internal class Character
    {
        public Character(String name)
        {
            Name = name;
            QuantityThreshHolds = new List<int[]>();
        }

        public String Name { set; get; }
        public String Id { set; get; }
        public Account Account { set; get; }
        public Boolean ShouldTradeItems { set; get; }
        public Boolean ShouldTradeShips { set; get; }
        public Boolean ShouldAdjustSells { set; get; }
        public Boolean ShouldAdjustBuys { set; get; }
        public int MaximumOrders { set; get; }
        public int StationId { set; get; }
        public List<int[]> QuantityThreshHolds { set; get; }
        public int LoginColor { set; get; }
        public Queue<int> TradeQueue { set; get; }
        public Dictionary<int, int> TradeHistory { set; get; }
    }
}