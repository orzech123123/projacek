using react_app.Services;
using System.ComponentModel.DataAnnotations;

namespace react_app.Wmprojack.Entities
{
    public class IgnoredOrder
    {
        public string Id { get; set; }
        [Required]
        public string ProviderOrderId { get; set; }
        [Required]
        public OrderProviderType ProviderType { get; set; }
        [Required]
        public string Code { get; set; }
    }
}
