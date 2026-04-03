using AppForLogin.ViewModel;

namespace AppForLogin.Views;

public partial class CategorieDetailPage : ContentPage
{
	public CategorieDetailPage(CategoriesDetailViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}