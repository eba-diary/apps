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
            SmtpClient smtpClient = new SmtpClient("mail.sentry.com");
            
            MailAddress from = new MailAddress(Configuration.Config.GetHostSetting("DatasetMgmtEmail"));
            
            MailMessage myMail = new System.Net.Mail.MailMessage();
            myMail.From = from;
            myMail.Subject = interval + " Events from data.sentry.com";
            myMail.To.Add(emailAddress);
            myMail.IsBodyHtml = true;
            myMail.Body += @"<p><b><font color=""red"">Do Not Reply To This Email, This Inbox Is Not Monitored</font></b></p>";
            

            switch (interval)
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

            
            List<Event> dsEvents = events.Where(w => w.EventType.Group != EventTypeGroup.BusinessArea.GetDescription()).Distinct().ToList();
            List<Event> baEvents = events.Where(w => w.EventType.Group == EventTypeGroup.BusinessArea.GetDescription()).Distinct().OrderBy(o => o.TimeCreated).ToList();

            //DATASET
            myMail.Body += @"</p><table cellpadding='0' cellspacing='0' border='0' width='100 % '><tr bgcolor='003DA5'><td><b>Dataset Events</b></td></table></p>";
            string header = @"<tr bgcolor='00A3E0'><td><b>Creation Date</b></td><td><b>Description</b></td><td><b>Status</b></td><td><b>Initiator</b></td><td><b>Event Type</b></td></tr>";
            myMail.Body += CreateEvents(header, EventTypeGroup.DataSet,dsEvents);

            //BUSINESSAREA
            myMail.Body += @"</p><table cellpadding='0' cellspacing='0' border='0' width='100 % '><tr bgcolor='003DA5'><td><b>Business Area Events</b></td></table></p>";
            header = @"<tr bgcolor='00A3E0'><td><b>Creation Date</b></td><td><b>Description</b></td><td><b>Initiator</b></td><td><b>Event Type</b></td></tr>";
            myMail.Body += CreateEvents(header, EventTypeGroup.BusinessArea,baEvents);

            smtpClient.Send(myMail);
        }


        public string CreateEvents(string header, EventTypeGroup etGroup , List<Event> events)
        {
            string body = string.Empty;

            body += @"<table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100 %"">";
            var groups = events.Where(x => x.Parent_Event != null).ToList();
            events.RemoveAll(x => x.Parent_Event != null);

            if (groups.Any())
            {
                List<IGrouping<string, Event>> groupedGroups = groups.GroupBy(x => x.Parent_Event).ToList();
                foreach (var group in groupedGroups)
                {
                    if (group.Count() > 1)
                    {
                        body += header;
                        foreach (Event e in group)
                        {
                            body += FormatEventLine(EventTypeGroup.DataSet, e);
                        }
                        body += @"<tr></tr>";
                    }
                    else
                    {
                        events.Add(group.First());
                    }
                }
            }

            if (events.Count > 0)
            {
                body += header;

                foreach (Event e in events.OrderBy(x => x.EventType.Severity))
                {
                    body += FormatEventLine(etGroup, e);
                }
            }

            body += @"</table>";

            return body;
        }


        public string FormatEventLine(EventTypeGroup group, Event e)
        {
            string body = @"<tr>";
            body += @"<td>" + e.TimeCreated + @"</td>";

            if (group == EventTypeGroup.BusinessArea)
            {
                string reason = "<a href=" + Configuration.Config.GetHostSetting("WebApiUrl") + "/Notification/ManageNotification>" + e.Notification.Title + "</a>";
                if (e.Notification.MessageSeverity == Core.GlobalEnums.NotificationSeverity.Critical)
                    reason += "<br>" + e.Notification.Message;        
               
                body += @"<td>" + reason + @"</td>";
            }
            else
            {
                body += @"<td>" + e.Reason + @"</td>";
                body += @"<td>" + e.Status.Description + @"</td>";
            }

            //Needed to resolve service accounts
            int n;
            var user = int.TryParse(e.UserWhoStartedEvent.Trim(), out n) ? _associateInfoProvider.GetAssociateInfo(e.UserWhoStartedEvent.Trim()).FullName : e.UserWhoStartedEvent.Trim();
            body += @"<td>" + user + @"</td>";
            body += @"<td>" + e.EventType.Description + @"</td>";
            body += @" </tr>";

            return body;
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

            foreach (var address in emailAddress.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                myMail.To.Add(address);
            }            

            foreach (var address in cc.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                myMail.CC.Add(address);
            }            

            smtpClient.Send(myMail);
        }
    }
}
