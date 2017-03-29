using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class AssetStateModel
    {
        public static IEnumerable<AssetStateModel> GetAssetStateList()
        {
            List<AssetStateModel> list = new List<AssetStateModel>();
            foreach (int s in Enum.GetValues(typeof(AssetState)))
            {
                string name = Enum.GetName(typeof(AssetState), s);
                list.Add(new AssetStateModel(s, name));
            }
            return list;
        }

    /// <summary>
    /// Creates an AssetStateModel, and converts the Pascal-Case name into human-readable format
    /// </summary>
    public AssetStateModel(int id, string name)
    {
        this.Id = id;
        //convert PascalCase to human-readable format
        this.Name = Regex.Replace(name, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
    }

    public int Id { get; set; }
    public string Name { get; set; }
    }
}
