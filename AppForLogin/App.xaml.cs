using AppForLogin.Model;
using AppForLogin.Views;

namespace AppForLogin
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

           
        }

        protected override Window CreateWindow(IActivationState? activationState )
        {
            Page rootPage;
            if (user == null)
            {
                rootPage = Services.ServiceProviderHelper.GetService<LoginPage>()!;
            }
            else
            {
                rootPage = new AppShell(user);
            }

            return new Window(rootPage);
        }


        // existing static user field is used elsewhere in your project:
        public static AppForLogin.Model.User? user;
    }
}
