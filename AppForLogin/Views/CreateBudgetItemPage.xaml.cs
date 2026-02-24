using AppForLogin.ViewModel;

namespace AppForLogin.Views
{
    public partial class CreateBudgetItemPage : ContentPage
    {
        public CreateBudgetItemPage(CreateBudgetItemViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}