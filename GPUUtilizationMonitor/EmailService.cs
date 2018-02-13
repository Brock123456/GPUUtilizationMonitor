using System;
using System.Net.Mail;
using LogService;

namespace EmailService
{
    public class EmailClass
    {
        public void SendEmail(string to, string subject, string body, string from, string password)
        {
            try
            {
                MailMessage email = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";

                // set up the Gmail server
                smtp.EnableSsl = true;
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential(from, password);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                // draft the email
                MailAddress fromAddress = new MailAddress(from);
                email.From = fromAddress;
                email.To.Add(to);
                email.Subject = subject;
                email.Body = body;

                smtp.Send(email);
                
            }
            catch (Exception e)
            {
                LogClass logClass = new LogClass();
                logClass.Log("Error sending email - carring on any ways - " + e);
            }
        }
    }
}