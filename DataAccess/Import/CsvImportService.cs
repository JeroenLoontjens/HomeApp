using CsvHelper;
using CsvHelper.Configuration;
using DataAccess.Data;
using DataAccess.Model;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace DataAccess.Import
{
    public record ImportResult(int Imported, int Duplicates, int Total);

    public class CsvImportService
    {
        private static readonly CultureInfo DutchCulture = new("nl-NL");

        public List<RabobankCsvRow> ParseCsv(Stream fileStream)
        {
            var config = new CsvConfiguration(DutchCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true,
                MissingFieldFound = null,
            };

            using var reader = new StreamReader(fileStream, leaveOpen: true);
            using var csv = new CsvReader(reader, config);

            return csv.GetRecords<RabobankCsvRow>().ToList();
        }

        public List<RabobankImport> MapToImportRows(List<RabobankCsvRow> csvRows)
        {
            var importedAt = DateTime.UtcNow;

            return csvRows.Select((row, index) =>
            {
                if (!DateTime.TryParse(row.Datum, DutchCulture, DateTimeStyles.None, out var datum))
                    throw new FormatException($"Row {index + 1} (Volgnr={row.Volgnr}): invalid Datum value '{row.Datum}'.");

                if (!decimal.TryParse(row.Bedrag, NumberStyles.Number, DutchCulture, out var bedrag))
                    throw new FormatException($"Row {index + 1} (Volgnr={row.Volgnr}): invalid Bedrag value '{row.Bedrag}'.");

                if (!decimal.TryParse(row.SaldoNaTrn, NumberStyles.Number, DutchCulture, out var saldoNaTrn))
                    throw new FormatException($"Row {index + 1} (Volgnr={row.Volgnr}): invalid SaldoNaTrn value '{row.SaldoNaTrn}'.");

                var omschrijvingParts = new[] { row.Omschrijving1, row.Omschrijving2, row.Omschrijving3 }
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s!.Trim());

                return new RabobankImport
                {
                    Volgnr = row.Volgnr,
                    IBAN = row.IBAN,
                    Datum = datum,
                    Bedrag = bedrag,
                    SaldoNaTrn = saldoNaTrn,
                    TegenrekeningIBAN = string.IsNullOrWhiteSpace(row.TegenrekeningIBAN) ? null : row.TegenrekeningIBAN.Trim(),
                    NaamTegenpartij = string.IsNullOrWhiteSpace(row.NaamTegenpartij) ? null : row.NaamTegenpartij.Trim(),
                    Omschrijving = string.Join(" ", omschrijvingParts) is { Length: > 0 } s ? s : null,
                    Code = string.IsNullOrWhiteSpace(row.Code) ? null : row.Code.Trim(),
                    Status = RabobankImportStatus.Pending,
                    ImportedAt = importedAt,
                };
            }).ToList();
        }

        public async Task<ImportResult> ImportAsync(Stream fileStream, BudgetDBContext context)
        {
            var csvRows = ParseCsv(fileStream);
            var importRows = MapToImportRows(csvRows);

            var total = importRows.Count;
            var duplicates = 0;
            var newRows = new List<RabobankImport>();

            var incomingVolgnrs = importRows.Select(r => r.Volgnr).ToHashSet();
            var existingVolgnrs = await context.RabobankImports
                .Where(r => incomingVolgnrs.Contains(r.Volgnr))
                .Select(r => r.Volgnr)
                .ToHashSetAsync();

            foreach (var row in importRows)
            {
                if (existingVolgnrs.Contains(row.Volgnr))
                {
                    duplicates++;
                }
                else
                {
                    newRows.Add(row);
                }
            }

            if (newRows.Count > 0)
            {
                await context.RabobankImports.AddRangeAsync(newRows);
                await context.SaveChangesAsync();
            }

            return new ImportResult(Imported: newRows.Count, Duplicates: duplicates, Total: total);
        }
    }
}
