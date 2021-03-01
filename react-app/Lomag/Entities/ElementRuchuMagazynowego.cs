using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace react_app.Lomag.Entities
{
    [Table(nameof(ElementRuchuMagazynowego))]
    public class ElementRuchuMagazynowego
    {
        [Key] 
        public int IdElementuRuchuMagazynowego { get; set; }
        public decimal? Ilosc { get; set; }
        public string Uwagi { get; set; }
        public decimal? CenaJednostkowa { get; set; }
        [Column("IdRuchuMagazynowego")]
        public int? RuchMagazynowyId { get; set; }
        public RuchMagazynowy? RuchMagazynowy { get; set; }
        [Column("IdTowaru")]
        public int? TowarId { get; set; }
        public Towar? Towar { get; set; }
        public DateTime? Utworzono { get; set; }
        public DateTime? Zmodyfikowano { get; set; }
        public decimal? Wydano { get; set; }
        [Column("Uzytkownik")]
        public int? UzytkownikId { get; set; }
        public Uzytkownik? Uzytkownik { get; set; }
        public decimal? CurrencyPrice { get; set; }
        public int? Flags { get; set; }
        public decimal? Reserved { get; set; }
    }

    [Table(nameof(RuchMagazynowy))]
    public class RuchMagazynowy
    {
        [Key]
        public int IdRuchuMagazynowego { get; set; }
        [Column("IDRodzajuRuchuMagazynowego")]
        public int? RodzajRuchuMagazynowegoId { get; set; }
        public RodzajRuchuMagazynowego? RodzajRuchuMagazynowego { get; set; }
    }

    [Table(nameof(Uzytkownik))]
    public class Uzytkownik
    {
        [Key]
        public int IdUzytkownika { get; set; }
    }

    [Table(nameof(RodzajRuchuMagazynowego))]
    public class RodzajRuchuMagazynowego
    {
        [Key]
        public int IdRodzajuRuchuMagazynowego { get; set; }
        public string Nazwa { get; set; }
    }
}
