using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppForLogin.Model
{
    public class User
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public string Password { get; set; }  // Note this is stored hashed 
        public string Phone { get; set; }
        public string Role { get; set; }

        public List<License> Licenses { get; set; } = new();
    }
}
