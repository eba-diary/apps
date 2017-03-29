using System.Collections.Generic;
using Sentry.Core;

namespace Sentry.data.Core
{
    public class Asset : IValidatable
    {
        private string _name;
        private string _description;
#pragma warning disable CS0649
        private int _id;
        private int _version;
#pragma warning restore CS0649
        private IList<Category> _categories = new List<Category>();

        protected Asset()
        {

        }

        public Asset(string name, string description)
        {
            this.Name = name;
            this.Description = description;
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

        public virtual string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
            }
        }

        public virtual int Id
        {
            get
            {
                return _id;
            }
        }

        public virtual int Version
        {
            get
            {
                return _version;
            }
        }

        public virtual AssetDynamicDetails DynamicDetails
        {
            get
            {
                return AssetDynamicDetailsService.GetByAssetId(_id);
            }
        }

        public virtual IEnumerable<Category> Categories
        {
            get
            {
                return _categories;
            }
        }

        public virtual void AddCategory(Category category)
        {
            if (_categories.Contains(category))
            {
                throw new ValidationException("Cannot add a duplicate category to an item");
            }
            _categories.Add(category);
        }

        public virtual void RemoveCategory(Category category)
        {
            if (!_categories.Contains(category))
            {
                throw new ValidationException("The item does not belong to the category " + category.Name);
            }
            _categories.Remove(category);
        }

        private void EnsureAssetIsUp(string propertyName)
        {
            if (DynamicDetails.State != AssetState.Up)
            {
                throw new ValidationException(propertyName + " is not up!");
            }
        }

        public virtual ValidationResults ValidateForDelete()
        {
            return new ValidationResults();
        }

        public virtual ValidationResults ValidateForSave()
        {
            ValidationResults vr = new ValidationResults();
            if (string.IsNullOrWhiteSpace(Name))
            {
                vr.Add(ValidationErrors.nameIsBlank, "The Name of the asset is required");
            }
            if (string.IsNullOrWhiteSpace(Description))
            {
                vr.Add(ValidationErrors.nameIsBlank, "The Description of the asset is required");
            }
            return vr;
        }

        public class ValidationErrors
        {
            public const string nameIsBlank = "nameIsBlank";
            public const string descriptionIsBlank = "descriptionIsBlank";
        }
    }
}
