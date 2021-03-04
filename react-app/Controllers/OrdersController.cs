using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using react_app.Lomag;
using react_app.Services;
using react_app.Wmprojack;

namespace react_app.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly WmprojackDbContext wmprojackDbContext;

        public OrdersController(
            IEnumerable<IOrderProvider> orderProviders,
            LomagDbContext lomagDbContext,
            WmprojackDbContext wmprojackDbContext)
        {
            this.wmprojackDbContext = wmprojackDbContext;
        }

        [HttpGet]
        public object GetSyncs()
        {
            var allOrders = wmprojackDbContext.Orders.OrderByDescending(o => o.Date).ThenBy(o => o.ProviderOrderId).ToList();

            return new
            {
                syncs = allOrders
            };
        }
    }
}
