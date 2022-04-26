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
            
            StringBuilder body = new StringBuilder();
            body.Append(@"<p><b>Do Not Reply To This Email, This Inbox Is Not Monitored</b></p>");
            
            switch (interval)
            {
                case  "Weekly":
                    body.Append(@"<p>Below is a list of all the events that have taken place in the last <b>Week</b>.</p>");
                    break;
                case "Daily":
                    body.Append(@"<p>Below is a list of all the events that have taken place in the last <b>24 Hours</b>.</p>");
                    break;
                case "Hourly":
                    body.Append(@"<p>Below is a list of all the events that have taken place in the last <b>Hour</b>.</p>");
                    break;
                default:
                    body.Append(@"<p>Below is a list of all the events that have taken place recently. </p>");
                    break;
            }

            
            List<Event> dsEvents = events.Where(w => w.EventType.Group == EventTypeGroup.DataSet.GetDescription()
                                                    || w.EventType.Description == GlobalConstants.EventType.CREATED_DATASET
                                                    || w.EventType.Description == GlobalConstants.EventType.CREATE_DATASET_SCHEMA
                                                    || w.EventType.Description == GlobalConstants.EventType.CREATED_REPORT
                                                ).Distinct().ToList();

            List<Event> baEvents = events.Where(w => w.EventType.Group == EventTypeGroup.BusinessArea.GetDescription()).Distinct().OrderBy(o => o.TimeCreated).ToList();
            
            List<Event> DSCEvents = events.Where(w => w.EventType.Group == EventTypeGroup.BusinessAreaDSC.GetDescription()
                                                    && w.EventType.Description != GlobalConstants.EventType.CREATED_DATASET
                                                    && w.EventType.Description != GlobalConstants.EventType.CREATE_DATASET_SCHEMA
                                                    && w.EventType.Description != GlobalConstants.EventType.CREATED_REPORT
                                                    
                                                    //DSC EVENTS RELEASE NOTES OR NEWS
                                                    || w.EventType.Group == EventTypeGroup.BusinessAreaDSCReleaseNotes.GetDescription()
                                                    || w.EventType.Group == EventTypeGroup.BusinessAreaDSCNews.GetDescription() 
                                                    
                                                ).Distinct().OrderBy(o => o.TimeCreated).ToList();
            string header = String.Empty;
            //DATASET
            if (dsEvents.Any())
            {
                body.Append(@"</p><table cellpadding='0' cellspacing='0' border='0' width='100 % '><tr bgcolor='003DA5'><td><b>Dataset Events</b></td></table></p>");
                header = @"<tr bgcolor='00A3E0'><td><b>Creation Date</b></td><td><b>Description</b></td><td><b>Status</b></td><td><b>Initiator</b></td><td><b>Event Type</b></td></tr>";
                body.Append(CreateEvents(header, EventTypeGroup.DataSet, dsEvents));
            }

            //BUSINESSAREA
            if (baEvents.Any())
            {
                body.Append(@"</p><table cellpadding='0' cellspacing='0' border='0' width='100 % '><tr bgcolor='003DA5'><td><b>Business Area Events</b></td></table></p>");
                header = @"<tr bgcolor='00A3E0'><td><b>Creation Date</b></td><td><b>Description</b></td><td><b>Initiator</b></td><td><b>Event Type</b></td><td><b>Expiration Date</b></td></tr>";
                body.Append(CreateEvents(header, EventTypeGroup.BusinessArea, baEvents));
            }

            //BUSINESSAREA DSC
            if (DSCEvents.Any())
            {
                body.Append(@"</p><table cellpadding='0' cellspacing='0' border='0' width='100 % '><tr bgcolor='003DA5'><td><b>Data.Sentry.com Events</b></td></table></p>");
                header = @"<tr bgcolor='00A3E0'><td><b>Creation Date</b></td><td><b>Description</b></td><td><b>Initiator</b></td><td><b>Event Type</b></td><td><b>Expiration Date</b></td></tr>";
                body.Append(CreateEvents(header, EventTypeGroup.BusinessAreaDSC, DSCEvents));
            }

            myMail.Body = body.ToString();
            smtpClient.Send(myMail);
        }


        public string CreateEvents(string header, EventTypeGroup etGroup , List<Event> events)
        {
            StringBuilder body = new StringBuilder();

            body.Append(@"<table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100 %""  style=""background-color: aliceblue; "" > ");
            var groups = events.Where(x => x.Parent_Event != null).ToList();
            events.RemoveAll(x => x.Parent_Event != null);

            if (groups.Any())
            {
                List<IGrouping<string, Event>> groupedGroups = groups.GroupBy(x => x.Parent_Event).ToList();
                foreach (var group in groupedGroups)
                {
                    if (group.Count() > 1)
                    {
                        body.Append(header);
                        foreach (Event e in group)
                        {
                            body.Append(FormatEventLine(EventTypeGroup.DataSet, e));
                        }
                        body.Append(@"<tr></tr>");
                    }
                    else
                    {
                        events.Add(group.First());
                    }
                }
            }

            if (events.Count > 0)
            {
                body.Append(header);

                foreach (Event e in events.OrderBy(x => x.EventType.Severity))
                {
                    body.Append(FormatEventLine(etGroup, e));
                }
            }

            body.Append(@"</table>");

            return body.ToString();
        }


        public string FormatEventLine(EventTypeGroup group, Event e)
        {
            StringBuilder body = new StringBuilder();

            string columnStyle = @" style= ""vertical-align: top; padding-top:10px;"" ";                //Add style to add vertical alignment to each column and provide some space between rows
            body.Append(@" <tr>");
            body.Append(@" <td" + columnStyle   + " >" + e.TimeCreated + @"</td>");

            //EventTypeGroup's BusinessArea and BusinessAreaDSC both share same format
            if (group == EventTypeGroup.BusinessArea || group == EventTypeGroup.BusinessAreaDSC )
            {
                //BA Events Title and Message needs to be decoded because its stored as encoded HTML to show a RTF
                string reason = System.Net.WebUtility.HtmlDecode(e.Notification.Title);
                reason += "<br>" + System.Net.WebUtility.HtmlDecode(e.Notification.Message);                                                                                                                    
                body.Append(@"<td>" + reason + @"</td>");
            }
            else
            {
                body.Append(@"<td>" + e.Reason + @"</td>");
                body.Append(@"<td>" + e.Status.Description + @"</td>");
            }

            //Needed to resolve service accounts
            int n;
            var user = int.TryParse(e.UserWhoStartedEvent.Trim(), out n) ? _associateInfoProvider.GetAssociateInfo(e.UserWhoStartedEvent.Trim()).FullName : e.UserWhoStartedEvent.Trim();
            body.Append(@"<td" + columnStyle + ">" + user + @"</td>");
            body.Append(@"<td" + columnStyle + ">" + e.EventType.Description + @"</td>");

            if(group == EventTypeGroup.BusinessArea || group == EventTypeGroup.BusinessAreaDSC)
            {
                body.Append(@"<td" + columnStyle + ">" + e.Notification.ExpirationTime + @"</td>");
            }

            body.Append(@" </tr>");

            return body.ToString();
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
