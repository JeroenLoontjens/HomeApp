using AppForLogin.ViewModel;

namespace AppForLogin.Views;

public partial class CsvImportPage : ContentPage
{
    public CsvImportPage(CsvImportViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
