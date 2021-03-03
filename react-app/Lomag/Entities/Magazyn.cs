using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace react_app.Lomag.Entities
{
    [Table(nameof(Magazyn))]
    public class Magazyn
    {
        [Key]
        public int IdMagazynu { get; set; }
        public string Nazwa { get; set; }
    }
}
