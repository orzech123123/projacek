using System.Collections.Generic;

namespace react_app.Services
{
    public interface IOrderProvider
    {
        string GenerateUrl(string orderId);
        OrderProvider Type { get; }
        IEnumerable<OrderDto> GetOrders();
    }

    public enum OrderProvider
    {
        Allegro,
        Apaczka
    }
}
