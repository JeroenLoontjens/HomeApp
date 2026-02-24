namespace WEB_USER_API.Models.Entities
{
    public class License
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public DateTime? ExpirationDate { get; set; }

        // foreign key
        
        
        
    }
}
