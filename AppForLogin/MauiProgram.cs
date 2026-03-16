using AppForLogin.Configuration;
using AppForLogin.Services;
using AppForLogin.ViewModel;
using AppForLogin.Views;
using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Reflection;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Hosting;




namespace AppForLogin
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            var assembly0 = Assembly.GetExecutingAssembly();
            var names = assembly0.GetManifestResourceNames();
            foreach (var name in names)
            {
                System.Diagnostics.Debug.WriteLine("RESOURCE FOUND: " + name);
            }

            // Configuratie van de databasecontext
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("AppForLogin.appsettings.json");

            builder.Configuration.AddJsonStream(stream);



            // Configureer de databaseverbinding op basis van het platform
            string connectionString;
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                // Android emulator gebruikt 10.0.2.2 om de hostmachine te bereiken
                connectionString = "Server=10.0.2.2;Port=3306;Database=budgetapp;User=budgetapp_user;Password=AppUser!2026#Secure;";
            }
            else
            {
                // Voor andere platforms (Windows, iOS, etc.) gebruik de standaard verbindingsstring
                connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            }

            builder.Services.AddDbContextFactory<BudgetDBContext>(options =>
            {
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });

            // Shell
            builder.Services.AddSingleton<AppShell>();

            // Pages 
            builder.Services.AddSingleton<Homepage>();
            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<AboutPage>();
            builder.Services.AddSingleton<ContactPage>();
            builder.Services.AddTransient<UsersAdminPage>();
            builder.Services.AddTransient<CategoriesPage>();
            builder.Services.AddTransient<TransactionDetailPage>();
            builder.Services.AddTransient<BudgetTransactionsPage>();
            builder.Services.AddTransient<BudgetPlanPage>();
            builder.Services.AddTransient<BudgetPage>();
            builder.Services.AddTransient<CreateBudgetItemPage>();
            // ViewModels
            builder.Services.AddTransient<LoginPageViewModel>();
            builder.Services.AddTransient<UsersAdminViewModel>();
            builder.Services.AddTransient<CategoriesPageViewModel>();
            builder.Services.AddTransient<TransactionsOverViewModel>();
            builder.Services.AddTransient<TransactionDetailViewModel>();
            builder.Services.AddTransient<CreateBudgetItemViewModel>();

            //Services
            // Register UserService with a configured HttpClient without requiring AddHttpClient
            builder.Services.AddSingleton(sp =>
            {
                var client = new HttpClient { BaseAddress = new Uri(ApiConstants.BaseAddress) };
                    return client;
            });

            builder.Services.AddSingleton<UserService>();
            builder.Services.AddSingleton<IloginRepository, LoginService>();
            builder.Services.AddSingleton<BudgetService>();
            builder.Services.AddSingleton<NavigationService>();

            


#if DEBUG   
            builder.Logging.AddDebug();
#endif

            var app = builder.Build(); 
            
            // heel belangrijk: hier zet je de DI-container in je helper
            ServiceProviderHelper.Services = app.Services; 
            
            return app;

        }
    }
}
