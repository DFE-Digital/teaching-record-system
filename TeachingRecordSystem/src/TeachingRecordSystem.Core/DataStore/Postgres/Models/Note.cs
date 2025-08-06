using System.Diagnostics;
using System.Text;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class Note
{
    public required Guid NoteId { get; init; }
    public required Guid PersonId { get; init; }
    // Notes created in TRS have Content (plain text)
    public required string? Content { get; init; }
    // Notes migrated from DQT have ContentHtml
    public string? ContentHtml { get; set; }
    public required DateTime? UpdatedOn { get; set; }
    public required DateTime CreatedOn { get; init; }
    public required Guid? CreatedByUserId { get; init; }
    public User? CreatedBy { get; }
    public Guid? CreatedByDqtUserId { get; init; }
    public string? CreatedByDqtUserName { get; init; }
    public Guid? UpdatedByDqtUserId { get; set; }
    public string? UpdatedByDqtUserName { get; set; }
    public required Guid? FileId { get; init; }
    public required string? OriginalFileName { get; init; }

    public async Task<string> GetNoteContentAsync()
    {
        if (Content is not null)
        {
            return Content;
        }

        var text = ContentHtml;
        Debug.Assert(text is not null);

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

                var nodeText = node.Text();
                if (!string.IsNullOrWhiteSpace(nodeText))
                {
                    blocks.AppendLine(nodeText);
                }
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
