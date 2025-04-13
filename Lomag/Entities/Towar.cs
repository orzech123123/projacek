using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace react_app.Lomag.Entities
{
    [Table(nameof(Towar))]
    public class Towar
    {
        [Key]
        public int IdTowaru { get; set; }
        public string Nazwa { get; set; }
        public string KodKreskowy { get; set; }
        public decimal StanMinimalny { get; set; }

        public int IdMagazynu { get; set; }
    }
}
