using Sentry.Core;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    [Serializable]
    //This approach is described at the site http://ivanz.com/2011/06/16/editing-variable-length-reorderable-collections-in-asp-net-mvc-part-1/
    public class MediaTypeExtension : IValidatable, IEqualityComparer<MediaTypeExtension>
    {
        public virtual int Index { get; set; }
        public virtual int Id { get; set; }
        public virtual string Key { get; set; }
        public virtual string Value { get; set; }        

        public virtual ValidationResults ValidateForDelete()
        {
            ValidationResults vr = new ValidationResults();
            return vr;
        }

        public virtual ValidationResults ValidateForSave()
        {
            ValidationResults vr = new ValidationResults();

            if (string.IsNullOrWhiteSpace(Key))
            {
                vr.Add(ValidationErrors.mediaTypeIsBlank, "The mediatype is required");
            }
            if (string.IsNullOrWhiteSpace(Value))
            {
                vr.Add(ValidationErrors.fileExtensionIsBlank, "The file extension S3 Key is required");
            }
            return vr;
        }

        public class ValidationErrors
        {
            public const string mediaTypeIsBlank = "mediaTypeIsBlank";
            public const string fileExtensionIsBlank = "fileExtensionIsBlank";
        }

        public virtual bool Equals(MediaTypeExtension x, MediaTypeExtension y)
        {
            return x.Key == y.Key && x.Value == y.Value;
        }

        public virtual int GetHashCode(MediaTypeExtension obj)
        {
            return (obj.Key + obj.Value).GetHashCode();
        }
    }
}
