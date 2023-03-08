using System.Collections.Generic;
using System.Net.Mail;
using Sentry.data.Core;
using System.Linq;
using System.Text;
using System;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.Common.Logging;
using Newtonsoft.Json;

namespace Sentry.data.Infrastructure
{
    public class EmailService : IEmailService
    {
        private IAssociateInfoProvider _associateInfoProvider;
        private readonly IDataFeatures _dataFeatures;
        private readonly IEmailClient _emailClient;

        public EmailService(IAssociateInfoProvider associateInfoProvider, IDataFeatures dataFeatures,IEmailClient emailClient)
        {
            _associateInfoProvider = associateInfoProvider;
            _dataFeatures = dataFeatures;
            _emailClient = emailClient;
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

        //SEND S3 CONNECTOR SINK EMAIL TO DSCSUPPORT TO INDICATE S3 SINK CONNECTOR RESULT
        public void SendS3SinkConnectorRequestEmail(DataFlow df, ConnectorCreateRequestDto requestDto, ConnectorCreateResponseDto responseDto)
        {
            //GET EMAIL INFO FROM CONFIG FILE AND VERIFY IT EXISTS
            string toString = Configuration.Config.GetHostSetting(GlobalConstants.HostSettings.S3SINKEMAILTO);
            string fromString = Configuration.Config.GetHostSetting(GlobalConstants.HostSettings.DATASETEMAIL);
            if(String.IsNullOrWhiteSpace(toString) || String.IsNullOrWhiteSpace(fromString))
            {
                return;
            }

            //START GETTING PARTS OF EMAIL
            MailAddress mailAddress = new MailAddress(fromString);
            MailMessage myMail = new System.Net.Mail.MailMessage();
            myMail.From = mailAddress;
            myMail.Subject = $"S3 SINK CONNECTOR CREATE {responseDto.SuccessStatusCodeDescription}"; 
            myMail.IsBodyHtml = true;
            myMail.Body = GetS3SinkConnectorEmailBody(df, requestDto, responseDto);

            foreach (var address in toString.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                myMail.To.Add(address);
            }

            //SEND EMAIL
            _emailClient.Send(myMail);

            Logger.Info($"Method <SendS3SinkConnectorRequestEmail> S3_SINK_CONNECTOR_REQUEST_EMAIL Successfully Sent. Here are Details.  FROM: {fromString} TO: {toString} Body: {myMail.Body}");
        }

        public void SendNewMotiveTokenAddedEmail(DataSourceToken token)
        {
            string toString = Configuration.Config.GetHostSetting(GlobalConstants.HostSettings.MOTIVEEMAILTO);
            string fromString = Configuration.Config.GetHostSetting(GlobalConstants.HostSettings.DATASETEMAIL);
            if (String.IsNullOrWhiteSpace(toString) || String.IsNullOrWhiteSpace(fromString))
            {
                Logger.Error("Tried to send Motive Token email with incomplete sender/recipient.");
                return;
            }

            MailAddress mailAddress = new MailAddress(fromString);
            MailMessage myMail = new MailMessage()
            {
                From = mailAddress,
                Subject = $"New Motive Token Added: {token.TokenName ?? "Company Name Pending"}",
                IsBodyHtml = true,
                Body = "<p>New OAuth Token Received, Contact <a href='mailto:DSCSupport@sentry.com'>DSC Support</a> with any questions.</p>"
            };

            foreach (var address in toString.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                myMail.To.Add(address);
            }

            _emailClient.Send(myMail);
        }
        
        public void SendMotiveDuplicateTokenEmail(DataSourceToken newToken, DataSourceToken oldToken)
        {
            string toString = Configuration.Config.GetHostSetting(GlobalConstants.HostSettings.MOTIVEEMAILTO);
            string fromString = Configuration.Config.GetHostSetting(GlobalConstants.HostSettings.DATASETEMAIL);
            if (String.IsNullOrWhiteSpace(toString) || String.IsNullOrWhiteSpace(fromString))
            {
                Logger.Error("Tried to send Motive Duplicate Token email with incomplete sender/recipient.");
                return;
            }

            MailAddress mailAddress = new MailAddress(fromString);
            MailMessage myMail = new MailMessage()
            {
                From = mailAddress,
                Subject = $"Duplicate Motive Token Received",
                IsBodyHtml = true,
                Body = $"<p>Token {oldToken.TokenName} {oldToken.Id} has same ID ({oldToken.ForeignId}) as {newToken.TokenName} {newToken.Id}. Review both tokens and verify the correct one is enabled, and the other is deleted. Contact <a href='mailto:DSCSupport@sentry.com'>DSC Support</a> with any questions.</p>"
            };

            foreach (var address in toString.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                myMail.To.Add(address);
            }

            _emailClient.Send(myMail);
        }

        //GET BODY OF S3 SINK EMAIL
        private string GetS3SinkConnectorEmailBody(DataFlow df, ConnectorCreateRequestDto requestDto, ConnectorCreateResponseDto responseDto)
        {
            StringBuilder builder = new StringBuilder();

            /***********************************************************************************************************************************
            S3 SINK CONNECTOR REQUEST RESPONSE
            ***********************************************************************************************************************************/
            //TABLE HEADER
            builder.Append(@"</p><table cellpadding='0' cellspacing='0' border='0' width='100 % '><tr bgcolor='003DA5'><td><b>S3SinkConnector Response from API:</b></td></table></p>");

            //TABLE THAT HOLDS DETAIL
            builder.Append(@"<table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100 %""  style=""background-color: aliceblue; "" > ");
            //DETAIL
            builder.Append(GetS3SinkConnectorEmailSingleRow("Success Status", responseDto.SuccessStatusCodeDescription));
            builder.Append(GetS3SinkConnectorEmailSingleRow("Status Code", responseDto.StatusCode));
            builder.Append(GetS3SinkConnectorEmailSingleRow("Reason Phrase", responseDto.ReasonPhrase));
            builder.Append(@"</table>");




            /***********************************************************************************************************************************
             S3 SINK CONNECTOR REQUEST
            ***********************************************************************************************************************************/
            //TABLE HEADER
            builder.Append(@"</p><table cellpadding='0' cellspacing='0' border='0' width='100 % '><tr bgcolor='003DA5'><td><b>S3SinkConnector Request to API:</b></td></table></p>");

            //TABLE THAT HOLDS DETAIL
            builder.Append(@"<table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100 %""  style=""background-color: aliceblue; "" > ");
            
            //DETAIL
            builder.Append(@" <tr>");
            builder.Append(@"<td>" + JsonConvert.SerializeObject(requestDto) + @" </td>");
            builder.Append(@" </tr>");
            builder.Append(@"</table>");



            /***********************************************************************************************************************************
             DATAFLOW INFO
            ***********************************************************************************************************************************/
            //TABLE HEADER
            builder.Append(@"</p><table cellpadding='0' cellspacing='0' border='0' width='100 % '><tr bgcolor='003DA5'><td><b>DATAFLOW DETAILS:</b></td></table></p>");

            //TABLE THAT HOLDS ROWS
            builder.Append(@"<table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100 %""  style=""background-color: aliceblue; "" > ");

            //SINGLE ROWS
            builder.Append(GetS3SinkConnectorEmailSingleRow("DataFlowURL", GetS3SinkConnectorDataFlowUrl(df)));
            builder.Append(GetS3SinkConnectorEmailSingleRow(nameof(df.TopicName), df.TopicName));
            builder.Append(GetS3SinkConnectorEmailSingleRow(nameof(df.S3ConnectorName), df.S3ConnectorName));
            builder.Append(GetS3SinkConnectorEmailSingleRow(nameof(df.FlowStorageCode), df.FlowStorageCode));
            builder.Append(GetS3SinkConnectorEmailSingleRow(nameof(df.NamedEnvironment), df.NamedEnvironment));
            builder.Append(GetS3SinkConnectorEmailSingleRow(nameof(df.SaidKeyCode), df.SaidKeyCode));
            builder.Append(GetS3SinkConnectorEmailSingleRow("DataFlowName", df.Name));
            builder.Append(GetS3SinkConnectorEmailSingleRow(nameof(df.CreatedBy), df.CreatedBy));
            builder.Append(GetS3SinkConnectorEmailSingleRow("DataFlowId", df.Id.ToString()));
            builder.Append(GetS3SinkConnectorEmailSingleRow(nameof(df.IngestionType), df.IngestionType.ToString()));
            builder.Append(GetS3SinkConnectorEmailSingleRow(nameof(df.DatasetId), df.DatasetId.ToString()));
            builder.Append(GetS3SinkConnectorEmailSingleRow(nameof(df.SchemaId), df.SchemaId.ToString()));
            builder.Append(@"</table>");


            return builder.ToString();
        }

        //CREATE S3 SINK URL
        private string GetS3SinkConnectorDataFlowUrl(DataFlow df)
        {
            string url = Configuration.Config.GetHostSetting(GlobalConstants.HostSettings.MAIN_WEB_URL);
            url += $"/DataFlow/{df.Id}/Detail";
            return url;
        }


        //BUILD SINGLE S3 SINK EMAIL ROW
        private string GetS3SinkConnectorEmailSingleRow(string label, string value)
        {
            StringBuilder builder = new StringBuilder();
            
            builder.Append(@" <tr>");
            builder.Append(@"<td>" + label + @"</td>");
            builder.Append(@"<td>" + value + @" </td>");
            builder.Append(@" </tr>");

            return builder.ToString();
        }
    }
}
