using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using INTEX_II_413.Models.ViewModels;

namespace INTEX_II_413.Infrastructure
{
    [HtmlTargetElement("div", Attributes = "page-model")]

    // inherit from taghelper
    public class PaginationTagHelper : TagHelper
    {
        // help build url
        private IUrlHelperFactory urlHelperFactory;

        public PaginationTagHelper(IUrlHelperFactory temp)
        {
            urlHelperFactory = temp;
        }

        // give info about view context
        [ViewContext]

        // makes attribute not bound to tag so person cannot type in it
        [HtmlAttributeNotBound]
        public ViewContext? ViewContext { get; set; }

        // get what page user is on
        public string? PageAction { get; set; }

        // makes attribute not bound to tag so person cannot type in it
        [HtmlAttributeName(DictionaryAttributePrefix = "page-url-")]

        // store string a key, value as object
        public Dictionary<string, object> PageUrlValues { get; set; } = new Dictionary<string, object>();

        public PaginationInfo PageModel { get; set; }

        // set up options in tag helper
        public bool PageClassesEnabled { get; set; } = false;
        public string PageClass { get; set; } = String.Empty;
        public string PageClassNormal { get; set; } = String.Empty;
        public string PageClassSelected { get; set; } = String.Empty;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (ViewContext != null && PageModel != null)
            {
                IUrlHelper urlHelper = urlHelperFactory.GetUrlHelper(ViewContext);
                TagBuilder result = new TagBuilder("div");

                // Merge existing query parameters with PageUrlValues
                var mergedUrlValues = new Dictionary<string, object>();
                foreach (var queryParam in ViewContext.HttpContext.Request.Query)
                {
                    if (!PageUrlValues.ContainsKey(queryParam.Key))
                    {
                        mergedUrlValues[queryParam.Key] = queryParam.Value;
                    }
                }
                foreach (var kvp in PageUrlValues)
                {
                    mergedUrlValues[kvp.Key] = kvp.Value;
                }

                for (int i = 1; i <= PageModel.TotalPages; i++)
                {
                    // build tag
                    TagBuilder tag = new TagBuilder("a");

                    // get values from page url and set page number to it
                    mergedUrlValues["pageNum"] = i;

                    // set href for tag
                    tag.Attributes["href"] = urlHelper.Action(PageAction, mergedUrlValues);

                    if (PageClassesEnabled)
                    {
                        tag.AddCssClass(PageClass);

                        // if tag is for current page, use the page class selected, if not use page class normal
                        tag.AddCssClass(i == PageModel.CurrentPage ? PageClassSelected : PageClassNormal);
                    }

                    // set inner html
                    tag.InnerHtml.Append(i.ToString());

                    // pass in tag
                    result.InnerHtml.AppendHtml(tag);
                }

                // append result to screen
                output.Content.AppendHtml(result.InnerHtml);
            }
        }

    }
}

