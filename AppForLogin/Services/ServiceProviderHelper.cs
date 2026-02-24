using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppForLogin.Services
{
    public static class ServiceProviderHelper
    {
        public static IServiceProvider Services { get; set; }
        public static T GetService<T>() => Services.GetRequiredService<T>();
    }
}
