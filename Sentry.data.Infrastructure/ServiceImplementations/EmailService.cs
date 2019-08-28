using System.Collections.Generic;
using System.Net.Mail;
using Sentry.data.Core;
using System.Linq;
using System.Text;
using System;

namespace Sentry.data.Infrastructure
{
    public class EmailService : IEmailService
    {
        private IAssociateInfoProvider _associateInfoProvider;

        public EmailService(IAssociateInfoProvider associateInfoProvider)
        {
            _associateInfoProvider = associateInfoProvider;
        }



        public void SendInvalidReportLocationEmail(BusinessIntelligenceDto report, string userName)
        {
            SmtpClient smtpClient = new SmtpClient("mail.sentry.com");
            MailAddress from = new MailAddress("NoReply@sentry.com");
            MailMessage myMail = new System.Net.Mail.MailMessage();
            myMail.From = from;

            myMail.Subject = $"{Configuration.Config.GetDefaultEnvironmentName()} - BI report location permission";

            myMail.To.Add("DSCSupport@sentry.com");

            myMail.IsBodyHtml = true;
            
            myMail.Body += @"<p><b><font color=""red"">Do Not Reply To This Email, This Inbox Is Not Monitored</font></b></p>";
            myMail.Body += $@"<p>{userName} tried to submit a Business Intelligence report with a report location that DSC does not have permisison to.</p>";
            myMail.Body += $@"<p>Enter a ticket with IAM for DSC to gain access to {report.Location} .</p>";

            smtpClient.Send(myMail);
        }


        public void SendEmail(string emailAddress, string interval, string subject, List<Event> events)
        {
            //Real code could look something like this:
            //Dim smtpClient As New System.Net.Mail.SmtpClient("mail.sentry.com")
            //smtpClient.Send("notifications@SentryData.com", toAddress, subject, body)

            SmtpClient smtpClient = new SmtpClient("mail.sentry.com");


            // set smtp-client with basicAuthentication
            //mySmtpClient.UseDefaultCredentials = false;
            //System.Net.NetworkCredential basicAuthenticationInfo = new System.Net.NetworkCredential("username", "password");
            //mySmtpClient.Credentials = basicAuthenticationInfo;

            // add from,to mailaddresses
            MailAddress from = new MailAddress(Configuration.Config.GetHostSetting("DatasetMgmtEmail"));
            MailMessage myMail = new System.Net.Mail.MailMessage();
            myMail.From = from;

            myMail.Subject = interval + " Events from data.sentry.com";

            myMail.To.Add(emailAddress);

            myMail.IsBodyHtml = true;

            myMail.Body += @"<p><b><font color=""red"">Do Not Reply To This Email, This Inbox Is Not Monitored</font></b></p>";


            switch(interval)
            {
                case  "Weekly":
                    myMail.Body += @"<p>Below is a list of all the events that have taken place in the last <b>Week</b>.</p>";
                    break;
                case "Daily":
                    myMail.Body += @"<p>Below is a list of all the events that have taken place in the last <b>24 Hours</b>.</p>";
                    break;
                case "Hourly":
                    myMail.Body += @"<p>Below is a list of all the events that have taken place in the last <b>Hour</b>.</p>";
                    break;
                default:
                    myMail.Body += @"<p>Below is a list of all the events that have taken place recently. </p>";
                    break;
            }

            events = events.Distinct().ToList();

            var groups = events.Where(x => x.Parent_Event != null).ToList();
            events.RemoveAll(x => x.Parent_Event != null);

            myMail.Body += @"<table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100 %"">";

            if (groups.Any())
            {
                List<IGrouping<string, Event>> groupedGroups = groups.GroupBy(x => x.Parent_Event).ToList();

                foreach (var group in groupedGroups)
                {
                    if(group.Count() > 1)
                    {
                        
                        myMail.Body += @"<tr bgcolor='4da6ff'><td><b>Creation Date</b></td><td><b>Description</b></td><td><b>Status</b></td><td><b>Initiator</b></td><td><b>Event Type</b></td></tr>";

                        foreach (Event e in group)
                        {
                            myMail.Body += @"<tr>";

                            myMail.Body += @"<td>" + e.TimeCreated + @"</td>";

                            myMail.Body += @"<td>" + e.Reason + @"</td>";

                            myMail.Body += @"<td>" + e.Status.Description + @"</td>";

                            //Needed to resolve service accounts
                            int n;
                            var user = int.TryParse(e.UserWhoStartedEvent.Trim(), out n) ? _associateInfoProvider.GetAssociateInfo(e.UserWhoStartedEvent.Trim()).FullName : e.UserWhoStartedEvent.Trim();

                            myMail.Body += @"<td>" + user + @"</td>";

                            myMail.Body += @"<td>" + e.EventType.Description + @"</td>";

                            myMail.Body += @" </tr>";
                        }

                        myMail.Body += @"<tr></tr>";
                    }
                    else
                    {
                        events.Add(group.First());
                    }


                }
            }

            if (events.Count > 0)
            {

                myMail.Body += @"<tr bgcolor='4da6ff'><td><b>Creation Date</b></td><td><b>Description</b></td><td><b>Status</b></td><td><b>Initiator</b></td><td><b>Event Type</b></td></tr>";

                foreach (Event e in events.OrderBy(x => x.EventType.Severity))
                {
                    myMail.Body += @"<tr>";

                    myMail.Body += @"<td>" + e.TimeCreated + @"</td>";

                    myMail.Body += @"<td>" + e.Reason + @"</td>";

                    myMail.Body += @"<td>" + e.Status.Description + @"</td>";

                    //Needed to resolve service accounts
                    int n;
                    var user = int.TryParse(e.UserWhoStartedEvent.Trim(), out n) ? _associateInfoProvider.GetAssociateInfo(e.UserWhoStartedEvent.Trim()).FullName : e.UserWhoStartedEvent.Trim();

                    myMail.Body += @"<td>" + user + @"</td>";

                    myMail.Body += @"<td>" + e.EventType.Description + @"</td>";

                    myMail.Body += @" </tr>";
                }
            }

            myMail.Body += @"</table>";

            
            smtpClient.Send(myMail);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="cc">Multiple emails supported by separating with semi-colon</param>
        public void SendGenericEmail(string emailAddress, string subject, string body, string cc)
        {
            SmtpClient smtpClient = new SmtpClient("mail.sentry.com");
            MailAddress from = new MailAddress("NoReply@sentry.com");
            MailMessage myMail = new MailMessage
            {
                From = from,
                Subject = subject,
                IsBodyHtml = true,
                Body = body,
            };

            foreach (var address in emailAddress.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries))
            {
                myMail.To.Add(address);
            }            

            foreach (var address in cc.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries))
            {
                myMail.CC.Add(address);
            }            

            smtpClient.Send(myMail);
        }
    }
}
