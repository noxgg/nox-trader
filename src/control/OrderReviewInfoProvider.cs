using System;
using System.Collections.Generic;
using noxiousET.src.orders;

namespace noxiousET.src.control
{
    internal class OrderReviewInfoProvider
    {
        private readonly OrderReviewer _orderReviewer;

        public OrderReviewInfoProvider(OrderReviewer orderReviewer)
        {
            _orderReviewer = orderReviewer;
        }

        public List<String[]> GetOrdersRequiringReview()
        {
            var result = new List<String[]>();
            List<String> actionsToTake = _orderReviewer.GetActionsToTake();
            foreach (string s in actionsToTake)
            {
                //TODO Magic number removal
                string[] split = s.Split(',');
                var newEntry = new string[5];
                newEntry[2] = split[0];
                newEntry[4] = split[1];
                newEntry[0] = split[2];
                newEntry[1] = _orderReviewer.GetAction(split[0], split[1], Convert.ToBoolean(split[2]));
                newEntry[3] = _orderReviewer.GetItemName(split[0], split[1], Convert.ToBoolean(split[2]));
                result.Add(newEntry);
            }
            return result;
        }

        public List<String[]> GetItemDetails(string character, string typeId, string buyOrSell)
        {
            return _orderReviewer.GetOrdersForReviewEntry(character, typeId, buyOrSell.Equals("B"));
        }
    }
}