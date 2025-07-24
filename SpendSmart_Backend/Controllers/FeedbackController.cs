using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using System.Text;

namespace YourApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FeedbackController> _logger;

        public FeedbackController(
            ApplicationDbContext context, 
            IConfiguration configuration,
            ILogger<FeedbackController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitFeedback([FromBody] FeedbackSubmissionDto feedbackDto)
        {
            try
            {
                // Validate the input
                if (feedbackDto.Rating < 1 || feedbackDto.Rating > 5)
                {
                    return BadRequest("Rating must be between 1 and 5.");
                }

                // Create feedback entity
                var feedback = new Feedback
                {
                    UserId = feedbackDto.UserId,
                    Rating = feedbackDto.Rating,
                    Comment = feedbackDto.Comment?.Trim(),
                    PageContext = feedbackDto.PageContext,
                    SubmittedAt = DateTime.UtcNow,
                    IsProcessed = false
                };

                // Save to database
                _context.Feedbacks.Add(feedback);
                await _context.SaveChangesAsync();

                // Send email notification (optional)
                try
                {
                    await SendFeedbackNotificationEmail(feedback);
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning($"Failed to send feedback notification email: {emailEx.Message}");
                    // Don't fail the whole request if email fails
                }

                _logger.LogInformation($"Feedback submitted successfully. ID: {feedback.Id}, UserId: {feedbackDto.UserId}");

                return Ok(new
                {
                    success = true,
                    message = "Thank you for your feedback!",
                    feedbackId = feedback.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error submitting feedback: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while submitting your feedback. Please try again."
                });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserFeedback(int userId, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var feedbacks = await _context.Feedbacks
                    .Where(f => f.UserId == userId)
                    .OrderByDescending(f => f.SubmittedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(f => new
                    {
                        f.Id,
                        f.Rating,
                        f.Comment,
                        f.PageContext,
                        f.SubmittedAt,
                        f.IsProcessed
                    })
                    .ToListAsync();

                var totalCount = await _context.Feedbacks
                    .CountAsync(f => f.UserId == userId);

                return Ok(new
                {
                    feedbacks,
                    totalCount,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving user feedback: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving feedback.");
            }
        }

        // ADMIN ONLY - Get all feedback
        [HttpGet("all")]
        public async Task<IActionResult> GetAllFeedback(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? minRating = null,
            [FromQuery] int? maxRating = null,
            [FromQuery] string? pageContext = null)
        {
            try
            {
                var query = _context.Feedbacks
                    .Include(f => f.User) // Assuming you have a User navigation property
                    .AsQueryable();

                // Apply filters
                if (minRating.HasValue)
                    query = query.Where(f => f.Rating >= minRating.Value);

                if (maxRating.HasValue)
                    query = query.Where(f => f.Rating <= maxRating.Value);

                if (!string.IsNullOrEmpty(pageContext))
                    query = query.Where(f => f.PageContext == pageContext);

                var feedbacks = await query
                    .OrderByDescending(f => f.SubmittedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(f => new
                    {
                        f.Id,
                        f.UserId,
                        UserName = f.User.Name, // Adjust based on your User model
                        UserEmail = f.User.Email,
                        f.Rating,
                        f.Comment,
                        f.PageContext,
                        f.SubmittedAt,
                        f.IsProcessed
                    })
                    .ToListAsync();

                var totalCount = await query.CountAsync();

                return Ok(new
                {
                    feedbacks,
                    totalCount,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving all feedback: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving feedback.");
            }
        }

        [HttpPut("{feedbackId}/mark-processed")]
        public async Task<IActionResult> MarkFeedbackAsProcessed(int feedbackId)
        {
            try
            {
                var feedback = await _context.Feedbacks.FindAsync(feedbackId);
                if (feedback == null)
                {
                    return NotFound("Feedback not found.");
                }

                feedback.IsProcessed = true;
                feedback.ProcessedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Feedback marked as processed." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error marking feedback as processed: {ex.Message}");
                return StatusCode(500, "An error occurred while updating feedback.");
            }
        }

        private async Task SendFeedbackNotificationEmail(Feedback feedback)
        {
            // CUSTOMIZE: Add your email configuration in appsettings.json
            var smtpHost = _configuration["EmailSettings:SmtpHost"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var smtpUser = _configuration["EmailSettings:SmtpUser"];
            var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
            var fromEmail = _configuration["EmailSettings:FromEmail"];
            
            // CUSTOMIZE: Add your notification email addresses here
            var notificationEmails = _configuration["EmailSettings:FeedbackNotificationEmails"]
                ?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                ?? new[] { "admin@yourcompany.com", "feedback@yourcompany.com" }; // Default emails

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser))
            {
                _logger.LogWarning("Email configuration missing. Skipping email notification.");
                return;
            }

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPassword),
                EnableSsl = true
            };

            var ratingStars = new string('⭐', feedback.Rating);
            var subject = $"New Feedback Received - {ratingStars} ({feedback.Rating}/5)";
            
            var body = new StringBuilder();
            body.AppendLine("<html><body>");
            body.AppendLine("<h2>New Feedback Received</h2>");
            body.AppendLine($"<p><strong>Rating:</strong> {ratingStars} ({feedback.Rating}/5)</p>");
            body.AppendLine($"<p><strong>User ID:</strong> {feedback.UserId}</p>");
            body.AppendLine($"<p><strong>Page Context:</strong> {feedback.PageContext}</p>");
            body.AppendLine($"<p><strong>Submitted At:</strong> {feedback.SubmittedAt:yyyy-MM-dd HH:mm:ss} UTC</p>");
            
            if (!string.IsNullOrEmpty(feedback.Comment))
            {
                body.AppendLine("<p><strong>Comment:</strong></p>");
                body.AppendLine($"<div style='border-left: 3px solid #ff8c00; padding-left: 15px; margin-left: 10px;'>");
                body.AppendLine($"<p>{feedback.Comment.Replace("\n", "<br>")}</p>");
                body.AppendLine("</div>");
            }
            
            body.AppendLine("<hr>");
            body.AppendLine($"<p><small>Feedback ID: {feedback.Id}</small></p>");
            body.AppendLine("</body></html>");

            foreach (var email in notificationEmails)
            {
                try
                {
                    var message = new MailMessage(fromEmail, email.Trim())
                    {
                        Subject = subject,
                        Body = body.ToString(),
                        IsBodyHtml = true
                    };

                    await client.SendMailAsync(message);
                    _logger.LogInformation($"Feedback notification sent to {email}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to send email to {email}: {ex.Message}");
                }
            }
        }
    }

    // DTOs
    public class FeedbackSubmissionDto
    {
        public int UserId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string? PageContext { get; set; }
        public DateTime Timestamp { get; set; }
    }
}