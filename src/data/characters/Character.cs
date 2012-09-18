using System;
using System.Collections.Generic;
using noxiousET.src.data.accounts;

namespace noxiousET.src.data.characters
{
    class Character
    {
        public String name { set; get; }
        public String id { set; get; }
        public Account account { set; get; }
        public Boolean tradeItems { set; get; }
        public Boolean tradeShips { set; get; }
        public Boolean adjustSells { set; get; }
        public Boolean adjustBuys { set; get; }
        public int maximumOrders { set; get; }
        public int stationid { set; get; }
        public List<int[]> quantityThreshHolds { set; get; }
        public int loginColor { set; get; }
        public Queue<int> tradeQueue { set; get; }
        public Dictionary<int, int> tradeHistory { set; get; }

        public Character(String name)
        {
            this.name = name;
            this.quantityThreshHolds = new List<int[]>();
        }
    }
}