using System;

namespace react_app
{
    public class Order
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public OrderProvider Type { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }

    public enum OrderProvider
    {
        Allegro,
        Apaczka
    }
}
