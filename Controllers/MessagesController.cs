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
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            if (message == null || string.IsNullOrWhiteSpace(message.Content))
                return BadRequest("Mesaj içeriği boş olamaz.");

            if (message.ReceiverUserId == null)
            {
                // Toplu mesaj: tüm kullanıcılara kopya oluştur
                var userIds = await _context.Users.Select(u => u.Id).ToListAsync();
                var messages = userIds.Select(uid => new Message
                {
                    SenderUserId = message.SenderUserId,
                    ReceiverUserId = uid,
                    Content = message.Content,
                    SentAt = DateTime.UtcNow
                }).ToList();

                _context.Messages.AddRange(messages);
            }
            else
            {
                // Bireysel mesaj
                _context.Messages.Add(message);
            }

            await _context.SaveChangesAsync();
            return Ok("Mesaj gönderildi.");
        }

        // 2. Kullanıcı → destek mesajı gönder
        [HttpPost("support")]
        public async Task<IActionResult> SendSupport([FromBody] Message message)
        {
            if (message == null || string.IsNullOrWhiteSpace(message.Content))
                return BadRequest("Mesaj içeriği boş olamaz.");

            // Admin'e gönderildiği varsayılır (örnek: admin ID = 1)
            message.ReceiverUserId = 1;
            message.SentAt = DateTime.UtcNow;

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return Ok("Destek mesajı gönderildi.");
        }

        // 3. Kullanıcı → gelen mesajları listele
        [HttpGet("inbox/{userId}")]
        public async Task<IActionResult> GetInbox(int userId)
        {
            var messages = await _context.Messages
                .Where(m => m.ReceiverUserId == userId)
                .OrderByDescending(m => m.SentAt)
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