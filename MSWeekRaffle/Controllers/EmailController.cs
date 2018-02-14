using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using MSWeekRaffle.Database;
using MSWeekRaffle.Interfaces;
using MSWeekRaffle.Models;
using MSWeekRaffle.Sendgrid;
using MSWeekRaffle.Services;
using Newtonsoft.Json;

namespace MSWeekRaffle.Controllers
{
    public class EmailController : ApiController
    {
        public IEmailSender EmailSender { get; set; } = new SendgridEmailSender();

        public IImageProcessing ImageProcessing { get; set; } = new MessageToolkitImageProcessing();

        [HttpGet]
        public async Task<string> SendEmails()
        {
            var cookie = this.GetCookie(Request, "MSWeekRaffle");

            if (cookie == null)
            {
                this.Redirect(ConfigurationManager.AppSettings["BASE_URL"] + "/Home/Error");

                return "error";
            }

            if (cookie["MSWEEK_SECRET"] != ConfigurationManager.AppSettings["MSWEEK_SECRET"])
            {
                this.Redirect(ConfigurationManager.AppSettings["BASE_URL"] + "/Home/Error");

                return "error";
            }

            var results = new List<ResultModel>();
            var emails = File.ReadAllLines(HostingEnvironment.MapPath("~/emailsToProcess.txt"));

            foreach (var email in emails)
            {
                var validationCode = Guid.NewGuid();

                var validationUrl = string.Format(
                "{0}/api/email/validateRegistration?email={1}&validationCode={2}",
                ConfigurationManager.AppSettings["BASE_URL"],
                email,
                validationCode);

                using (var dbContext = new msweekEntities())
                {
                    if (dbContext.Registrations.Where(x => x.EmailAddress == email).Any())
                    {
                        results.Add(new ResultModel()
                        {
                            EmailAddress = email,
                            Message = "The email is already registered !"
                        });

                        continue;
                    }

                    dbContext.Registrations.Add(new Registration()
                    {
                        EmailAddress = email,
                        CreatedDate = DateTime.UtcNow,
                        ValidationCode = validationCode.ToString(),
                    });

                    dbContext.SaveChanges();
                }

                var imagePath = this.ImageProcessing.SaveImage(this.ImageProcessing.GenerateQR(validationUrl));

                var message = string.Format("Salut,<br /><br />Ai mai jos biletul de intrare la evenimentul Microsoft Week, care se va desf&#259;&#351;ura de luni p&#259;n&#259; vineri inclusiv, &#238;ntre orele 19 - 21 &#238;n sala PR001. Aten&#355;ie, biletul sub form&#259; de QR code trebuie scanat de unul dintre membrii Microsoft Student Partners pentru a putea participa la eveniment(pe care &#238;i vei putea recunoa&#351;te dup&#259; tricou).<br /><br /><img src=\"{0}\" width=\"100\" height=\"100\" /><br /><br />Te a&#351;tept&#259;m cu mai multe surprize la eveniment!<br /><br />Echipa Microsoft Innovation Center UPB, EG101", imagePath);

                var result = await EmailSender.SendMailAsync(new EmailData()
                {
                    FromEmailAddress = "YOUR_MSP_EMAIL",
                    FromName = "Evenimente MIC",
                    ToEmailAddress = email,
                    Subject = "Microsoft Week 17-21 oct.",
                    Content = message,
                });

                if (result != HttpStatusCode.Accepted)
                {
                    results.Add(new ResultModel()
                    {
                        EmailAddress = email,
                        Message = "There was a problem sending the email. Try manually !"
                    });

                    continue;
                }

                results.Add(new ResultModel()
                {
                    EmailAddress = email,
                    Message = "Success !"
                });

                await Task.Delay(100);
            }

            File.WriteAllLines(HostingEnvironment.MapPath("~/emailsToProcess.txt"), new string[] { });

            return JsonConvert.SerializeObject(results);
        }

        [HttpGet]
        public IHttpActionResult ValidateRegistration(string email, string validationCode)
        {
            var cookie = this.GetCookie(Request, "MSWeekRaffle");

            if (cookie == null)
            {
                return this.Redirect(ConfigurationManager.AppSettings["BASE_URL"] + "/Home/Error");
            }

            if (cookie["MSWEEK_SECRET"] != ConfigurationManager.AppSettings["MSWEEK_SECRET"])
            {
                return this.Redirect(ConfigurationManager.AppSettings["BASE_URL"] + "/Home/Error");
            }

            using (var dbContext = new msweekEntities())
            {
                if (!dbContext.Registrations.Where(x => x.EmailAddress == email && x.ValidationCode == validationCode).Any())
                {
                    return this.Redirect(ConfigurationManager.AppSettings["BASE_URL"] + "/Home/Error");
                }

                var entry = dbContext.Registrations
                    .Where(x => x.EmailAddress == email)
                    .First();

                entry.IsValidated = true;
                entry.ActivatedDate = DateTime.UtcNow;

                dbContext.SaveChanges();
            }

            return this.Redirect(ConfigurationManager.AppSettings["BASE_URL"] + "/Home/Validated");
        }

        private Dictionary<string, string> GetCookie(HttpRequestMessage request, string cookieName)
        {
            var cookies = request.Headers
                .Where(x => x.Key.ToLower() == "cookie");

            if (cookies != null)
            {
                var cookieValueArray = cookies.FirstOrDefault().Value;

                if (cookieValueArray != null)
                {
                    var cookiePair = cookieValueArray
                        .FirstOrDefault()
                        .Split(new char[] { ';' })
                        .Where(x => x.Trim().StartsWith(cookieName))
                        .FirstOrDefault();

                    if (cookiePair != null)
                    {
                        var cookie = cookiePair
                            .Trim()
                            .Substring(cookieName.Length + 1);

                        return new Dictionary<string, string>()
                        {
                            { cookie.Split('=')[0], cookie.Split('=')[1] }
                        };
                    }
                }
            }

            return null;
        }
    }
}