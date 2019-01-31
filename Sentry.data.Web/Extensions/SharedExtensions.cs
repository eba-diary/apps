using System.Collections.Generic;

namespace Sentry.data.Web
{
    public static class SharedExtensions
    {

        public static CategoryModel ToModel(this Core.Category core)
        {
            if (core == null) { return new CategoryModel(); }
            return new CategoryModel()
            {
                AbbreviatedName = core.AbbreviatedName,
                Color = core.Color,
                Id = core.Id,
                Name = core.Name,
                ObjectType = core.ObjectType
            };
        }

        public static List<CategoryModel> ToModel(this List<Core.Category> core)
        {
            if(core?.Count == 0) { return new List<CategoryModel>(); }

            List<CategoryModel> models = new List<CategoryModel>();
            core.ForEach(x => models.Add(x.ToModel()));

            return models;
        }


    }
}