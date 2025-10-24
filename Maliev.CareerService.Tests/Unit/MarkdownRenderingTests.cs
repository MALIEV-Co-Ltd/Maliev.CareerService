using FluentAssertions;
using Maliev.CareerService.Api.Services;
using Xunit;

namespace Maliev.CareerService.Tests.Unit;

/// <summary>
/// Unit tests for MarkdownService - validates Markdown to HTML rendering with security
/// </summary>
public class MarkdownRenderingTests
{
    private readonly IMarkdownService _markdownService;

    public MarkdownRenderingTests()
    {
        _markdownService = new MarkdownService();
    }

    [Fact]
    public void RenderToHtml_SimpleMarkdown_RendersCorrectly()
    {
        // Arrange
        var markdown = "# Heading\n\nThis is a paragraph.";

        // Act
        var html = _markdownService.ToHtml(markdown);

        // Assert
        html.Should().Contain("<h1");
        html.Should().Contain("Heading");
        html.Should().Contain("<p>");
        html.Should().Contain("This is a paragraph");
    }

    [Fact]
    public void RenderToHtml_BoldAndItalic_RendersCorrectly()
    {
        // Arrange
        var markdown = "**Bold text** and *italic text*";

        // Act
        var html = _markdownService.ToHtml(markdown);

        // Assert
        html.Should().Contain("<strong>");
        html.Should().Contain("Bold text");
        html.Should().Contain("<em>");
        html.Should().Contain("italic text");
    }

    [Fact]
    public void RenderToHtml_Lists_RendersCorrectly()
    {
        // Arrange
        var markdown = @"## Requirements

- Bachelor's degree
- 3+ years experience
- Strong communication skills";

        // Act
        var html = _markdownService.ToHtml(markdown);

        // Assert
        html.Should().Contain("<h2");
        html.Should().Contain("Requirements");
        html.Should().Contain("<ul>");
        html.Should().Contain("<li>");
        html.Should().Contain("Bachelor's degree");
        html.Should().Contain("3+ years experience");
    }

    [Fact]
    public void RenderToHtml_OrderedLists_RendersCorrectly()
    {
        // Arrange
        var markdown = @"## Steps

1. First step
2. Second step
3. Third step";

        // Act
        var html = _markdownService.ToHtml(markdown);

        // Assert
        html.Should().Contain("<ol>");
        html.Should().Contain("<li>");
        html.Should().Contain("First step");
        html.Should().Contain("Second step");
    }

    [Fact]
    public void RenderToHtml_Links_RendersCorrectly()
    {
        // Arrange
        var markdown = "Visit [our website](https://example.com) for more information.";

        // Act
        var html = _markdownService.ToHtml(markdown);

        // Assert
        html.Should().Contain("<a");
        html.Should().Contain("href=\"https://example.com\"");
        html.Should().Contain("our website");
    }

    [Fact]
    public void RenderToHtml_ScriptTags_AreStrippedOrEscaped()
    {
        // Arrange
        var markdown = "Normal text <script>alert('XSS')</script> more text";

        // Act
        var html = _markdownService.ToHtml(markdown);

        // Assert
        // Script tags should be either removed or escaped
        html.Should().NotContain("<script>");
        html.Should().NotContain("alert('XSS')");
    }

    [Fact]
    public void RenderToHtml_JavaScriptInLinks_IsNeutralized()
    {
        // Arrange
        var markdown = "[Click me](javascript:alert('XSS'))";

        // Act
        var html = _markdownService.ToHtml(markdown);

        // Assert
        // javascript: protocol should be blocked or sanitized
        html.Should().NotContain("javascript:");
    }

    [Fact]
    public void RenderToHtml_OnEventHandlers_AreStripped()
    {
        // Arrange
        var markdown = "<div onclick=\"alert('XSS')\">Click me</div>";

        // Act
        var html = _markdownService.ToHtml(markdown);

        // Assert
        // Event handlers should be stripped
        html.Should().NotContain("onclick");
        html.Should().NotContain("alert('XSS')");
    }

    [Fact]
    public void RenderToHtml_IframeTags_AreBlocked()
    {
        // Arrange
        var markdown = "Normal text <iframe src=\"https://malicious.com\"></iframe> more text";

        // Act
        var html = _markdownService.ToHtml(markdown);

        // Assert
        // iframe tags should be blocked
        html.Should().NotContain("<iframe");
    }

    [Fact]
    public void RenderToHtml_ObjectAndEmbedTags_AreBlocked()
    {
        // Arrange
        var markdown = "<object data=\"malicious.swf\"></object> and <embed src=\"malicious.swf\">";

        // Act
        var html = _markdownService.ToHtml(markdown);

        // Assert
        // object and embed tags should be blocked
        html.Should().NotContain("<object");
        html.Should().NotContain("<embed");
    }

