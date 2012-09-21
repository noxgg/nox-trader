using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using noxiousET.src.orders;
namespace noxiousET.src.control
{
    class OrderReviewInfoProvider
    {
        OrderReviewer orderReviewer;

        public OrderReviewInfoProvider(OrderReviewer orderReviewer)
        {
            this.orderReviewer = orderReviewer;
        }

        public List<String[]> getOrdersRequiringReview()
        {
            List<String[]> result = new List<String[]>();
            List<String> actionsToTake = orderReviewer.getActionsToTake();
            string[] split;
            foreach (string s in actionsToTake)
            {
                split = s.Split(',');
                string[] newEntry = new string[5];
                newEntry[2] = split[0];
                newEntry[4] = split[1];
                newEntry[0] = split[2];
                newEntry[1] = orderReviewer.getAction(split[0], split[1], int.Parse(split[2]));
                newEntry[3] = orderReviewer.getItemName(split[0], split[1], int.Parse(split[2]));
                result.Add(newEntry);
            }
            return result;
        }

        public List<String[]> getItemDetails(string character, string typeId, string buyOrSell)
        {
            if (buyOrSell.Equals("B"))
                buyOrSell = EtConstants.BUY.ToString();
            else
                buyOrSell = EtConstants.SELL.ToString();
            return orderReviewer.getOrdersForReviewEntry(character, typeId, int.Parse(buyOrSell));
        }
    }
}
