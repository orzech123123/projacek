using System.Collections.Generic;
using System.Threading.Tasks;

namespace react_app.Services
{
    public interface IOrderProvider
    {
        string GenerateUrl(string orderId);
        OrderProviderType Type { get; }
        Task<IEnumerable<OrderDto>> GetOrders();
    }

    public enum OrderProviderType
    {
        Allegro,
        Apaczka,
        Apilo
    }
}
