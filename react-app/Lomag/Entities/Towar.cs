using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace react_app.Lomag.Entities
{
    [Table("Towar")]
    public class Towar
    {
        [Key]
        public int IdTowaru { get; set; }
        public string Nazwa { get; set; }
    }
}
