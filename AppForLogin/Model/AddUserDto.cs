namespace AppForLogin.Model
{
    public class AddUserDto
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public string Password { get; set; } // plain text input
        public string Phone { get; set; }
        public string Role { get; set; }

        public List<LicenseDto> Licenses { get; set; }
    }
}
