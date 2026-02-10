namespace TeachingRecordSystem.Core.Services.Notify;

public class TemplateRendererTests
{
    private readonly TemplateRenderer _renderer;

    public TemplateRendererTests()
    {
        _renderer = new TemplateRenderer();
    }

    [Fact]
    public void Render_WithEmptyTemplate_ReturnsEmptyString()
    {
        // Arrange
        var template = "";
        var personalization = new Dictionary<string, string>();

        // Act
        var result = _renderer.Render(template, personalization);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void Render_WithPlainText_WrapsParagraphWithStyles()
    {
        // Arrange
        var template = "Hello World";
        var personalization = new Dictionary<string, string>();

        // Act
        var result = _renderer.Render(template, personalization);

        // Assert
        Assert.Contains("<p style=\"Margin: 0 0 20px 0; font-size: 19px; line-height: 25px; color: #0B0C0C;\">Hello World</p>", result);
    }

    [Fact]
    public void Render_WithPersonalization_SubstitutesValues()
    {
        // Arrange
        var template = "Hello ((name))!";
        var personalization = new Dictionary<string, string>
        {
            ["name"] = "John"
        };

        // Act
        var result = _renderer.Render(template, personalization);

        // Assert
        Assert.Contains("Hello John!", result);
    }

    [Fact]
    public void Render_WithMissingPersonalizationKey_LeavesPlaceholder()
    {
        // Arrange
        var template = "Hello ((name))!";
        var personalization = new Dictionary<string, string>();

        // Act
        var result = _renderer.Render(template, personalization);

        // Assert
        Assert.Contains("((name))", result);
    }

    [Fact]
    public void Render_WithMultiplePersonalization_SubstitutesAll()
    {
        // Arrange
        var template = "Hello ((first_name)) ((last_name))!";
        var personalization = new Dictionary<string, string>
        {
            ["first_name"] = "John",
            ["last_name"] = "Doe"
        };

        // Act
        var result = _renderer.Render(template, personalization);

        // Assert
        Assert.Contains("Hello John Doe!", result);
    }

    [Fact]
    public void Render_WithH1Heading_RendersAsH2WithStyles()
    {
        // Arrange
        var template = "# Main Heading";
        var personalization = new Dictionary<string, string>();

        // Act
        var result = _renderer.Render(template, personalization);

        // Assert
        Assert.Contains("<h2 style=\"Margin: 0 0 15px 0; padding: 10px 0 0 0; font-size: 27px; line-height: 35px; font-weight: bold; color: #0B0C0C;\">Main Heading</h2>", result);
    }

    [Fact]
    public void Render_WithH2Heading_RendersAsH3WithStyles()
    {
        // Arrange
        var template = "## Sub Heading";
        var personalization = new Dictionary<string, string>();

        // Act
        var result = _renderer.Render(template, personalization);

        // Assert
        Assert.Contains("<h3 style=\"Margin: 0 0 15px 0; padding: 10px 0 0 0; font-size: 19px; line-height: 25px; font-weight: bold; color: #0B0C0C;\">Sub Heading</h3>", result);
    }

    [Fact]
    public void Render_WithH3Heading_RendersAsParagraph()
    {
        // Arrange
        var template = "### Small Heading";
        var personalization = new Dictionary<string, string>();

        // Act
        var result = _renderer.Render(template, personalization);

        // Assert
        Assert.Contains("<p style=\"Margin: 0 0 20px 0; font-size: 19px; line-height: 25px; color: #0B0C0C;\">Small Heading</p>", result);
    }

    [Fact]
    public void Render_WithUnorderedList_RendersWithDiscBullets()
    {
        // Arrange
        var template = """
            * Item 1
            * Item 2
            * Item 3
            """;
        var personalization = new Dictionary<string, string>();

        // Act
        var result = _renderer.Render(template, personalization);

        // Assert
        Assert.Contains("<table role=\"presentation\" style=\"padding: 0 0 20px 0;\">", result);
        Assert.Contains("<ul style=\"Margin: 0 0 0 20px; padding: 0; list-style-type: disc;\">", result);
        Assert.Contains("<li style=\"Margin: 5px 0 5px; padding: 0 0 0 5px; font-size: 19px; line-height: 25px; color: #0B0C0C;\">Item 1</li>", result);
        Assert.Contains("<li style=\"Margin: 5px 0 5px; padding: 0 0 0 5px; font-size: 19px; line-height: 25px; color: #0B0C0C;\">Item 2</li>", result);
        Assert.Contains("<li style=\"Margin: 5px 0 5px; padding: 0 0 0 5px; font-size: 19px; line-height: 25px; color: #0B0C0C;\">Item 3</li>", result);
    }

    [Fact]
    public void Render_WithOrderedList_RendersWithDecimalNumbers()
    {
        // Arrange
        var template = """
            1. First
            2. Second
            3. Third
            """;
        var personalization = new Dictionary<string, string>();

        // Act
        var result = _renderer.Render(template, personalization);

        // Assert
        Assert.Contains("<table role=\"presentation\" style=\"padding: 0 0 20px 0;\">", result);
        Assert.Contains("<ol style=\"Margin: 0 0 0 20px; padding: 0; list-style-type: decimal;\">", result);
        Assert.Contains("<li style=\"Margin: 5px 0 5px; padding: 0 0 0 5px; font-size: 19px; line-height: 25px; color: #0B0C0C;\">First</li>", result);
    }

    [Fact]
    public void Render_WithLink_RendersWithStyles()
    {
        // Arrange
        var template = "[Click here](https://example.com)";
        var personalization = new Dictionary<string, string>();

        // Act
        var result = _renderer.Render(template, personalization);

        // Assert
        Assert.Contains("<a style=\"word-wrap: break-word; color: #1D70B8;\" href=\"https://example.com\">Click here</a>", result);
    }

    [Fact]
    public void Render_WithLinkAndTitle_RendersWithTitleAttribute()
    {
        // Arrange
        var template = "[Click here](https://example.com \"Example Site\")";
        var personalization = new Dictionary<string, string>();

        // Act
        var result = _renderer.Render(template, personalization);

        // Assert
        Assert.Contains("<a style=\"word-wrap: break-word; color: #1D70B8;\" href=\"https://example.com\" title=\"Example Site\">Click here</a>", result);
    }

    [Fact]
    public void Render_WithAutolink_RendersWithStyles()
    {
        // Arrange
        var template = "Visit https://example.com for more info";
        var personalization = new Dictionary<string, string>();

        // Act
        var result = _renderer.Render(template, personalization);

        // Assert
        Assert.Contains("<a style=\"word-wrap: break-word; color: #1D70B8;\" href=\"https://example.com\">https://example.com</a>", result);
    }

    [Fact]
    public void Render_WithEmailAutolink_DoesNotRenderAsLink()
    {
        // Arrange
        var template = "Email: user@example.com";
        var personalization = new Dictionary<string, string>();

        // Act
        var result = _renderer.Render(template, personalization);

        // Assert
        Assert.DoesNotContain("<a", result);
        Assert.Contains("user@example.com", result);
    }

    [Fact]
    public void Render_WithBlockquote_RendersWithStyles()
    {
        // Arrange
        var template = "> This is a quote";
        var personalization = new Dictionary<string, string>();

        // Act
        var result = _renderer.Render(template, personalization);

        // Assert
        Assert.Contains("<div style=\"Margin: 0 0 20px 0;\">", result);
        Assert.Contains("<blockquote style=\"Margin: 0; border-left: 10px solid #B1B4B6; padding: 15px 0 0.1px 15px; font-size: 19px; line-height: 25px;\">", result);
        Assert.Contains("This is a quote", result);
        Assert.Contains("</blockquote>", result);
    }

    [Fact]
    public void Render_WithHorizontalRule_RendersWithStyles()
    {
        // Arrange
        var template = """
            Before

            ---

            After
            """;
        var personalization = new Dictionary<string, string>();

        // Act
        var result = _renderer.Render(template, personalization);

        // Assert
        Assert.Contains("<hr style=\"border: 0; height: 1px; background: #B1B4B6; Margin: 30px 0 30px 0;\">", result);
    }

    [Fact]
    public void Render_WithLineBreak_RendersAsBr()
    {
        // Arrange
        var template = "Line 1  \nLine 2";
        var personalization = new Dictionary<string, string>();

        // Act
        var result = _renderer.Render(template, personalization);

        // Assert - check that br tag exists and both lines are present
        Assert.Contains("<br>", result);
        Assert.Contains("Line 1", result);
        Assert.Contains("Line 2", result);
    }

    [Fact]
    public void Render_WithComplexTemplate_RendersAllElementsCorrectly()
    {
        // Arrange
        var template = """
            # Welcome ((name))

            Thank you for registering!

            ## What happens next

            We will review your application and:

            * Check your details
            * Verify your documents
            * Send you a confirmation

            For more information, visit [our website](https://example.com).

            > Please keep this email for your records.

            ---

            Contact us at support@example.com
            """;
        var personalization = new Dictionary<string, string>
        {
            ["name"] = "Jane"
        };

        // Act
        var result = _renderer.Render(template, personalization);

        // Assert
        Assert.Contains("Welcome Jane", result);
        Assert.Contains("<h2 style=", result);
        Assert.Contains("<h3 style=", result);
        Assert.Contains("<ul style=", result);
        Assert.Contains("<a style=", result);
        Assert.Contains("<blockquote style=", result);
        Assert.Contains("<hr style=", result);
    }

    [Fact]
    public void Render_DisablesHtmlTags()
    {
        // Arrange
        var template = "This has <script>alert('xss')</script> tags";
        var personalization = new Dictionary<string, string>();

        // Act
        var result = _renderer.Render(template, personalization);

        // Assert
        Assert.DoesNotContain("<script>", result);
        Assert.Contains("&lt;script&gt;", result);
    }

    [Fact]
    public void Render_WithBulletPoint_SupportsAlternativeSyntax()
    {
        // Arrange
        var template = """
            - Item 1
            - Item 2
            """;
        var personalization = new Dictionary<string, string>();

        // Act
        var result = _renderer.Render(template, personalization);

        // Assert
        Assert.Contains("<ul style=", result);
        Assert.Contains("Item 1", result);
        Assert.Contains("Item 2", result);
    }

    [Fact]
    public void Render_WithStripLinksTrue_DoesNotRenderAnchorTags()
    {
        // Arrange
        var template = "[Click here](https://example.com)";
        var personalization = new Dictionary<string, string>();

        // Act
        var result = _renderer.Render(template, personalization, stripLinks: true);

        // Assert
        Assert.DoesNotContain("<a", result);
        Assert.Contains("Click here", result);
    }

    [Fact]
    public void Render_WithStripLinksFalse_RendersAnchorTags()
    {
        // Arrange
        var template = "[Click here](https://example.com)";
        var personalization = new Dictionary<string, string>();

        // Act
        var result = _renderer.Render(template, personalization, stripLinks: false);

        // Assert
        Assert.Contains("<a style=\"word-wrap: break-word; color: #1D70B8;\" href=\"https://example.com\">Click here</a>", result);
    }

    [Fact]
    public void Render_WithStripLinksDefault_RendersAnchorTags()
    {
        // Arrange
        var template = "[Click here](https://example.com)";
        var personalization = new Dictionary<string, string>();

        // Act
        var result = _renderer.Render(template, personalization);

        // Assert
        Assert.Contains("<a style=\"word-wrap: break-word; color: #1D70B8;\" href=\"https://example.com\">Click here</a>", result);
    }

    [Fact]
    public void Render_WithStripLinksTrue_AutolinkDoesNotRenderAnchorTags()
    {
        // Arrange
        var template = "Visit https://example.com for more info";
        var personalization = new Dictionary<string, string>();

        // Act
        var result = _renderer.Render(template, personalization, stripLinks: true);

        // Assert
        Assert.DoesNotContain("<a", result);
        Assert.Contains("https://example.com", result);
        Assert.Contains("Visit", result);
        Assert.Contains("for more info", result);
    }

    [Fact]
    public void Render_WithStripLinksTrue_MultipleLinks_DoesNotRenderAnyAnchorTags()
    {
        // Arrange
        var template = """
            Check out [our website](https://example.com) or visit https://docs.example.com directly.
            """;
        var personalization = new Dictionary<string, string>();

        // Act
        var result = _renderer.Render(template, personalization, stripLinks: true);

        // Assert
        Assert.DoesNotContain("<a", result);
        Assert.Contains("our website", result);
        Assert.Contains("https://docs.example.com", result);
    }
}
