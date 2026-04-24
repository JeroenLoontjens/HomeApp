using AppForLogin.ViewModel;

namespace AppForLogin.Views;

public partial class TransactionChartPage : ContentPage
{
    private readonly TransactionChartPageViewModel _viewModel;

    public TransactionChartPage(TransactionChartPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _viewModel.LoadAsync();
    }
}
