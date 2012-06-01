namespace noxiousET.src.model.orders
{
    class Order
    {
        private string orderID;
        private string typeID;
        private string station;
        private int range;
        private double price;
        private int volume;
        private int runs;

        public Order()
        {
        }

        public Order(string orderID, string typeID, string station, int range, double price, int volume)
        {
            this.orderID = orderID;
            this.typeID = typeID;
            this.station = station;
            this.range = range;
            this.price = price;
            this.volume = volume;
            this.runs = 0;
        }

        public string getOrderID()
        {
            return this.orderID;
        }

        public string getStation()
        {
            return this.station;
        }

        public string getTypeID()
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
