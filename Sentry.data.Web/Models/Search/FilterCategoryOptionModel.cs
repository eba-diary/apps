namespace Sentry.data.Web
{
    public class FilterCategoryOptionModel
    {
        private string _id;

        public string OptionId 
        {             
            get => string.IsNullOrEmpty(ParentCategoryName) ? _id : ParentCategoryName.Replace(" ", "_") + "_" + _id;
            set => _id = value;
        }
        public string OptionName { get; set; }
        public string ParentCategoryName { get; set; }
        public int ResultCount { get; set; }
        public bool Selected { get; set; }
    }
}