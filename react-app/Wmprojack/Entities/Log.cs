using System;

namespace react_app.Wmprojack.Entities
{
    public class Log
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string Properties { get; set; }
        public string Level { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