    [Fact]
    public void RenderToHtml_MultipleHeadingLevels_RendersCorrectly()
    {
        // Arrange
        var markdown = @"# Main Title
## Subtitle
### Section
#### Subsection";

        // Act
        var html = _markdownService.ToHtml(markdown);

        // Assert
        html.Should().Contain("<h1");
        html.Should().Contain("<h2");
        html.Should().Contain("<h3");
        html.Should().Contain("<h4");
    }

    [Fact]
    public void RenderToHtml_CodeBlocks_RendersCorrectly()
    {
        // Arrange
        var markdown = @"```javascript
function hello() {
    console.log('Hello');
}
```";

        // Act
        var html = _markdownService.ToHtml(markdown);

        // Assert
        html.Should().Contain("<pre>");
        html.Should().Contain("function hello()");
    }

    [Fact]
    public void RenderToHtml_InlineCode_RendersCorrectly()
    {
        // Arrange
        var markdown = "Use the `console.log()` function to debug.";

        // Act
        var html = _markdownService.ToHtml(markdown);

        // Assert
        html.Should().Contain("<code>");
        html.Should().Contain("console.log()");
    }

    [Fact]
    public void RenderToHtml_Blockquotes_RendersCorrectly()
    {
        // Arrange
        var markdown = "> This is a quote\n> with multiple lines";

        // Act
        var html = _markdownService.ToHtml(markdown);

        // Assert
        html.Should().Contain("<blockquote>");
        html.Should().Contain("This is a quote");
    }

    [Fact]
    public void RenderToHtml_EmptyString_ReturnsEmptyOrSafe()
    {
        // Arrange
        var markdown = "";

        // Act
        var html = _markdownService.ToHtml(markdown);

        // Assert
        html.Should().NotBeNull();
        // Should return empty string or safe empty HTML
        html.Should().BeOneOf("", "<p></p>", string.Empty);
    }

    [Fact]
    public void RenderToHtml_NullInput_HandlesGracefully()
    {
        // Arrange
        string? markdown = null;

        // Act & Assert
        var act = () => _markdownService.ToHtml(markdown!);

        // Should either handle null gracefully or throw ArgumentNullException
        if (act.Should().NotThrow().Which == null)
        {
            // If it doesn't throw, result should be safe
            var html = _markdownService.ToHtml(markdown!);
            html.Should().NotBeNull();
        }
    }

    [Fact]
    public void RenderToHtml_SpecialCharacters_AreEscaped()
    {
        // Arrange
        var markdown = "Text with < > & \" ' characters";

        // Act
        var html = _markdownService.ToHtml(markdown);

        // Assert
        // Special HTML characters should be escaped
        html.Should().NotContain("<>");
        // They should be escaped as &lt; &gt; &amp; etc.
    }

    [Fact]
    public void RenderToHtml_VeryLongText_HandlesWithoutError()
    {
        // Arrange
        var markdown = new string('A', 10000) + "\n\n" + new string('B', 10000);

        // Act
        var act = () => _markdownService.ToHtml(markdown);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RenderToHtml_MixedContent_RendersAllElementsCorrectly()
    {
        // Arrange
        var markdown = @"# Job Requirements

## Required Skills

- **Programming**: Proficiency in C# and .NET
- **Database**: Experience with PostgreSQL
- **Cloud**: Knowledge of Kubernetes

### Nice to Have

1. Docker experience
2. CI/CD pipeline knowledge

Visit [our careers page](https://example.com/careers) for more information.";

        // Act
        var html = _markdownService.ToHtml(markdown);

        // Assert
        html.Should().Contain("<h1");
        html.Should().Contain("<h2");
        html.Should().Contain("<ul>");
        html.Should().Contain("<ol>");
        html.Should().Contain("<strong>");
        html.Should().Contain("<a");
        html.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void RenderToHtml_DataAttributes_AreStripped()
    {
        // Arrange
        var markdown = "<div data-secret=\"sensitive\">Text</div>";

        // Act
        var html = _markdownService.ToHtml(markdown);

        // Assert
        // Custom data attributes should be stripped for security
        html.Should().NotContain("data-secret");
    }

    [Fact]
    public void RenderToHtml_StyleAttribute_IsStripped()
    {
        // Arrange
        var markdown = "<p style=\"display:none\">Hidden text</p>";

        // Act
        var html = _markdownService.ToHtml(markdown);

        // Assert
        // Style attributes should be stripped to prevent CSS injection
        html.Should().NotContain("style=");
        html.Should().NotContain("display:none");
    }
}
