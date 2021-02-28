using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace react_app.Wmprojack.Entities
{
    public class Order
    {
        public string Id { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string ProviderOrderId { get; set; }
        [Required]
        public OrderProvider ProviderType { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Code { get; set; }

        [NotMapped]
        public int Quantity { get; set; }
        [NotMapped]
        public bool IsValid => !string.IsNullOrEmpty(ProviderOrderId) &&
            !string.IsNullOrEmpty(Name) &&
            Quantity > 0 &&
            !string.IsNullOrEmpty(Code);
    }

    public enum OrderProvider
    {
        Allegro,
        Apaczka
    }
}
