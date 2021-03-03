using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace react_app.Lomag.Entities
{
    [Table(nameof(Uzytkownik))]
    public class Uzytkownik
    {
        [Key]
        public int IdUzytkownika { get; set; }
    }
}
