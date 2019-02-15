
namespace Sentry.data.Web
{
    public class CategoryModel
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Color { get; set; }
        public virtual string ObjectType { get; set; }
        public virtual string AbbreviatedName { get; set; }
    }
}