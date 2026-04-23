using CsvHelper.Configuration.Attributes;

namespace DataAccess.Import
{
    public class RabobankCsvRow
    {
        [Name("IBAN/BBAN")]
        public string IBAN { get; set; } = string.Empty;

        [Name("Volgnr")]
        public string Volgnr { get; set; } = string.Empty;

        [Name("Datum")]
        public string Datum { get; set; } = string.Empty;

        [Name("Bedrag")]
        public string Bedrag { get; set; } = string.Empty;

        [Name("Saldo na trn")]
        public string SaldoNaTrn { get; set; } = string.Empty;

        [Name("Tegenrekening IBAN/BBAN")]
        public string? TegenrekeningIBAN { get; set; }

        [Name("Naam tegenpartij")]
        public string? NaamTegenpartij { get; set; }

        [Name("Code")]
        public string? Code { get; set; }

        [Name("Omschrijving-1")]
        public string? Omschrijving1 { get; set; }

        [Name("Omschrijving-2")]
        public string? Omschrijving2 { get; set; }

        [Name("Omschrijving-3")]
        public string? Omschrijving3 { get; set; }
    }
}
