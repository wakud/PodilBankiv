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
        public Banks(string fileName, DateTime dat, int kb, string bank, int kil, decimal suma)
        {
            FileNameDate = fileName;
            Dat = dat;
            Kb = kb;
            Bank = bank;
            Kil = kil;
            Suma = suma;
        }

    }

    public class Abons
    {
        public DateTime? Dat { get; set; }
        public int? Numbpers { get; set; }
        public decimal? Suma { get; set; }
        public string Poppok { get; set; }
        public int? kb { get; set; }

        public Abons(DateTime dat, int numbers, decimal suma, string poppok, int kb)
        {
            Dat = dat;
            Numbpers = numbers;
            Suma = suma == null ? 0 : suma;
            Poppok = poppok;
            kb = kb;
        }
    }
}
