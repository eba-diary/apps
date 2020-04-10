using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;
using Sentry.data.Core.GlobalEnums;

namespace Sentry.data.Web
{
    public class NotificationModel
    {

        public NotificationModel() { }


        public int NotificationId { get; set; }


        [DisplayName("Expiration Time")] public DateTime ExpirationTime { get; set; }
        [DisplayName("Start Time")] public DateTime StartTime { get; set; }
        [DisplayName("Creator")] public string CreateUser { get; set; }
        [DisplayName("Severity")] public NotificationSeverity MessageSeverity { get; set; }
        [DisplayName("Asset")] public string ObjectId { get; set; }
        [DisplayName("Message")] public string Message { get; set; }
        [DisplayName("Title")] public string Title { get; set; }

        public Boolean IsActive { get; set; }
        public string ObjectName { get; set; }
        public string MessageSeverityDescription { get; set; }
        public bool CanEdit { get; set; }
        public string ObjectType { get; set; }

        public IEnumerable<SelectListItem> AllDataAssets { get; set; }
        public IEnumerable<SelectListItem> AllSeverities { get; set; }



        public List<string> Validate()
        {
            List<string> errors = new List<string>();
            if (this.ExpirationTime <= this.StartTime)
            {
                errors.Add("Expiration Time cannot be before Start Time");
            }
            if (this.StartTime >= this.ExpirationTime)
            {
                errors.Add("Start Time cannot be after Expiration Time");
            }
            if (this.StartTime <= DateTime.Now.AddHours(-1))
            {
                errors.Add("Start Time cannot be greater than an hour in the past");
            }
            if (this.ExpirationTime <= DateTime.Now)
            {
                errors.Add("Expiration Time cannot be in the past");
            }
            if (this.Title.Length > 250)
            {
                errors.Add("Title cannot be greater than 250 characters.");
            }
            if (this.Message.Length > 250)
            {
                errors.Add("Message cannot be greater than 250 characters.");
            }

            return errors;
        }
    }
}