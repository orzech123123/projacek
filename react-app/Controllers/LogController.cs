using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using react_app.Extensions;
using react_app.Wmprojack;
using react_app.Wmprojack.Entities;

namespace react_app.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LogController : ControllerBase
    {
        private readonly WmprojackDbContext wmprojackDbContext;

        public LogController(WmprojackDbContext wmprojackDbContext)
        {
            this.wmprojackDbContext = wmprojackDbContext;
        }

        [HttpGet]
        public object GetLogs(bool isDescending = true, string orderBy = nameof(Log.TimeStamp), int pageSize = 10, int pageIndex = 0)
        {
            var skip = pageIndex * pageSize;
            var take = pageSize;

            IQueryable<Log> query = Logs;
            query = query.OrderBy(orderBy, isDescending);
            var logs = query
                .Skip(skip)
                .Take(take)
                .ToList();

            var count = Logs.Count();

            return new
            {
                logs,
                count
            };
        }

        private IQueryable<Log> Logs => wmprojackDbContext.Logs.FromSqlRaw(@"
                SELECT TOP (1000) [Id] ,[Message], [Timestamp], [Properties], [Level]
                FROM [WmProJack].[dbo].[LogEvents]
                order by [TimeStamp] desc")
            .Where(l => l.Properties.Contains("react_app") || new[] { "Warning", "Error" , "Fatal" }.Contains(l.Level));
    }
}
