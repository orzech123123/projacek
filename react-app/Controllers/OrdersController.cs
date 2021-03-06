using System.Linq;
using Microsoft.AspNetCore.Mvc;
using react_app.Extensions;
using react_app.Wmprojack;
using react_app.Wmprojack.Entities;

namespace react_app.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly WmprojackDbContext wmprojackDbContext;

        public OrdersController(WmprojackDbContext wmprojackDbContext)
        {
            this.wmprojackDbContext = wmprojackDbContext;
        }

        [HttpGet]
        public object GetSyncs(bool isDescending = true, string orderBy = nameof(Order.Date), int pageSize = 10, int pageIndex = 0)
        {
            var skip = pageIndex * pageSize;
            var take = pageSize;

            IQueryable<Order> query = wmprojackDbContext.Orders;
            query = query.OrderBy(orderBy, isDescending);
            var syncs = query
                .Skip(skip)
                .Take(take)
                .ToList();

            var count = wmprojackDbContext.Orders.Count();

            return new
            {
                syncs,
                count
            };
        }
    }
}
