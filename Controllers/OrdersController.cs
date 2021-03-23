using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using react_app.Extensions;
using react_app.Services;
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

        [HttpPost]
        public string StopSync()
        {
            return DateTime.Now.ToString();
        }

        [HttpGet]
        public object GetSyncs(bool isDescending = true, string orderBy = nameof(Order.Date), int pageSize = 10, int pageIndex = 0, string filter = null)
        {
            filter = filter ?? string.Empty;

            var skip = pageIndex * pageSize;
            var take = pageSize;

            IQueryable<Order> query = Orders(filter);
            query = query.OrderBy(orderBy, isDescending);
            var syncs = query
                .Skip(skip)
                .Take(take)
                .ToList();

            var count = Orders(filter).Count();

            return new
            {
                syncs,
                count
            };
        }

        private IQueryable<Order> Orders(string filter) {
            var isDate = DateTime.TryParse(filter, out DateTime date);
            var isProviderType = Enum.TryParse(filter, out OrderProvider providerType) && Enum.IsDefined(typeof(OrderProvider), filter);

            if(isDate)
            {
                return wmprojackDbContext.Orders.Where(o => o.Date.Date == date.Date);
            }

            if (isProviderType)
            {
                return wmprojackDbContext.Orders.Where(o => o.ProviderType == providerType);
            }

            return wmprojackDbContext.Orders
                .Where(o =>
                    o.Name.ToLower().Contains(filter.ToLower()) ||
                    o.ProviderOrderId.ToLower().Contains(filter.ToLower()) ||
                    o.Code.ToLower().Contains(filter.ToLower()));
        }
    }
}
