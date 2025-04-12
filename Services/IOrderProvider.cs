using System.Collections.Generic;

namespace react_app.Services
{
    public interface IOrderProvider
    {
        string GenerateUrl(string orderId);
        OrderProviderType Type { get; }
        IEnumerable<OrderDto> GetOrders();
    }

    public enum OrderProviderType
    {
        Allegro,
        Apaczka,
        Apilo
    }
}
