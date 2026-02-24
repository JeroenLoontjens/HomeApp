using AppForLogin.ViewModel;
using AppForLogin.Views.Generic;

namespace AppForLogin.Views;

public partial class CategoriesPage : ContentPage
{
	public CategoriesPage(CategoriesPageViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}