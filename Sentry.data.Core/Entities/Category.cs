using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class Category
    {
#pragma warning disable CS0649
        private int _id;
#pragma warning restore CS0649
        private string _name;
        private string _color;
        private Category _parentCategory;
        private IList<Category> _subCategories = new List<Category>();

        protected Category()
        {

        }

        public Category(string name, Category parentCategory = null)
        {
            _name = name;
            this.ParentCategory = parentCategory;
        }

        public virtual string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public virtual Category ParentCategory
        {
            get
            {
                return _parentCategory;
            }
            set
            {
                //If the current parent is set, remove the child (me) from it
                if (_parentCategory != null)
                {
                    _parentCategory._subCategories.Remove(this);
                }
                _parentCategory = value;
                //Now, add me to the children of the new parent
                if (_parentCategory != null)
                {
                    _parentCategory._subCategories.Add(this);
                }
            }
        }

        public virtual int Id
        {
            get
            {
                return _id;
            }
        }

        public virtual IEnumerable<Category> SubCategories
        {
            get
            {
                return _subCategories;
            }
        }

        public virtual string FullName
        { 
            get
            {
                return this.ToString();
            }
        }

        public virtual string Color
        {
            get
            {
                return _color;
            }

            set
            {
                _color = value;
            }
        }

        public override string ToString()
        {
            if (ParentCategory != null)
            {
                return ParentCategory.ToString() + " > " + Name;
            }
            else
            {
                return Name;
            }
        }
    }
}
