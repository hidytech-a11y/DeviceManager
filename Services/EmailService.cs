using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace DeviceManager.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpHost = _configuration["Email:SmtpHost"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["Email:Username"];
                var smtpPassword = _configuration["Email:Password"];
                var fromEmail = _configuration["Email:FromEmail"] ?? "noreply@devicemanager.com";
                var fromName = _configuration["Email:FromName"] ?? "Device Manager";

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, fromEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <html>
                        <head>
                            <style>
                                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                                .header {{ background-color: #0d6efd; color: white; padding: 20px; text-align: center; }}
                                .content {{ padding: 20px; background-color: #f8f9fa; }}
                                .footer {{ padding: 10px; text-align: center; font-size: 12px; color: #666; }}
                                .button {{ background-color: #0d6efd; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block; margin-top: 10px; }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <div class='header'>
                                    <h2>Device Manager Notification</h2>
                                </div>
                                <div class='content'>
                                    {body}
                                </div>
                                <div class='footer'>
                                    <p>This is an automated message from Device Manager System.</p>
                                    <p>© {DateTime.Now.Year} Device Manager. All rights reserved.</p>
                                </div>
                            </div>
                        </body>
                        </html>
                    "
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();

                await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpUsername, smtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email to {toEmail}: {ex.Message}");
                // Don't throw - we don't want email failures to break the app
            }
        }
    }
}