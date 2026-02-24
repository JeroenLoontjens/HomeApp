using System;
using System.Collections.Generic;
using System.Text;

namespace AppForLogin.Model
{
    public class LoginDto
    {

        public required string Email { get; set; }
        public required string Password { get; set; } // plain text input
    }
}
