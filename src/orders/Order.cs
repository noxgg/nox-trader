namespace noxiousET.src.orders
{
    internal class Order
    {
        private readonly string _orderId;
        private readonly double _price;
        private readonly string _stationId;
        private readonly int _typeId;
        private int _runs;

        public Order()
        {
        }

        public Order(int typeID)
        {
            _typeId = typeID;
            _orderId = "";
            _stationId = "";
            _price = 0;
        }

        public Order(string orderId, int typeId, string stationId, int range, double price, int volume)
        {
            _orderId = orderId;
            _typeId = typeId;
            _stationId = stationId;
            _price = price;
            _runs = 0;
        }

        public string GetOrderId()
        {
            return _orderId;
        }

        public string GetStationid()
        {
            return _stationId;
        }

        public int GetTypeId()
        {
            return _typeId;
        }

        public int GetRuns()
        {
            return _runs;
        }

        public double GetPrice()
        {
            return _price;
        }

        public int IncrementRuns()
        {
            ++_runs;
            return 0;
        }
    };
}