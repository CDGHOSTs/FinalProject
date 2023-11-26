using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Mail;

namespace FinalProject.Pages
{
    public class composeModel : PageModel
    {
        public IActionResult OnPostSendMessage(string to, string subject, string message)
        {
            // ตรวจสอบว่ามีข้อมูลอยู่ใน to, subject, และ message หรือไม่
            if (!string.IsNullOrEmpty(to) && !string.IsNullOrEmpty(subject) && !string.IsNullOrEmpty(message))
            {
                // สร้างอ็อบเจ็กต์ของคลาส MailMessage เพื่อสร้างอีเมล์
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("your-email@example.com"); // อีเมล์ผู้ส่ง
                mail.To.Add(to); // อีเมล์ผู้รับ
                mail.Subject = subject; // เรื่อง
                mail.Body = message; // ข้อความ

                // ส่งอีเมล์โดยใช้ SMTP server
                SmtpClient smtpClient = new SmtpClient("your-smtp-server.com");
                smtpClient.Port = 587; // Port ของ SMTP server
                smtpClient.Credentials = new System.Net.NetworkCredential("your-username", "your-password"); // ข้อมูลการเข้าสู่ระบบของ SMTP server
                smtpClient.EnableSsl = true; // เปิดใช้งาน SSL

                // พยายามส่งอีเมล์
                try
                {
                    smtpClient.Send(mail);
                    return RedirectToPage("/Index"); // ส่งผู้ใช้กลับไปที่หน้า Index หลังจากส่งอีเมล์เรียบร้อย
                }
                catch (Exception ex)
                {
                    // หากเกิดข้อผิดพลาดในการส่งอีเมล์
                    ModelState.AddModelError(string.Empty, "Error sending email: " + ex.Message);
                    return Page(); // แสดงหน้า compose อีกครั้งเพื่อให้ผู้ใช้ลองส่งอีเมล์อีกครั้ง
                }
                finally
                {
                    // ปิดการเชื่อมต่อ SMTP
                    smtpClient.Dispose();
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Please provide recipient, subject, and message."); // ถ้าข้อมูลไม่ครบถ้วน
                return Page(); // แสดงหน้า compose อีกครั้งเพื่อให้ผู้ใช้ลองกรอกข้อมูลใหม่
            }
        }
    }
}
