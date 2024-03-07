using System.Text;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace TeachingRecordSystem.Core.Dqt.Models;

public static class AnnotationExtensions
{
    public static async Task<string> GetNoteTextWithoutHtml(this Annotation annotation)
    {
        var text = annotation.NoteText;

        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        using var context = BrowsingContext.New(Configuration.Default);
        using var doc = (IHtmlDocument)await context.OpenAsync(req => req.Content(text));

        var blocks = new StringBuilder();

        VisitDocumentNodes(
            doc,
            node =>
            {
                if (node.NodeType != NodeType.Text)
                {
                    return;
                }

                if (node.ParentElement is IHtmlScriptElement)
                {
                    return;
                }

                blocks.Append(node.Text());
            });

        return blocks.ToString();

        void VisitDocumentNodes(IHtmlDocument document, Action<INode> visit)
        {
            VisitNode(document.DocumentElement);

            void VisitNode(INode node)
            {
                visit(node);

                foreach (var child in node.GetDescendants())
                {
                    visit(child);
                }
            }
        }
    }
}
