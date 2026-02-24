using AppForLogin.ViewModel;
using DataAccess.Model;

namespace AppForLogin.Views;


public partial class TransactionDetailPage : ContentPage
{
	
   
    public TransactionDetailPage(TransactionDetailViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
    }

	

	
}