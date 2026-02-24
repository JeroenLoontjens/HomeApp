namespace WEB_USER_API.Models
{
    public class LicenseDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}
