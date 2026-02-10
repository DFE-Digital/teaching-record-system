using System.Text.RegularExpressions;
using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace TeachingRecordSystem.Core.Services.Notify;

// See https://github.com/alphagov/notifications-utils/blob/c3b3711d9b63cf131151087be7726f7b9b1b0e55/notifications_utils/markdown.py#L155
internal partial class TemplateRenderer
{
    [GeneratedRegex(@"\(\(([^)]+)\)\)", RegexOptions.Compiled)]
    private static partial Regex PersonalizationPattern { get; }

    public string Render(string template, IReadOnlyDictionary<string, string> personalization, bool stripLinks = false)
    {
        var substituted = SubstitutePersonalization(template, personalization);

        var writer = new StringWriter();
        var pipeline = new MarkdownPipelineBuilder()
            .DisableHtml()
            .EnableTrackTrivia()
            .UseAutoLinks()
            .UseSoftlineBreakAsHardlineBreak()
            .Build();

        var renderer = new NotifyEmailHtmlRenderer(writer, stripLinks);
        pipeline.Setup(renderer);

        var document = Markdown.Parse(substituted, pipeline);
        renderer.Render(document);
        writer.Flush();

        return writer.ToString();
    }

    private static string SubstitutePersonalization(string template, IReadOnlyDictionary<string, string> personalization)
    {
        return PersonalizationPattern.Replace(template, match =>
        {
            var key = match.Groups[1].Value;
            return personalization.TryGetValue(key, out var value) ? value : match.Value;
        });
    }

    private class NotifyEmailHtmlRenderer : HtmlRenderer
    {
        private const string LinkStyle = "word-wrap: break-word; color: #1D70B8;";
        private const string ParagraphStyle = "Margin: 0 0 20px 0; font-size: 19px; line-height: 25px; color: #0B0C0C;";
        private const string H2Style = "Margin: 0 0 15px 0; padding: 10px 0 0 0; font-size: 27px; line-height: 35px; font-weight: bold; color: #0B0C0C;";
        private const string H3Style = "Margin: 0 0 15px 0; padding: 10px 0 0 0; font-size: 19px; line-height: 25px; font-weight: bold; color: #0B0C0C;";
        private const string HrStyle = "border: 0; height: 1px; background: #B1B4B6; Margin: 30px 0 30px 0;";
        private const string BlockquoteStyle = "Margin: 0; border-left: 10px solid #B1B4B6; padding: 15px 0 0.1px 15px; font-size: 19px; line-height: 25px;";
        private const string BlockquoteContainerStyle = "Margin: 0 0 20px 0;";
        private const string ListItemStyle = "Margin: 5px 0 5px; padding: 0 0 0 5px; font-size: 19px; line-height: 25px; color: #0B0C0C;";
        private const string ListStyle = "Margin: 0 0 0 20px; padding: 0;";
        private const string ListTableStyle = "padding: 0 0 20px 0;";
        private const string ListTableCellStyle = "font-family: Helvetica, Arial, sans-serif;";

        private readonly bool _stripLinks;

        public NotifyEmailHtmlRenderer(TextWriter writer, bool stripLinks) : base(writer)
        {
            _stripLinks = stripLinks;

            ObjectRenderers.Clear();

            ObjectRenderers.Add(new NotifyHeadingRenderer());
            ObjectRenderers.Add(new NotifyParagraphRenderer());
            ObjectRenderers.Add(new NotifyThematicBreakRenderer());
            ObjectRenderers.Add(new NotifyQuoteBlockRenderer());
            ObjectRenderers.Add(new NotifyListRenderer());
            ObjectRenderers.Add(new NotifyLinkInlineRenderer(stripLinks));
            ObjectRenderers.Add(new NotifyAutolinkInlineRenderer(stripLinks));
            ObjectRenderers.Add(new NotifyLineBreakInlineRenderer());
            ObjectRenderers.Add(new HtmlBlockRenderer());
            ObjectRenderers.Add(new CodeBlockRenderer());
            ObjectRenderers.Add(new LiteralInlineRenderer());
        }

        private class NotifyHeadingRenderer : HtmlObjectRenderer<HeadingBlock>
        {
            protected override void Write(HtmlRenderer renderer, HeadingBlock obj)
            {
                if (obj.Level == 1)
                {
                    renderer.Write($"<h2 style=\"{H2Style}\">");
                    renderer.WriteLeafInline(obj);
                    renderer.WriteLine("</h2>");
                }
                else if (obj.Level == 2)
                {
                    renderer.Write($"<h3 style=\"{H3Style}\">");
                    renderer.WriteLeafInline(obj);
                    renderer.WriteLine("</h3>");
                }
                else
                {
                    renderer.Write($"<p style=\"{ParagraphStyle}\">");
                    renderer.WriteLeafInline(obj);
                    renderer.WriteLine("</p>");
                }
            }
        }

        private class NotifyParagraphRenderer : HtmlObjectRenderer<ParagraphBlock>
        {
            protected override void Write(HtmlRenderer renderer, ParagraphBlock obj)
            {
                if (renderer.ImplicitParagraph)
                {
                    renderer.WriteLeafInline(obj);
                }
                else
                {
                    renderer.Write($"<p style=\"{ParagraphStyle}\">");
                    renderer.WriteLeafInline(obj);
                    renderer.WriteLine("</p>");
                }
            }
        }

