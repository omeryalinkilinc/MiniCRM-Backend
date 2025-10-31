namespace MiniCRM.Api.Models
{

    public class Message
    {
        public int Id { get; set; }
        public int SenderUserId { get; set; }      // Gönderen (admin veya kullanýcý)
        public int? ReceiverUserId { get; set; }   // Alýcý (null ise tüm kullanýcýlara gönderilmiþ demektir)
        public string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

        public User SenderUser { get; set; }
        public User? ReceiverUser { get; set; }
    }


}