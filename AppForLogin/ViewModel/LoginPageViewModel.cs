using AppForLogin.Model;
using AppForLogin.Services;
using AppForLogin.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;

namespace AppForLogin.ViewModel
{
    public partial class LoginPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _email;
        [ObservableProperty]
        private string _password;
        [ObservableProperty]
        private bool rememberMe;

        readonly IloginRepository _loginService ;

        public LoginPageViewModel(IloginRepository loginService)
        {
            _loginService = loginService;

            if (Preferences.ContainsKey("AuthToken"))
            {
                var token =  Preferences.Get("AuthToken", string.Empty);
                if (!string.IsNullOrEmpty(token))
                {
                    // Already logged in, navigate to homepage
                    Application.Current.MainPage = new AppShell(App.user);
                    
                }
            }
        }


        [RelayCommand]
        public async Task SignIn()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current.MainPage.DisplayAlertAsync("Error", "Please enter both email and password.", "OK");
                return;
            }

            try
            {
                var  loginResponse = await _loginService.Login(Email, Password);
                App.user = loginResponse.User;

                if (RememberMe)
                {                     
                    Preferences.Set("AuthToken", loginResponse.Token);
                }
                else
                {
                    Preferences.Remove("AuthToken");
                }

                _loginService.SetAuthHeader(loginResponse.Token);

                Application.Current.MainPage = new AppShell(loginResponse.User);
                await Shell.Current.GoToAsync($"//{nameof(Homepage)}");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlertAsync("Login Failed", ex.Message, "OK");


            }
        }
    }
}
