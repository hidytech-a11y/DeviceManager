using System.Threading.Tasks;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;

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

        public async System.Threading.Tasks.Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var apiKey = _configuration["Brevo:ApiKey"];
                var fromEmail = _configuration["Email:FromEmail"];
                var fromName = _configuration["Email:FromName"] ?? "Device Manager";

                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogError("Brevo API key not configured!");
                    return;
                }

                Configuration.Default.ApiKey["api-key"] = apiKey;

                var apiInstance = new TransactionalEmailsApi();

                var sendSmtpEmail = new SendSmtpEmail(
                    sender: new SendSmtpEmailSender(fromEmail, fromName),
                    to: new List<SendSmtpEmailTo>
                    {
                        new SendSmtpEmailTo(toEmail)
                    },
                    subject: subject,
                    htmlContent: body
                );

                var response = await apiInstance.SendTransacEmailAsync(sendSmtpEmail);

                _logger.LogInformation($"Email sent via Brevo. MessageId: {response.MessageId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email via Brevo: {ex}");
            }
        }
    }
}