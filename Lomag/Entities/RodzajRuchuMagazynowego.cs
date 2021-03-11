using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace react_app.Lomag.Entities
{
    [Table(nameof(RodzajRuchuMagazynowego))]
    public class RodzajRuchuMagazynowego
    {
        [Key]
        public int IdRodzajuRuchuMagazynowego { get; set; }
        public string Nazwa { get; set; }
    }
}
