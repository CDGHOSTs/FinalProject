using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.Pages
{
    public class composeModel : PageModel
    {
        private readonly ILogger<composeModel> _logger;
        [BindProperty]
        public EmailInfo NewEmail { get; set; }

        public composeModel(ILogger<composeModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            // This method is intentionally left empty.
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Shared/sendfail");
            }

            bool recipientExists = await CheckRecipientExistsAsync(NewEmail.EmailReceiver);

            if (!recipientExists)
            {
                ModelState.AddModelError("", "Recipient email does not exist in the database.");
                return Page();
            }
            else
            {
                try
                {
                    String connectionString = "Server=tcp:bankfinalproject.database.windows.net,1433;Initial Catalog=finalproject;Persist Security Info=False;User ID=bank;password=123456#B;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
                    using (var connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        var sql = @"
                            INSERT INTO emails (Emailsubject, Emailmessage, Emaildate, Emailisread, Emailsender, Emailreceiver) 
                            VALUES (@Subject, @Message, GETDATE(), @IsRead, @Sender, @Receiver)";

                        using (var command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@Subject", NewEmail.EmailSubject);
                            command.Parameters.AddWithValue("@Message", NewEmail.EmailMessage);
                            command.Parameters.AddWithValue("@IsRead", "0");
                            command.Parameters.AddWithValue("@Sender", User.Identity.Name);
                            command.Parameters.AddWithValue("@Receiver", NewEmail.EmailReceiver);

                            var result = await command.ExecuteNonQueryAsync();
                            if (result > 0)
                            {
                                _logger.LogInformation("Email sent successfully.");
                                TempData["SuccessMessage"] = "Email sent successfully.";
                                return RedirectToPage("/Shared/sendsuccess");
                            }
                            else
                            {
                                _logger.LogWarning("Failed to insert email record.");
                                ModelState.AddModelError("", "Failed to send email.");
                                return Page();
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    _logger.LogError($"An SQL error occurred: {ex.Message}");
                    ModelState.AddModelError("", "A database error occurred.");
                    return Page();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occurred: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while sending the email.");
                    return Page();
                }
            }
        }

        private async Task<bool> CheckRecipientExistsAsync(string recipientEmail)
        {
            try
            {
                String connectionString = "Server=tcp:bankfinalproject.database.windows.net,1433;Initial Catalog=finalproject;Persist Security Info=False;User ID=bank;password=123456#B;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var emailExistsQuery = "SELECT COUNT(*) FROM emails WHERE emailreceiver = @Receiver";
                    using (var emailExistsCommand = new SqlCommand(emailExistsQuery, connection))
                    {
                        emailExistsCommand.Parameters.AddWithValue("@Receiver", recipientEmail);
                        var emailExists = (int)await emailExistsCommand.ExecuteScalarAsync();

                        return emailExists > 0;
                    }
                }
            }
            catch (SqlException)
            {
                return false;
            }
        }

        public class EmailInfo
        {
            [Required]
            [Display(Name = "Subject")]
            public string EmailSubject { get; set; }

            [Required]
            [Display(Name = "Message")]
            public string EmailMessage { get; set; }

            [Required]
            [Display(Name = "Receiver")]
            public string EmailReceiver { get; set; }
        }
    }
}
