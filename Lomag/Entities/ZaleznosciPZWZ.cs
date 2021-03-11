using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace react_app.Lomag.Entities
{
    [Table(nameof(ZaleznosciPZWZ))]
    public class ZaleznosciPZWZ
    {
        [Key]
        public int IdZaleznosciPZWZ { get; set; }
        public decimal Ilosc { get; set; }

        [Column("IdElementuPZ")]
        public int ElementPZId { get; set; }
        public ElementRuchuMagazynowego ElementPZ { get; set; }

        [Column("IdElementuWZ")]
        public int ElementWZId { get; set; }
        public ElementRuchuMagazynowego ElementWZ { get; set; }
    }
}
