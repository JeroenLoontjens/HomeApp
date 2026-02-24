using System.ComponentModel;

namespace WEB_USER_API.Models.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public  string Password { get; set; } // Note this is stored hashed
        public string Phone { get; set; }
        public string Role { get; set; }

        public ICollection<License> Licenses { get; set; } = new List<License>();

    }
}
