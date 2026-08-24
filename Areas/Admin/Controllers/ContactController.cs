using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopVanPhongPham.Data;

namespace ShopVanPhongPham.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ContactController : Controller
    {
        private readonly AppDbContext _context;

        public ContactController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Contact
        // Danh sách tất cả tin nhắn, mới nhất lên trước
        public async Task<IActionResult> Index()
        {
            var messages = await _context.ContactMessages
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            // Đếm số chưa đọc để hiển thị badge
            ViewBag.UnreadCount = messages.Count(m => !m.IsRead);

            return View(messages);
        }

        // GET: /Admin/Contact/Detail/5
        // Xem chi tiết 1 tin nhắn — tự động đánh dấu đã đọc
        public async Task<IActionResult> Detail(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message == null) return NotFound();

            // Tự động đánh dấu đã đọc khi Admin mở
            if (!message.IsRead)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return View(message);
        }

        // POST: /Admin/Contact/SaveNote
        // Lưu ghi chú nội bộ của Admin cho tin nhắn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveNote(int id, string adminNote)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message == null) return NotFound();

            message.AdminNote = adminNote;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã lưu ghi chú thành công!";
            return RedirectToAction("Detail", new { id });
        }

        // POST: /Admin/Contact/MarkRead/5
        // Đánh dấu đã đọc thủ công (dùng từ danh sách)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message != null)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // POST: /Admin/Contact/Delete/5
        // Xóa tin nhắn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message != null)
            {
                _context.ContactMessages.Remove(message);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa tin nhắn.";
            }
            return RedirectToAction("Index");
        }
    }
}