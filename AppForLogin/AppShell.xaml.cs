using AppForLogin.Model;
using AppForLogin.Views;
using AppForLogin.Services;

namespace AppForLogin
{
    public partial class AppShell : Shell
    {
        private User? _currentUser;

        public AppShell(User? currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            UpdateHeader();

            // Register routes if you use Shell navigation by route name elsewhere
            Routing.RegisterRoute(nameof(Homepage), typeof(Homepage));
            Routing.RegisterRoute(nameof(UsersAdminPage), typeof(UsersAdminPage));
            Routing.RegisterRoute(nameof(AboutPage), typeof(AboutPage));
            Routing.RegisterRoute(nameof(ContactPage), typeof(ContactPage));
            Routing.RegisterRoute(nameof(BudgetPage), typeof(BudgetPage));
            Routing.RegisterRoute(nameof(BudgetTransactionsPage), typeof(BudgetTransactionsPage));
            Routing.RegisterRoute(nameof(CategoriesPage), typeof(CategoriesPage));
            Routing.RegisterRoute(nameof(CategorieDetailPage), typeof(CategorieDetailPage));    
            Routing.RegisterRoute(nameof(BudgetPlanPage), typeof(BudgetPlanPage));
            Routing.RegisterRoute(nameof(TransactionDetailPage), typeof(TransactionDetailPage));
            Routing.RegisterRoute(nameof(TransactionChartPage), typeof(TransactionChartPage));
            Routing.RegisterRoute(nameof(CreateBudgetItemPage), typeof(CreateBudgetItemPage));
            Routing.RegisterRoute(nameof(BudgetLineDetailPage), typeof(BudgetLineDetailPage));


            // ensure LoginPage route exists so Shell navigation can target it
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));

            if (currentUser != null && (currentUser.Role == "Ouder" || currentUser.Role == "Admin"))
            {
                Items.Add(new FlyoutItem
                {
                    Title = "User Admin",
                    Route = nameof(UsersAdminPage), // use the correct name
                    Items =
                    {
                        new ShellContent
                        {
                            ContentTemplate = new DataTemplate(() =>
                            {
                                return ServiceProviderHelper.GetService<UsersAdminPage>();
                            })
                        }

                    }
                });

            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateHeader();

            // If no user is signed in, navigate to the LoginPage at startup
            if (_currentUser is null)
            {
                 Shell.Current.GoToAsync("//LoginPage");
            }
        }

        // call this when user changes (e.g., after login) if you need immediate update
        public void UpdateHeader()
        {
            if (HeaderNameLabel == null || HeaderRoleLabel == null) return;

            if (_currentUser is null)
            {
                HeaderNameLabel.Text = "Not signed in";
                HeaderRoleLabel.Text = "Role: -";
            }
            else
            {
                HeaderNameLabel.Text = _currentUser.Name ?? "Unknown";
                HeaderRoleLabel.Text = $"Role: {_currentUser.Role ?? "-"}";
            }
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            // simple logout: clear App.user and navigate to login
            App.user = null;
            UpdateHeader();
            Application.Current.MainPage = ServiceProviderHelper.GetService<LoginPage>();
        }

    }
}
