﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;

//
// see also:
// http://www.mattlunn.me.uk/blog/2014/08/how-to-dynamically-via-ajax-add-new-items-to-a-bound-list-model-in-asp-mvc-net/comment-page-1/
//

namespace Sentry.data.Web
{
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Generates a GUID-based editor template, rather than the index-based template generated by Html.EditorFor()
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="html"></param>
        /// <param name="propertyExpression">An expression which points to the property on the model you wish to generate the editor for</param>
        /// <param name="indexResolverExpression">An expression which points to the property on the model which holds the GUID index (optional, but required to make Validation* methods to work on post-back)</param>
        /// <param name="includeIndexField">
        /// True if you want this helper to render the hidden &lt;input /&gt; for you (default). False if you do not want this behaviour, and are instead going to call Html.EditorForManyIndexField() within the Editor view. 
        /// The latter behaviour is desired in situations where the Editor is being rendered inside lists or tables, where the &lt;input /&gt; would be invalid.
        /// </param>
        /// <returns>Generated HTML</returns>
        public static MvcHtmlString EditorForMany<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, IEnumerable<TValue>>> propertyExpression, Expression<Func<TValue, string>> indexResolverExpression = null, bool includeIndexField = true) where TModel : class
        {
            var items = propertyExpression.Compile()(html.ViewData.Model);
            var htmlBuilder = new StringBuilder();
            var htmlFieldName = ExpressionHelper.GetExpressionText(propertyExpression);
            var htmlFieldNameWithPrefix = html.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlFieldName);
            Func<TValue, string> indexResolver = null;

            if (indexResolverExpression == null)
            {
                indexResolver = x => null;
            }
            else
            {
                indexResolver = indexResolverExpression.Compile();
            }

            foreach (var item in items)
            {
                var dummy = new { Item = item };
                var guid = indexResolver(item);
                var memberExp = Expression.MakeMemberAccess(Expression.Constant(dummy), dummy.GetType().GetProperty("Item"));
                var singleItemExp = Expression.Lambda<Func<TModel, TValue>>(memberExp, propertyExpression.Parameters);

                if (String.IsNullOrEmpty(guid))
                {
                    guid = Guid.NewGuid().ToString();
                }
                else
                {
                    guid = html.AttributeEncode(guid);
                }

                if (includeIndexField)
                {
                    htmlBuilder.Append(_EditorForManyIndexField<TValue>(htmlFieldNameWithPrefix, guid, indexResolverExpression));
                }

                htmlBuilder.Append(html.EditorFor(singleItemExp, null, String.Format("{0}[{1}]", htmlFieldName, guid)));
            }

            return new MvcHtmlString(htmlBuilder.ToString());
        }

        /// <summary>
        /// Used to manually generate the hidden &lt;input /&gt;. To be used in conjunction with EditorForMany(), when "false" was passed for includeIndexField. 
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="html"></param>
        /// <param name="indexResolverExpression">An expression which points to the property on the model which holds the GUID index (optional, but required to make Validation* methods to work on post-back)</param>
        /// <returns>Generated HTML for hidden &lt;input /&gt;</returns>
        public static MvcHtmlString EditorForManyIndexField<TModel>(this HtmlHelper<TModel> html, Expression<Func<TModel, string>> indexResolverExpression = null)
        {
            var htmlPrefix = html.ViewData.TemplateInfo.HtmlFieldPrefix;
            var first = htmlPrefix.LastIndexOf('[');
            var last = htmlPrefix.IndexOf(']', first + 1);

            if (first == -1 || last == -1)
            {
                throw new InvalidOperationException("EditorForManyIndexField called when not in a EditorForMany context");
            }

            var htmlFieldNameWithPrefix = htmlPrefix.Substring(0, first);
            var guid = htmlPrefix.Substring(first + 1, last - first - 1);

            return _EditorForManyIndexField<TModel>(htmlFieldNameWithPrefix, guid, indexResolverExpression);
        }

        private static MvcHtmlString _EditorForManyIndexField<TModel>(string htmlFieldNameWithPrefix, string guid, Expression<Func<TModel, string>> indexResolverExpression)
        {
            var htmlBuilder = new StringBuilder();
            htmlBuilder.AppendFormat(@"<input type=""hidden"" name=""{0}.Index"" value=""{1}"" />", htmlFieldNameWithPrefix, guid);

            if (indexResolverExpression != null)
            {
                htmlBuilder.AppendFormat(@"<input type=""hidden"" name=""{0}[{1}].{2}"" value=""{1}"" />", htmlFieldNameWithPrefix, guid, ExpressionHelper.GetExpressionText(indexResolverExpression));
            }

            return new MvcHtmlString(htmlBuilder.ToString());
        }

        public static MvcHtmlString RemoveLink(this HtmlHelper htmlHelper, string linkText, string container, string deleteElement)
        {
            var js = string.Format("data.Images.removeNestedForm(this,'{0}','{1}');return false;", container, deleteElement);
            TagBuilder tb = new TagBuilder("a");
            tb.Attributes.Add("href", "#");
            tb.Attributes.Add("onclick", js);
            tb.InnerHtml = linkText;
            var tag = tb.ToString(TagRenderMode.Normal);
            return MvcHtmlString.Create(tag);
        }

        public static MvcHtmlString AddImageLink<TModel>(this HtmlHelper<TModel> htmlHelper, string linkText, string containerElement, string counterElement, string collectionProperty, Type nestedType)
        {
            var ticks = DateTime.UtcNow.Ticks;
            var nestedObject = Activator.CreateInstance(nestedType);
            var partial = htmlHelper.EditorFor(x => nestedObject).ToHtmlString().JsEncode();
            partial = partial.Replace("id=\\\"nestedObject", "id=\\\"" + collectionProperty + "_" + ticks + "_");
            partial = partial.Replace("name=\\\"nestedObject", "name=\\\"" + collectionProperty + "[" + ticks + "]");
            var js = string.Format("data.Images.addNestedForm('{0}','{1}','{2}','{3}');return false;", containerElement, counterElement, ticks, partial);
            TagBuilder tb = new TagBuilder("a");
            tb.Attributes.Add("href", "#");
            tb.Attributes.Add("onclick", js);
            tb.InnerHtml = linkText;
            var tag = tb.ToString(TagRenderMode.Normal);
            return MvcHtmlString.Create(tag);
        }

        private static string JsEncode(this string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            int i;
            int len = s.Length;
            StringBuilder sb = new StringBuilder(len + 4);
            string t;
            for (i = 0; i < len; i += 1)
            {
                char c = s[i];
                switch (c)
                {
                    case '>':
                    case '"':
                    case '\\':
                        sb.Append('\\');
                        sb.Append(c);
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    case '\n':
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\r':
                        break;
                    default:
                        if (c < ' ')
                        {
                            string tmp = new string(c, 1);
                            t = "000" + int.Parse(tmp, System.Globalization.NumberStyles.HexNumber);
                            sb.Append("\\u" + t.Substring(t.Length - 4));
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            return sb.ToString();
        }
    }
}