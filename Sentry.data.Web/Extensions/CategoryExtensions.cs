using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public static class CategoryExtensions
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
            if(core is null || core.Count == 0) { return new List<CategoryModel>(); }

            List<CategoryModel> models = new List<CategoryModel>();
            core.ForEach(x => models.Add(x.ToModel()));

            return models;
        }

        public static IEnumerable<SelectListItem> ToEnumSelectList<TEnum>(this TEnum value, string selectedVal = null)
        {
            List<SelectListItem> items;

            if (selectedVal == null)
            {
                items = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Select(v => new SelectListItem { Text = v.ToString(), Value = ((Enum)(object)v).ToString() }).ToList();
            }
            else
            {
                items = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Select(v =>
                   new SelectListItem
                   {
                       Selected = (((int)(object)v).ToString() == selectedVal),
                       Text = v.ToString(),
                       Value = ((int)(object)v).ToString()
                   }).ToList();
            }

            return items;
        }
    }
}