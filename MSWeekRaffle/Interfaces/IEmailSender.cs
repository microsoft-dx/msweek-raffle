using System.Net;
using System.Threading.Tasks;
using MSWeekRaffle.Models;

namespace MSWeekRaffle.Sendgrid
{
    public interface IEmailSender
    {
        Task<HttpStatusCode> SendMailAsync(EmailData emailData);
    }
}