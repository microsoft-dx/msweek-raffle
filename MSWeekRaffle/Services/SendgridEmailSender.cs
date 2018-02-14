using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using MSWeekRaffle.Models;
using MSWeekRaffle.Sendgrid;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace MSWeekRaffle.Services
{
    public class SendgridEmailSender : IEmailSender
    {
        public async Task<HttpStatusCode> SendMailAsync(EmailData emailData)
        {
            var sender = new SendGridAPIClient(ConfigurationManager.AppSettings["API_KEY"]);

            var fromPerson = new Email(emailData.FromEmailAddress, emailData.FromName);
            var toPerson = new Email(emailData.ToEmailAddress);
            var content = new Content("text/html", emailData.Content);

            var mail = new Mail(fromPerson, emailData.Subject, toPerson, content);

            var response = await sender.client.mail.send.post(requestBody: mail.Get());

            return response.StatusCode;
        }
    }
}