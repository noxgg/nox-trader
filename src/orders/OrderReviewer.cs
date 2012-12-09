using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using noxiousET.src.etevent;

namespace noxiousET.src.orders
{
    class OrderReviewer
    {
        EventDispatcher eventDispatcher;
        Dictionary<string, List<String[]>> ordersForReview;
        Dictionary<string, string> actionsToTake;

        public OrderReviewer(EventDispatcher eventDispatcher)
        {
            this.eventDispatcher = eventDispatcher;
            this.eventDispatcher.updateActionToTakeRequestHandler += new EventDispatcher.UpdateActionToTakeRequestHandler(updateActionToTakeListener);
            ordersForReview = new Dictionary<string, List<string[]>>();
            actionsToTake = new Dictionary<string, string>();
        }

        public List<String[]> getOrdersForReviewEntry(string character, string typeId, int buyOrSell)
        {
            string key = generateKey(character, int.Parse(typeId), buyOrSell);
            if (ordersForReview.ContainsKey(key))
                return ordersForReview[key];
            return null;
        }

        public string getAction(string character, string typeId, int buyOrSell)
        {
            string key = generateKey(character, int.Parse(typeId), buyOrSell);
            if (actionsToTake.ContainsKey(key))
                return actionsToTake[key];
            return "";
        }

        public double getPrice(string character, int typeId, int buyOrSell)
        {
            string key = generateKey(character, typeId, buyOrSell);

            if (buyOrSell.Equals(EtConstants.SELL))
            {
                return Double.Parse(ordersForReview[key][1][1]);
            }
            else
            {
                foreach (string[] orderRow in ordersForReview[key])
                {
                    if (orderRow[0].Equals("Buy"))
                        return Double.Parse(orderRow[1]);
                }
            }
            return 0;
        }

        public string getItemName(string character, string typeId, int buyOrSell)
        {
            string key = generateKey(character, int.Parse(typeId), buyOrSell);
            if (ordersForReview.ContainsKey(key))
                return ordersForReview[key][0][1];
            return "";
        }

        private void updateActionToTakeListener(Object o, string character, string typeid, string buyOrSell, string action)
        {
            string key = generateKey(character, int.Parse(typeid), int.Parse(buyOrSell));
            if (actionsToTake.ContainsKey(key))
                actionsToTake.Remove(key);
            actionsToTake.Add(key, action);
        }

        public List<string> getActionsToTake()
        {
            return actionsToTake.Keys.ToList<string>();
        }

        public void addOrderRequiringReview(string stationId, List<string[]> orderData, string ownedPrice, string characterName, string itemName)
        {
            int typeId = int.Parse(orderData[0][2]);
            List<String[]> printableOrderInfo = new List<string[]>();
            string key = generateKey(characterName, typeId, -1);
            printableOrderInfo.Add(new string[] { characterName, itemName, typeId.ToString() });
            
            foreach (string[] orderRow in orderData)
            {
                if (isCompetingOrder(Convert.ToBoolean(orderRow[7]), Convert.ToInt32(orderRow[3]), Convert.ToInt32(orderRow[13]), orderRow[10], stationId))
                {
                    if (orderRow[0].Equals(ownedPrice))
                    {
                        key = generateKey(characterName, typeId, buyOrSellToInt(orderRow[7]));
                        printableOrderInfo.Add(new string[] { "OWNED " + printableBuyOrSell(orderRow[7]), orderRow[0], orderRow[1], getPrintableTimeLeft(orderRow[8], orderRow[9]) });
                    }
                    else
                    {
                        printableOrderInfo.Add(new string[] { printableBuyOrSell(orderRow[7]), orderRow[0], orderRow[1], getPrintableTimeLeft(orderRow[8], orderRow[9]) });
                    }
                }
            }
            if (ordersForReview.ContainsKey(key))
            {
                ordersForReview.Remove(key);
            }
            ordersForReview.Add(key, printableOrderInfo);
            actionsToTake.Add(key, "Ignore");
        }

        private string generateKey(string characterName, int typeId, int buyOrSell)
        {
            return (characterName + "," + typeId.ToString() + "," + buyOrSell.ToString());
        }

        public void removeOrderRequiringReview(string character, int typeId, int buyOrSell)
        {
            string key = generateKey(character, typeId, buyOrSell);
            if (ordersForReview.ContainsKey(key))
            {
                ordersForReview.Remove(key);
                actionsToTake.Remove(key);
            }
        }

        public Boolean shouldCancel(string characterName, int typeId, int buyOrSell)
        {
            string key = generateKey(characterName, typeId, buyOrSell);
            if (actionsToTake.ContainsKey(key) && actionsToTake[key].Equals("Cancel"))
                return true;
            return false;
        }

        public Boolean shouldUpdate(string characterName, int typeId, int buyOrSell)
        {
            string key = generateKey(characterName, typeId, buyOrSell);
            if (actionsToTake.ContainsKey(key) && actionsToTake[key].Equals("Update"))
                return true;
            return false;
        }

        public Boolean shouldIgnore(string characterName, int typeId, int buyOrSell)
        {
            string key = generateKey(characterName, typeId, buyOrSell);
            if (actionsToTake.ContainsKey(key) && actionsToTake[key].Equals("Ignore"))
                return true;
            return false;
        }

        private int buyOrSellToInt(String isBidColumn)
        {
            Boolean isBuyOrder = Convert.ToBoolean(isBidColumn);

            return isBuyOrder ? EtConstants.BUY : EtConstants.SELL;
        }

        private String printableBuyOrSell(String isBidColumn)
        {
            Boolean isBuyOrder = Convert.ToBoolean(isBidColumn);

            return isBuyOrder ? "Buy" : "Sell";
        }

        private String getPrintableTimeLeft(String issueDate, String duration)
        {
            issueDate = issueDate.Substring(0, issueDate.Length - 7);
            DateTime issueDateDT = new DateTime();
            long durationTicks = new DateTime().AddDays(int.Parse(duration)).Ticks;

            int value = int.Parse(issueDate.Substring(0, 4));
            issueDateDT = issueDateDT.AddYears(value - 1);

            value = int.Parse(issueDate.Substring(5, 2));
            issueDateDT = issueDateDT.AddMonths(value - 1);

            value = int.Parse(issueDate.Substring(8, 2));
            issueDateDT = issueDateDT.AddDays(value - 1);

            value = int.Parse(issueDate.Substring(11, 2));
            issueDateDT = issueDateDT.AddHours(value);

            value = int.Parse(issueDate.Substring(14, 2));
            issueDateDT = issueDateDT.AddMinutes(value);

            long issueDateTicks = issueDateDT.Ticks;
            long currentDateTicks = DateTime.UtcNow.Ticks;
            long timeRemainingTicks = durationTicks - (currentDateTicks - issueDateTicks);
            double daysRemaining = (double)timeRemainingTicks / (double)TimeSpan.TicksPerDay;
            return (daysRemaining).ToString("##.####");
        }

        private bool isCompetingOrder(Boolean isBuyOrder, int range, int jumps, string stationid, string targetStationid)
        {
            if (!isBuyOrder && stationid.Equals(targetStationid))
                return true;
            else if (isBuyOrder && (range == -1 && stationid.CompareTo(targetStationid) == 0 || range - jumps >= 0))//If this is my station or the order is in range
                return true;
            return false;
        }
    }
}
