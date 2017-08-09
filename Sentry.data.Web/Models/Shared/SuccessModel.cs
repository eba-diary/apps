using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class SuccessModel
    {
        public SuccessModel(string title, string message, Boolean wasSuccess)
        {
            //if (wasSuccess)
            //{
            this.Title = title;
            this.Message = message;
            this.WasSuccessful = wasSuccess;
            //}
            //else
            //{
            //    this.Message = "<b>" + message + "</b>";
            //}
            
        }

        public string Title { get; set; }
        public string Message { get; set; }
        public Boolean WasSuccessful { get; set; } 

    }
}