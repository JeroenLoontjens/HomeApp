using AppForLogin.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppForLogin.Services
{
    public interface IloginRepository
    {
        Task<LoginResponse> Login(string email, string password);
        void SetAuthHeader(string token);

    }
}
