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
        private const int OrderInfoBuyOrSell = 0;
        private const int OrderInfoPrice = 1;
        private const int OrderInfoVolumeRemaining = 2;
        private const int OrderInfoTimeLeft = 3;
        private const int OrderInfoMetaRow = 0;
        private const int OrderInfoFirstDataRow = 1;
        private const int MetaDataCharacter = 0;
        private const int MetaDataTypeName = 1;
        private const int MetaDataTypeId = 2;

        public OrderReviewer(EventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
            _eventDispatcher.updateActionToTakeRequestHandler += UpdateActionToTakeListener;
            _ordersForReview = new Dictionary<string, List<string[]>>();
            _actionsToTake = new Dictionary<string, string>();
        }

        public List<String[]> GetOrdersForReviewEntry(string character, string typeId, bool isBuy)
        {
            string key = GenerateKey(character, int.Parse(typeId), isBuy);
            return _ordersForReview.ContainsKey(key) ? _ordersForReview[key] : null;
        }

        public string GetAction(string character, string typeId, bool isBuy)
        {
            string key = GenerateKey(character, int.Parse(typeId), isBuy);
            return _actionsToTake.ContainsKey(key) ? _actionsToTake[key] : "";
        }

        public double GetPrice(string character, int typeId, bool isBuy)
        {
            string key = GenerateKey(character, typeId, isBuy);

            return isBuy
                       ? (from orderRow in _ordersForReview[key]
                          where orderRow[OrderInfoBuyOrSell].Equals("Buy")
                          select Double.Parse(orderRow[OrderInfoPrice])).FirstOrDefault()
                       : Double.Parse(_ordersForReview[key][OrderInfoFirstDataRow][OrderInfoPrice]);
        }

        public string GetItemName(string character, string typeId, bool isBuy)
        {
            string key = GenerateKey(character, int.Parse(typeId), isBuy);
            return _ordersForReview.ContainsKey(key) ? _ordersForReview[key][OrderInfoMetaRow][MetaDataTypeName] : "";
        }

        private void UpdateActionToTakeListener(Object o, string character, string typeid, bool isBuy,
                                                string action)
        {
            string key = GenerateKey(character, int.Parse(typeid), isBuy);
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
            int typeId = int.Parse(orderData[0][EtConstants.OrderDataColumnTypeId]);
            var printableOrderInfo = new List<string[]>();
            string key = GenerateKey(characterName, typeId, false);
            printableOrderInfo.Add(new[] {characterName, itemName, typeId.ToString()});

            foreach (var orderRow in orderData)
            {
                if (IsCompetingOrder(orderRow, stationId))
                {
                    string printableBuyOrSell = PrintableBuyOrSell(orderRow[EtConstants.OrderDataColumnIsBuyOrder]);

                    if (orderRow[EtConstants.OrderDataColumnPrice].Equals(ownedPrice))
                    {
                        key = GenerateKey(characterName, typeId, Convert.ToBoolean(orderRow[EtConstants.OrderDataColumnIsBuyOrder]));
                        printableBuyOrSell = "OWNED " + printableBuyOrSell;
                    }
                    printableOrderInfo.Add(new[]
                    {
                        printableBuyOrSell, 
                        orderRow[EtConstants.OrderDataColumnPrice], 
                        orderRow[EtConstants.OrderDataColumnVolumeRemaining],
                        GetPrintableTimeLeft(orderRow[EtConstants.OrderDataColumnIssueDate], orderRow[EtConstants.OrderDataColumnDuration])
                    });
                }
            }
            if (_ordersForReview.ContainsKey(key))
            {
                _ordersForReview.Remove(key);
            }
            _ordersForReview.Add(key, printableOrderInfo);
            _actionsToTake.Add(key, "Ignore");
        }

        private static string GenerateKey(string characterName, int typeId, bool isBuy)
        {
            return (characterName + "," + typeId + "," + isBuy);
        }

        public void RemoveOrderRequiringReview(string character, int typeId, bool isBuy)
        {
            string key = GenerateKey(character, typeId, isBuy);
            if (_ordersForReview.ContainsKey(key))
            {
                _ordersForReview.Remove(key);
                _actionsToTake.Remove(key);
            }
        }

        public Boolean ShouldCancel(string characterName, int typeId, bool isBuy)
        {
            string key = GenerateKey(characterName, typeId, isBuy);
            return _actionsToTake.ContainsKey(key) && _actionsToTake[key].Equals("Cancel");
        }

        public Boolean ShouldUpdate(string characterName, int typeId, bool isBuy)
        {
            string key = GenerateKey(characterName, typeId, isBuy);
            return _actionsToTake.ContainsKey(key) && _actionsToTake[key].Equals("Update");
        }

        public Boolean ShouldIgnore(string characterName, int typeId, bool isBuy)
        {
            string key = GenerateKey(characterName, typeId, isBuy);
            return _actionsToTake.ContainsKey(key) && _actionsToTake[key].Equals("Ignore");
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

        private static bool IsCompetingOrder(string[] orderRow, string targetStationid)
        {
            bool isBuyOrder = Convert.ToBoolean(orderRow[EtConstants.OrderDataColumnIsBuyOrder]);
            string stationid = orderRow[EtConstants.OrderDataColumnStationId];
            int range = Convert.ToInt32(orderRow[EtConstants.OrderDataColumnRange]);
            int jumps = Convert.ToInt32(orderRow[EtConstants.OrderDataColumnJumps]);

            if (!isBuyOrder && stationid.Equals(targetStationid))
                return true;
            if (isBuyOrder && (range == EtConstants.StationRange && stationid.Equals(targetStationid) || range - jumps >= 0))
                //If this is my station or the order is in range
                return true;
            return false;
        }
    }
}