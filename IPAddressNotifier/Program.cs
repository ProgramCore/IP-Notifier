using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

/*
 * A small app the checks to see if the ISP has changed your public IP, and emails recipients if it has
 * The smtp settings below are configured to work with outlook, but can easily be changed for a different email service
 */

namespace IPAddressNotifier
{
    class Program
    {
        public static string ipAddress = string.Empty;
        public const int checkIntervalMinutes = 10;
        private const string SMTP_SERVER = "smtp-mail.outlook.com";
        private const int SMTP_PORT = 587;
        private const string FROM_EMAIL = "FromEmail@email.com";
        private const string FROM_EMAIL_PW = "#passwordOfFromEmail";
        private static List<string> recipients = new List<string>() {"email@email.com", "email@email.com"};

        static void Main(string[] args)
        {
            Timer timer = new Timer(checkIntervalMinutes * 60000);
            timer.Elapsed += Timer_Elapsed;

            timer.Start();

            Console.WriteLine("Press enter to exit");
            Console.Read();
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var currentIP = GetPublicIP4();
            
            if(!string.IsNullOrEmpty(currentIP))
            {
                Console.WriteLine($"Current IP: {currentIP}");

                if (!ipAddress.Equals(currentIP))
                {
                    ipAddress = currentIP ?? string.Empty;
                    SendEmailIP();
                }
            }
        }

        private static string GetPublicIP4()
        {
            string ip = string.Empty;

            try
            {
                ip = new WebClient().DownloadString("https://api.ipify.org");
            }
            catch(WebException we)
            {
                ip = null;
            }

            return ip;
        }

        private static string GetPublicIP()
        {
            Ping ping = new Ping();
            var instance = ping.Send("google.com");

            if (instance.Status == IPStatus.Success)
            {
                return instance.Address.ToString();
            }

            return null;
        }

        private static void SendEmailIP()
        {
            var mail = new MailMessage();
            mail.From = new MailAddress(FROM_EMAIL);
            AddRecipients(mail);            
            mail.Subject = "Public IP Changed";
            mail.IsBodyHtml = true;
            mail.Body = $"<html><body><p>{ipAddress}</p></body></html>";

            SmtpClient SmtpServer = new SmtpClient(SMTP_SERVER);
            SmtpServer.Port = SMTP_PORT;
            SmtpServer.UseDefaultCredentials = false;
            SmtpServer.Credentials = new System.Net.NetworkCredential(FROM_EMAIL, FROM_EMAIL_PW);
            SmtpServer.EnableSsl = true;

            try
            {
                SmtpServer.Send(mail);
            }
            catch (SmtpException se)
            {
                Console.WriteLine("Unable to send email.");
            }
            finally
            {
                mail.Dispose();
            }
        }

        private static void AddRecipients(MailMessage mail)
        {
            foreach(string addr in recipients)
            {
                mail.To.Add(addr);
            }
        }
    }
}
