using System.Collections.Generic;

namespace react_app.Services
{
    public interface IOrderProvider
    {
        IEnumerable<OrderDto> GetOrders();
    }
}
