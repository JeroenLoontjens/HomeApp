using System.Windows.Input;
using DataAccess.Model;
using AppForLogin.ViewModel;
using Microsoft.VisualBasic;

namespace AppForLogin.Views;

public partial class BudgetTransactionsPage : ContentPage
{
	public BudgetTransactionsPage(TransactionsOverViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
	}

    protected override void OnAppearing()
    {
       
        base.OnAppearing();

        if (BindingContext is TransactionsOverViewModel vm)
        {
             vm.ReLoadTransactionsCommand.Execute(null);
        }
    }
	
}