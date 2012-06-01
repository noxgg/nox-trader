namespace noxiousET.src.orders
{
    class Order
    {
        private string orderID;
        private int typeID;
        private string stationid;
        private int range;
        private double price;
        private int volume;
        private int runs;

        public Order()
        {
        }

        public Order(string orderID, int typeID, string stationid, int range, double price, int volume)
        {
            this.orderID = orderID;
            this.typeID = typeID;
            this.stationid = stationid;
            this.range = range;
            this.price = price;
            this.volume = volume;
            this.runs = 0;
        }

        public string getOrderID()
        {
            return this.orderID;
        }

        public string getStationid()
        {
            return this.stationid;
        }

        public int getTypeID()
        {
            return this.typeID;
        }
        public int getRuns()
        {
            return this.runs;
        }

        public double getPrice()
        {
            return this.price;
        }

        public int incrementRuns()
        {
            ++this.runs;
            return 0;
        }
    };
}
