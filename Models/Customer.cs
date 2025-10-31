namespace MiniCRM.Api.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public required string Name { get; set; }
        public required string Company { get; set; }
        public required string CustomerType { get; set; }

        public int? TransactionCount { get; set; } // Nullable yapýldý
        public DateTime RegistrationDate { get; set; }
        public string? PhotoUrl { get; set; }

        public User User { get; set; }
    }
}
