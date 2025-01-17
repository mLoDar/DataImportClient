using System.Net;
using System.Net.Mail;

using Newtonsoft.Json.Linq;





namespace DataImportClient.Scripts
{
    internal class EmailClient
    {
        private const string _currentSection = "EmailClient";

        private static string _recipients = string.Empty;



        private struct SenderDetails
        {
            internal string senderEmail;
            internal string senderPassword;
            internal string smtpHost;
            internal int smtpPort;

            internal readonly bool HoldsInvalidValues()
            {
                var stringFields = new string[] { senderEmail, senderPassword, smtpHost };

                if (stringFields.Any(string.IsNullOrEmpty))
                {
                    return true;
                }

                if (int.TryParse(smtpPort.ToString(), out int _) == false)
                {
                    return true;
                }

                return false;
            }
        };



        internal static async Task<bool> SendEmail(string originSection, string emailSubject, string emailBody)
        {
            ActivityLogger.Log(_currentSection, $"Preparing to send a new email for '{originSection}'. Fetching sender details.");

            (SenderDetails senderDetails, Exception? occuredError) = await GetSenderDetails();

            if (occuredError != null)
            {
                ActivityLogger.Log(_currentSection, "[ERROR] - Failed to get all sender details from the application configuration.");
                ActivityLogger.Log(_currentSection, occuredError.Message, true);

                return false;
            }



            _recipients = string.Empty;

            try
            {
                MailMessage mail = new()
                {
                    From = new MailAddress(senderDetails.senderEmail),
                    Subject = emailSubject,
                    Body = emailBody,
                    IsBodyHtml = false,
                };

                if (_recipients == null || _recipients.Equals(string.Empty))
                {
                    throw new Exception("Email recipients are empty.");
                }

                mail.To.Add(_recipients);

                SmtpClient smtpClient = new(senderDetails.smtpHost, senderDetails.smtpPort)
                {
                    Credentials = new NetworkCredential(senderDetails.senderEmail, senderDetails.senderPassword),
                    EnableSsl = true
                };

                await smtpClient.SendMailAsync(mail);
            }
            catch (Exception exception)
            {
                ActivityLogger.Log(_currentSection, "[ERROR] - Failed to send an email with the following details:");
                ActivityLogger.Log(_currentSection, $"Recipients: '{_recipients}'", true);
                ActivityLogger.Log(_currentSection, $"Subject: '{emailSubject}'", true);
                ActivityLogger.Log(_currentSection, $"Body: '{emailBody}'", true);
                ActivityLogger.Log(_currentSection, exception.Message, true);
                return false;
            }

            return true;
        }
        
        private static async Task<(SenderDetails senderDetails, Exception? occuredError)> GetSenderDetails()
        {
            JObject savedConfiguration;

            try
            {
                savedConfiguration = await ConfigurationHelper.LoadConfiguration();

                if (savedConfiguration["error"] != null)
                {
                    throw new Exception($"Saved configuration file contains errors. Error: {savedConfiguration["error"]}");
                }
            }
            catch (Exception exception)
            {
                return (new SenderDetails(), exception);
            }



            JObject senderSettings;

            try
            {
                JObject emailAlerts = savedConfiguration["emailAlerts"] as JObject ?? [];

                if (emailAlerts == null || emailAlerts == new JObject())
                {
                    throw new Exception("Configuration file does not contain a 'emailAlerts' object.");
                }

                senderSettings = emailAlerts["senderSettings"] as JObject ?? [];

                if (senderSettings == null || senderSettings == new JObject())
                {
                    throw new Exception("Configuration file does not contain a 'senderSettings' object.");
                }

                JArray emailRecipients = emailAlerts["emailRecipients"] as JArray ?? [];

                if (emailRecipients == null || emailRecipients == new JArray())
                {
                    throw new Exception("Configuration file does not contain a 'emailRecipients' object.");
                }

                _recipients = string.Join(',', emailRecipients);
            }
            catch (Exception exception)
            {
                return (new SenderDetails(), exception);
            }



            SenderDetails senderDetails;

            try
            {
                senderDetails = new()
                {
                    senderEmail = senderSettings?["senderEmail"]?.ToString() ?? string.Empty,
                    senderPassword = senderSettings?["senderPassword"]?.ToString() ?? string.Empty,
                    smtpHost = senderSettings?["smtpHost"]?.ToString() ?? string.Empty,
                    smtpPort = Convert.ToInt32(senderSettings?["smtpPort"]),
                };

                if (senderDetails.HoldsInvalidValues() == true)
                {
                    throw new Exception("One or mulitple configuration values are null. Please check the configuration file!");
                }
            }
            catch (Exception exception)
            {
                return (new SenderDetails(), exception);
            }

            return (senderDetails, null);
        }
    }
}