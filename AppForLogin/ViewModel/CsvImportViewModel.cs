using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataAccess.Data;
using DataAccess.Import;
using DataAccess.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace AppForLogin.ViewModel;

public partial class CsvImportViewModel : ObservableObject
{
    private readonly IDbContextFactory<BudgetDBContext> _contextFactory;
    private readonly CsvImportService _csvImportService;

    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string? geselecteerdBestand;
    [ObservableProperty] private string? importResultTekst;
    [ObservableProperty] private bool heeftResultaat;
    [ObservableProperty] private ObservableCollection<RabobankImport> pendingRijen = new();

    private Stream? _geselecteerdBestandStream;

    public CsvImportViewModel(IDbContextFactory<BudgetDBContext> contextFactory, CsvImportService csvImportService)
    {
        _contextFactory = contextFactory;
        _csvImportService = csvImportService;
        _ = LaadPendingRijen();
    }

    [RelayCommand]
    private async Task SelecteerBestand()
    {
        var options = new PickOptions
        {
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI,   new[] { ".csv" } },
                { DevicePlatform.Android, new[] { "text/csv", "text/comma-separated-values" } },
                { DevicePlatform.iOS,     new[] { "public.comma-separated-values-text" } },
            })
        };

        var result = await FilePicker.Default.PickAsync(options);
        if (result != null)
        {
            GeselecteerdBestand = result.FileName;
            _geselecteerdBestandStream = await result.OpenReadAsync();
            ImportResultTekst = null;
            HeeftResultaat = false;
        }
    }

    [RelayCommand]
    private async Task Importeer()
    {
        if (_geselecteerdBestandStream == null)
            return;

        IsBusy = true;
        var streamToDispose = _geselecteerdBestandStream;
        _geselecteerdBestandStream = null;
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var result = await _csvImportService.ImportAsync(streamToDispose, context);

            ImportResultTekst = $"✅ {result.Imported} geïmporteerd   ⚠️ {result.Duplicates} duplicaten   📊 {result.Total} totaal";
            HeeftResultaat = true;

            GeselecteerdBestand = null;

            await LaadPendingRijen();
        }
        catch (Exception ex)
        {
            if (Application.Current?.MainPage is Page page)
                await page.DisplayAlert("Fout", ex.Message, "OK");
        }
        finally
        {
            await (streamToDispose?.DisposeAsync() ?? ValueTask.CompletedTask);
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task LaadPendingRijen()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var rijen = await context.RabobankImports
            .Where(r => r.Status == RabobankImportStatus.Pending || r.Status == RabobankImportStatus.Duplicate)
            .OrderByDescending(r => r.Datum)
            .ToListAsync();

        PendingRijen.Clear();
        foreach (var rij in rijen)
            PendingRijen.Add(rij);
    }
}
