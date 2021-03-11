using System;
using System.Collections.Generic;

namespace react_app.Apaczka
{
    public class ApaczkaOrdersResponse
    {
        public ApaczkaOrdersContainer Response { get; set; }
    }

    public class ApaczkaOrdersContainer
    {
        public List<ApaczkaOrder> Orders { get; set; }
    }

    public class ApaczkaOrder
    {
        public string Id { get; set; }
        public string ServiceName { get; set; }
        public string Content { get; set; }
        public string Comment { get; set; }
        public DateTime Created { get; set; }
    }
}
