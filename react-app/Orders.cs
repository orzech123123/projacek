using System;
using System.Collections.Generic;

namespace react_app
{
    public class OrdersResponse
    {
        public OrdersContainer Response { get; set; }
    }

    public class OrdersContainer
    {
        public List<Order> Orders { get; set; }
    }

    public class Order
    {
        public string Id { get; set; }
        public string ServiceName { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
    }
}
