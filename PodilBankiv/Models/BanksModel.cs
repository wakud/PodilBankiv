using System;

namespace PodilBankiv.Models
{
    public class Banks
    {
        public string FileNameDate { get; set; }
        public DateTime? Dat { get; set; }
        public int? Kb { get; set; }
        public string Bank { get; set; }
        public int? Kil { get; set; }
        public decimal? Suma { get; set; }
    }

    public class Abons
    {
        public DateTime? Dat { get; set; }
        public int? Numbpers { get; set; }
        public decimal? Suma { get; set; }
        public string Poppok { get; set; }
        public int? kb { get; set; }
    }
}
