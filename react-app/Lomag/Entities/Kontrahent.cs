using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace react_app.Lomag.Entities
{
    [Table(nameof(Kontrahent))]
    public class Kontrahent
    {
        [Key]
        public int IdKontrahenta { get; set; }
        public string Nazwa { get; set; }
    }
}
