using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace react_app.Lomag.Entities
{
    [Table(nameof(RuchMagazynowy))]
    public class RuchMagazynowy
    {
        [Key]
        public int IdRuchuMagazynowego { get; set; }

        [Column("IDRodzajuRuchuMagazynowego")]
        public int? RodzajRuchuMagazynowegoId { get; set; }
        public RodzajRuchuMagazynowego? RodzajRuchuMagazynowego { get; set; }

        public DateTime? Data { get; set; }
        public string Uwagi { get; set; }
        public DateTime? Utworzono { get; set; }
        public DateTime? Zmodyfikowano { get; set; }

        [Column("IDMagazynu")]
        public int? MagazynId { get; set; }
        public Magazyn? Magazyn { get; set; }

        public string NrDokumentu { get; set; }

        [Column("IDKontrahenta")]
        public int? KontrahentId { get; set; }
        public Kontrahent? Kontrahent { get; set; }

        [Column("IDUzytkownika")]
        public int? UzytkownikId { get; set; }
        public Uzytkownik? Uzytkownik { get; set; }

        public int Operator { get; set; }

        [Column("IDCompany")]
        public int? CompanyId { get; set; }
        public Kontrahent? Company { get; set; }
    }
}
