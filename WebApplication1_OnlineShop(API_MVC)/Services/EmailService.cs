using System.Net;
using System.Net.Mail;
using System.Security.Policy;

namespace WebApplication1_API_MVC_.Services
{
    public interface IEmailSender
    {
        Task ChangePasswordNotification(string receptor, string subject, string body);
        Task ResetPasswordLink(string receptor, string subject, string body);
    }
    public class EmailService : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContext;

        public EmailService(IConfiguration configuration , IHttpContextAccessor httpContext)
        {
            _configuration = configuration;
            _httpContext = httpContext;
        }
        public async Task ChangePasswordNotification(string receptor , string subject , string body)
        {
            try
            {
                var email = _configuration.GetValue<string>("EMAIL_CONFIGURATION:EMAIL");
                var password = _configuration.GetValue<string>("EMAIL_CONFIGURATION:PASSWORD");
                var host = _configuration.GetValue<string>("EMAIL_CONFIGURATION:HOST");
                var port = _configuration.GetValue<int>("EMAIL_CONFIGURATION:PORT");

                var smtpClient = new SmtpClient(host, port);
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(email, password);

                var (ip, device) = await GetDeviceLog();

                body = body + "\n IP Address : " + ip + "\nDevice : " + device;

                var message = new MailMessage(email!, receptor, subject, body);
                await smtpClient.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
                throw;
            }

        }

        private async Task<(string ipAddress , string device_log)> GetDeviceLog()
        {
            string ipAddress = _httpContext.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(ipAddress))
            {
                // X-Forwarded-For can contain multiple IPs if there are multiple proxies
                ipAddress = ipAddress.Split(',')[0];
            }
            else
            {
                ipAddress = _httpContext.HttpContext.Connection.RemoteIpAddress.ToString();
            }
            var device_log = _httpContext.HttpContext.Request.Headers["User-Agent"].ToString();

            return (ipAddress, device_log);

        }

        public async Task ResetPasswordLink(string receptor , string subject, string body)
        {
            var email = _configuration.GetValue<string>("EMAIL_CONFIGURATION:EMAIL");
            var password = _configuration.GetValue<string>("EMAIL_CONFIGURATION:PASSWORD");
            var host = _configuration.GetValue<string>("EMAIL_CONFIGURATION:HOST");
            var port = _configuration.GetValue<int>("EMAIL_CONFIGURATION:PORT");

            var smtpClient = new SmtpClient(host,port);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(email, password);

            var message = new MailMessage(email! ,receptor , subject , body );
            await smtpClient.SendMailAsync(message);

        }
    }
}
