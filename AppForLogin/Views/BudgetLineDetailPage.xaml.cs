using AppForLogin.ViewModel;

namespace AppForLogin.Views;

public partial class BudgetLineDetailPage : ContentPage
{
    public BudgetLineDetailPage(BudgetLineDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
