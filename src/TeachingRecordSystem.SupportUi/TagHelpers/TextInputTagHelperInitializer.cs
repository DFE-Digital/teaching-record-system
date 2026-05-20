using GovUk.Frontend.AspNetCore.TagHelpers;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TeachingRecordSystem.SupportUi.TagHelpers;

public class TextInputTagHelperInitializer : ITagHelperInitializer<TextInputTagHelper>
{
    public void Initialize(TextInputTagHelper helper, ViewContext context)
    {
        helper.AutoComplete ??= "off";
    }
}
