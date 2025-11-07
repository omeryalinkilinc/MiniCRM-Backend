namespace MiniCRM.Api.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime CreatedAt { get; set; } // ✅ Eklendi
        public int UserId { get; set; }
        public User User { get; set; }
    }


}