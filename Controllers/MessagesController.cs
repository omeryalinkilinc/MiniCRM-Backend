using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniCRM.Api.Data;
using MiniCRM.Api.Models;
using System.Security.Claims;

namespace MiniCRM.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly MiniCRMDbContext _context;

        public MessagesController(MiniCRMDbContext context)
        {
            _context = context;
        }

        // 1. Admin → mesaj gönder (tek kullanıcıya veya tüm kullanıcılara)
        [Authorize(Roles = "Admin")]
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] MessageRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Content))
                return BadRequest("Mesaj içeriği boş olamaz.");

            var senderId = int.Parse(User.FindFirstValue("UserId"));

            if (request.ReceiverUserId == null)
            {
                var userIds = await _context.Users.Select(u => u.Id).ToListAsync();
                var messages = userIds.Select(uid => new Message
                {
                    SenderUserId = senderId,
                    ReceiverUserId = uid,
                    Content = request.Content,
                    SentAt = DateTime.UtcNow
                }).ToList();

                _context.Messages.AddRange(messages);
            }
            else
            {
                var message = new Message
                {
                    SenderUserId = senderId,
                    ReceiverUserId = request.ReceiverUserId,
                    Content = request.Content,
                    SentAt = DateTime.UtcNow
                };

                _context.Messages.Add(message);
            }

            await _context.SaveChangesAsync();
            return Ok("Mesaj gönderildi.");
        }


        // 2. Kullanıcı → destek mesajı gönder
        [Authorize(Roles = "Customer")]
        [HttpPost("support")]
        public async Task<IActionResult> SendSupport([FromBody] MessageRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Content))
                return BadRequest("Mesaj içeriği boş olamaz.");

            var senderId = int.Parse(User.FindFirstValue("UserId"));

            // 🔥 Admin ID'yi veritabanından dinamik al
            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Role == "Admin");
            if (adminUser == null)
                return BadRequest("Sistemde tanımlı admin bulunamadı.");

            var message = new Message
            {
                SenderUserId = senderId,
                ReceiverUserId = adminUser.Id, 
                Content = request.Content,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Destek mesajı gönderildi." });

        }



        // 3. Kullanıcı → gelen mesajları listele
        [HttpGet("inbox/{userId}")]
        public async Task<IActionResult> GetInbox(int userId)
        {
            var messages = await _context.Messages
                .Include(m => m.SenderUser)
                .Where(m => m.ReceiverUserId == userId || m.SenderUserId == userId)
                .OrderByDescending(m => m.SentAt)
                .Select(m => new
                {
                    m.Id,
                    m.Content,
                    m.SentAt,
                    SenderFullName = m.SenderUser.FullName,
                    SenderEmail = m.SenderUser.Email,
                    SenderUserId = m.SenderUser.Id
                })
                .ToListAsync();

            return Ok(messages);
        }


        // 4. Admin → gönderilen mesajları listele
        [HttpGet("sent/{adminId}")]
        public async Task<IActionResult> GetSent(int adminId)
        {
            var messages = await _context.Messages
                .Where(m => m.SenderUserId == adminId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            return Ok(messages);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("inbox-with-user/{adminId}")]
        public async Task<IActionResult> GetInboxWithUser(int adminId)
        {
            var messages = await _context.Messages
                .Include(m => m.SenderUser)
                .Include(m => m.ReceiverUser)
                .Where(m => m.ReceiverUserId == adminId || m.SenderUserId == adminId)
                .OrderByDescending(m => m.SentAt)
                .Select(m => new
                {
                    m.Id,
                    m.Content,
                    m.SentAt,
                    SenderFullName = m.SenderUser.FullName,
                    SenderEmail = m.SenderUser.Email,
                    SenderUserId = m.SenderUser.Id,
                    ReceiverFullName = m.ReceiverUser.FullName,
                    ReceiverEmail = m.ReceiverUser.Email,
                    ReceiverUserId = m.ReceiverUser.Id
                })
                .ToListAsync();

            return Ok(messages);
        }





        // 5. Mesajı okundu olarak işaretle
        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null) return NotFound();

            message.IsRead = true;
            await _context.SaveChangesAsync();
            return Ok("Mesaj okundu olarak işaretlendi.");
        }


    }
}