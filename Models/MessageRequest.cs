namespace MiniCRM.Api.Models
{
    public class MessageRequest
    {
        public int? ReceiverUserId { get; set; }
        public string Content { get; set; }
    }
}
