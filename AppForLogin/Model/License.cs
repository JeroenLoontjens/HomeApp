using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppForLogin.Model
{
    public class License
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public DateTime? ExpirationDate { get; set; }


        
    }
}
