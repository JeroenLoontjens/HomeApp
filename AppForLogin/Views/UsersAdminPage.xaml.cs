using System;
using Microsoft.Maui.Controls;
using AppForLogin.ViewModel;


namespace AppForLogin.Views
{
    public partial class UsersAdminPage : ContentPage
    {
        public UsersAdminPage(UsersAdminViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;


        }

        
    }
}