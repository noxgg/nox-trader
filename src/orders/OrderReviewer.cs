using System;
using System.Collections.Generic;
using System.Linq;
using noxiousET.src.etevent;

namespace noxiousET.src.orders
{
    internal class OrderReviewer
    {
        private readonly Dictionary<string, string> _actionsToTake;
        private readonly EventDispatcher _eventDispatcher;
        private readonly Dictionary<string, List<String[]>> _ordersForReview;

        public OrderReviewer(EventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
            _eventDispatcher.updateActionToTakeRequestHandler += UpdateActionToTakeListener;
            _ordersForReview = new Dictionary<string, List<string[]>>();
            _actionsToTake = new Dictionary<string, string>();
        }

        public List<String[]> GetOrdersForReviewEntry(string character, string typeId, int buyOrSell)
        {
            string key = GenerateKey(character, int.Parse(typeId), buyOrSell);
            return _ordersForReview.ContainsKey(key) ? _ordersForReview[key] : null;
        }

        public string GetAction(string character, string typeId, int buyOrSell)
        {
            string key = GenerateKey(character, int.Parse(typeId), buyOrSell);
            return _actionsToTake.ContainsKey(key) ? _actionsToTake[key] : "";
        }

        public double GetPrice(string character, int typeId, int buyOrSell)
        {
            string key = GenerateKey(character, typeId, buyOrSell);

            return buyOrSell.Equals(EtConstants.Sell)
                       ? Double.Parse(_ordersForReview[key][1][1])
                       : (from orderRow in _ordersForReview[key]
                          where orderRow[0].Equals("Buy")
                          select Double.Parse(orderRow[1])).FirstOrDefault();
        }

        public string GetItemName(string character, string typeId, int buyOrSell)
        {
            string key = GenerateKey(character, int.Parse(typeId), buyOrSell);
            return _ordersForReview.ContainsKey(key) ? _ordersForReview[key][0][1] : "";
        }

        private void UpdateActionToTakeListener(Object o, string character, string typeid, string buyOrSell,
                                                string action)
        {
            string key = GenerateKey(character, int.Parse(typeid), int.Parse(buyOrSell));
            if (_actionsToTake.ContainsKey(key))
            {
                _actionsToTake.Remove(key);
            }
            _actionsToTake.Add(key, action);
        }

        public List<string> GetActionsToTake()
        {
            return _actionsToTake.Keys.ToList();
        }

        public void AddOrderRequiringReview(string stationId, List<string[]> orderData, string ownedPrice,
                                            string characterName, string itemName)
        {
            int typeId = int.Parse(orderData[0][2]);
            var printableOrderInfo = new List<string[]>();
            string key = GenerateKey(characterName, typeId, -1);
            printableOrderInfo.Add(new[] {characterName, itemName, typeId.ToString()});

            foreach (var orderRow in orderData)
            {
                if (IsCompetingOrder(Convert.ToBoolean(orderRow[7]), Convert.ToInt32(orderRow[3]),
                                     Convert.ToInt32(orderRow[13]), orderRow[10], stationId))
                {
                    if (orderRow[0].Equals(ownedPrice))
                    {
                        key = GenerateKey(characterName, typeId, BuyOrSellToInt(orderRow[7]));
                        printableOrderInfo.Add(new[]
                        {
                            "OWNED " + PrintableBuyOrSell(orderRow[7]), orderRow[0], orderRow[1]
                            , GetPrintableTimeLeft(orderRow[8], orderRow[9])
                        });
                    }
                    else
                    {
                        printableOrderInfo.Add(new[]
                        {
                            PrintableBuyOrSell(orderRow[7]), orderRow[0], orderRow[1],
                            GetPrintableTimeLeft(orderRow[8], orderRow[9])
                        });
                    }
                }
            }
            if (_ordersForReview.ContainsKey(key))
            {
                _ordersForReview.Remove(key);
            }
            _ordersForReview.Add(key, printableOrderInfo);
            _actionsToTake.Add(key, "Ignore");
        }

        private static string GenerateKey(string characterName, int typeId, int buyOrSell)
        {
            return (characterName + "," + typeId + "," + buyOrSell.ToString());
        }

        public void RemoveOrderRequiringReview(string character, int typeId, int buyOrSell)
        {
            string key = GenerateKey(character, typeId, buyOrSell);
            if (_ordersForReview.ContainsKey(key))
            {
                _ordersForReview.Remove(key);
                _actionsToTake.Remove(key);
            }
        }

        public Boolean ShouldCancel(string characterName, int typeId, int buyOrSell)
        {
            string key = GenerateKey(characterName, typeId, buyOrSell);
            return _actionsToTake.ContainsKey(key) && _actionsToTake[key].Equals("Cancel");
        }

        public Boolean ShouldUpdate(string characterName, int typeId, int buyOrSell)
        {
            string key = GenerateKey(characterName, typeId, buyOrSell);
            return _actionsToTake.ContainsKey(key) && _actionsToTake[key].Equals("Update");
        }

        public Boolean ShouldIgnore(string characterName, int typeId, int buyOrSell)
        {
            string key = GenerateKey(characterName, typeId, buyOrSell);
            return _actionsToTake.ContainsKey(key) && _actionsToTake[key].Equals("Ignore");
        }

        private static int BuyOrSellToInt(String isBidColumn)
        {
            Boolean isBuyOrder = Convert.ToBoolean(isBidColumn);
            return isBuyOrder ? EtConstants.Buy : EtConstants.Sell;
        }

        private static String PrintableBuyOrSell(String isBidColumn)
        {
            Boolean isBuyOrder = Convert.ToBoolean(isBidColumn);
            return isBuyOrder ? "Buy" : "Sell";
        }

        private static String GetPrintableTimeLeft(String issueDate, String duration)
        {
            issueDate = issueDate.Substring(0, issueDate.Length - 7);
            var issueDateDt = new DateTime();
            long durationTicks = new DateTime().AddDays(int.Parse(duration)).Ticks;

            int value = int.Parse(issueDate.Substring(0, 4));
            issueDateDt = issueDateDt.AddYears(value - 1);

            value = int.Parse(issueDate.Substring(5, 2));
            issueDateDt = issueDateDt.AddMonths(value - 1);

            value = int.Parse(issueDate.Substring(8, 2));
            issueDateDt = issueDateDt.AddDays(value - 1);

            value = int.Parse(issueDate.Substring(11, 2));
            issueDateDt = issueDateDt.AddHours(value);

            value = int.Parse(issueDate.Substring(14, 2));
            issueDateDt = issueDateDt.AddMinutes(value);

            long issueDateTicks = issueDateDt.Ticks;
            long currentDateTicks = DateTime.UtcNow.Ticks;
            long timeRemainingTicks = durationTicks - (currentDateTicks - issueDateTicks);
            double daysRemaining = timeRemainingTicks/(double) TimeSpan.TicksPerDay;
            return (daysRemaining).ToString("##.####");
        }

        private static bool IsCompetingOrder(Boolean isBuyOrder, int range, int jumps, string stationid,
                                             string targetStationid)
        {
            if (!isBuyOrder && stationid.Equals(targetStationid))
                return true;
            if (isBuyOrder && (range == -1 && stationid.Equals(targetStationid) || range - jumps >= 0))
                //If this is my station or the order is in range
                return true;
            return false;
        }
    }
}