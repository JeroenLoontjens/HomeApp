namespace AppForLogin.Model
{
    public class LicenseDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}