        private class NotifyThematicBreakRenderer : HtmlObjectRenderer<ThematicBreakBlock>
        {
            protected override void Write(HtmlRenderer renderer, ThematicBreakBlock obj)
            {
                renderer.WriteLine($"<hr style=\"{HrStyle}\">");
            }
        }

        private class NotifyQuoteBlockRenderer : HtmlObjectRenderer<QuoteBlock>
        {
            protected override void Write(HtmlRenderer renderer, QuoteBlock obj)
            {
                renderer.WriteLine($"<div style=\"{BlockquoteContainerStyle}\">");
                renderer.WriteLine($"<blockquote style=\"{BlockquoteStyle}\">");
                renderer.WriteChildren(obj);
                renderer.WriteLine("</blockquote>");
                renderer.WriteLine("</div>");
            }
        }

        private class NotifyListRenderer : HtmlObjectRenderer<ListBlock>
        {
            protected override void Write(HtmlRenderer renderer, ListBlock obj)
            {
                renderer.WriteLine($"<table role=\"presentation\" style=\"{ListTableStyle}\">");
                renderer.WriteLine("<tr>");
                renderer.WriteLine($"<td style=\"{ListTableCellStyle}\">");

                if (obj.IsOrdered)
                {
                    renderer.WriteLine($"<ol style=\"{ListStyle} list-style-type: decimal;\">");
                }
                else
                {
                    renderer.WriteLine($"<ul style=\"{ListStyle} list-style-type: disc;\">");
                }

                foreach (var item in obj)
                {
                    var listItem = (ListItemBlock)item;

                    // Capture the list item content
                    var itemWriter = new StringWriter();
                    var itemRenderer = new HtmlRenderer(itemWriter);
                    itemRenderer.ImplicitParagraph = true;

                    // Copy over necessary renderers
                    foreach (var objectRenderer in renderer.ObjectRenderers)
                    {
                        itemRenderer.ObjectRenderers.Add(objectRenderer);
                    }

                    itemRenderer.WriteChildren(listItem);
                    itemWriter.Flush();
                    var content = itemWriter.ToString().Trim();

                    renderer.Write($"<li style=\"{ListItemStyle}\">{content}</li>");
                    renderer.WriteLine();
                }

                if (obj.IsOrdered)
                {
                    renderer.WriteLine("</ol>");
                }
                else
                {
                    renderer.WriteLine("</ul>");
                }

                renderer.WriteLine("</td>");
                renderer.WriteLine("</tr>");
                renderer.WriteLine("</table>");
            }
        }

        private class NotifyLinkInlineRenderer : HtmlObjectRenderer<LinkInline>
        {
            private readonly bool _stripLinks;

            public NotifyLinkInlineRenderer(bool stripLinks)
            {
                _stripLinks = stripLinks;
            }

            protected override void Write(HtmlRenderer renderer, LinkInline obj)
            {
                var url = obj.GetDynamicUrl?.Invoke() ?? obj.Url;

                if (!_stripLinks && !string.IsNullOrEmpty(url))
                {
                    var encodedUrl = System.Net.WebUtility.HtmlEncode(url);
                    renderer.Write($"<a style=\"{LinkStyle}\" href=\"{encodedUrl}\"");

                    if (!string.IsNullOrEmpty(obj.Title))
                    {
                        var encodedTitle = System.Net.WebUtility.HtmlEncode(obj.Title);
                        renderer.Write($" title=\"{encodedTitle}\"");
                    }

                    renderer.Write(">");
                }

                renderer.WriteChildren(obj);

                if (!_stripLinks && !string.IsNullOrEmpty(url))
                {
                    renderer.Write("</a>");
                }
            }
        }

        private class NotifyAutolinkInlineRenderer : HtmlObjectRenderer<AutolinkInline>
        {
            private readonly bool _stripLinks;

            public NotifyAutolinkInlineRenderer(bool stripLinks)
            {
                _stripLinks = stripLinks;
            }

            protected override void Write(HtmlRenderer renderer, AutolinkInline obj)
            {
                var url = obj.Url;

                if (!obj.IsEmail && !string.IsNullOrEmpty(url))
                {
                    var encodedUrl = System.Net.WebUtility.HtmlEncode(url);
                    if (_stripLinks)
                    {
                        renderer.Write(encodedUrl);
                    }
                    else
                    {
                        renderer.Write($"<a style=\"{LinkStyle}\" href=\"{encodedUrl}\">{encodedUrl}</a>");
                    }
                }
                else
                {
                    renderer.Write(url);
                }
            }
        }

        private class NotifyLineBreakInlineRenderer : HtmlObjectRenderer<LineBreakInline>
        {
            protected override void Write(HtmlRenderer renderer, LineBreakInline obj)
            {
                if (obj.IsHard)
                {
                    renderer.Write("<br>");
                }
                else
                {
                    renderer.WriteLine();
                }
            }
        }
    }
}
