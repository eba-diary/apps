using System;

namespace Sentry.data.Web
{
    public class FilterCategoryOptionModel
    {
        private string _id;
        private string _fullId;

        public string OptionId
        {
            get 
            {
                if (string.IsNullOrEmpty(_fullId))
                {
                    if (string.IsNullOrEmpty(_id))
                    {
                        _id = Guid.NewGuid().ToString();
                    }

                    return string.IsNullOrEmpty(ParentCategoryName) ? _id : ParentCategoryName.Replace(" ", "-") + "_" + _id;
                }

                return _fullId;
            }
            set => _fullId = value;
        }

        public string OptionValue { get; set; }
        public string ParentCategoryName { get; set; }
        public long ResultCount { get; set; }
        public bool Selected { get; set; }
    }
}