namespace WEB_USER_API.Models
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public List<LicenseDto> Licenses { get; set; } = new();
    }

}
