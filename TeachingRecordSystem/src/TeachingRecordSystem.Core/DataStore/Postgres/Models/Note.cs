using System.Text;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class Note
{
    public required Guid NoteId { get; set; }
    public required Guid PersonId { get; set; }
    public required string ContentHtml { get; set; }
    public required DateTime? UpdatedOn { get; set; }
    public required DateTime CreatedOn { get; set; }
    public required Guid? CreatedByDqtUserId { get; set; }
    public required string? CreatedByDqtUserName { get; set; }
    public required Guid? UpdatedByDqtUserId { get; set; }
    public required string? UpdatedByDqtUserName { get; set; }
    public required string? FileName { get; set; }
    public required string? OriginalFileName { get; set; }

    public async Task<string> GetNoteTextWithoutHtmlAsync()
    {
        var text = ContentHtml;

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
