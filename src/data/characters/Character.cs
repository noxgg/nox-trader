using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using noxiousET.src.data.accounts;

namespace noxiousET.src.data.characters
{
    class Character
    {
        public String name { set; get;  }
        public Account account { set; get; }
        public Boolean tradeItems { set; get; }
        public Boolean tradeShips { set; get; }
        public Boolean adjustSells { set; get; }
        public Boolean adjustBuys { set; get; }
        public int maximumOrders { set; get; }
        public int stationid { set; get; }
        public int fileNameTrimLength { set; get; }
        public int autoAdjustsPerAutoList { set; get; }
        public List<int[]> quantityThreshHolds { set; get; }
        public int loginColors { set; get; }

        public Character(String name)
        {
            this.name = name;
            this.quantityThreshHolds = new List<int[]>();
        }

        public Character(String name, Account account, Boolean tradeItems, Boolean tradeShips, Boolean adjustSells, Boolean adjustBuys, int maximumOrders,
            int stationid, int fileNameTrimLength, int autoAdjustsPerAutoList, List<int[]> quantityThreshHolds, int loginColors)
        {
            this.name = name;
            this.account = account;
            this.tradeItems = tradeItems;
            this.tradeShips = tradeShips;
            this.adjustSells = adjustSells;
            this.adjustBuys = adjustBuys;
            this.maximumOrders = maximumOrders;
            this.stationid = stationid;
            this.fileNameTrimLength = fileNameTrimLength;
            this.autoAdjustsPerAutoList = autoAdjustsPerAutoList;
            this.quantityThreshHolds = quantityThreshHolds;
            this.loginColors = loginColors;
        }
    }
}