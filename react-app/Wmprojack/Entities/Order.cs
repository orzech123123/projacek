using System;
using System.ComponentModel.DataAnnotations;

namespace react_app.Wmprojack.Entities
{
    public class Order
    {
        public string Id { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string ProviderId { get; set; }
        [Required]
        public OrderProvider Type { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Code { get; set; }
    }

    public enum OrderProvider
    {
        Allegro,
        Apaczka
    }
}
