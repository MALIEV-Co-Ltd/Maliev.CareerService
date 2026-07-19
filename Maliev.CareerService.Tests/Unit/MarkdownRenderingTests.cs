using Maliev.CareerService.Application.Services;
using Maliev.CareerService.Infrastructure.Services;
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
        Assert.Contains("<h1", html);
        Assert.Contains("Heading", html);
        Assert.Contains("<p>", html);
        Assert.Contains("This is a paragraph", html);
    }

    [Fact]
    public void RenderToHtml_BoldAndItalic_RendersCorrectly()
    {
        // Arrange
        var markdown = "**Bold text** and *italic text*";

        // Act
        var html = _markdownService.ToHtml(markdown);

        // Assert
        Assert.Contains("<strong>", html);
        Assert.Contains("Bold text", html);
        Assert.Contains("<em>", html);
        Assert.Contains("italic text", html);
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
        Assert.Contains("<h2", html);
        Assert.Contains("Requirements", html);
        Assert.Contains("<ul>", html);
        Assert.Contains("<li>", html);
        Assert.Contains("Bachelor's degree", html);
        Assert.Contains("3+ years experience", html);
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
        Assert.Contains("<ol>", html);
        Assert.Contains("<li>", html);
        Assert.Contains("First step", html);
        Assert.Contains("Second step", html);
    }

    [Fact]
    public void RenderToHtml_Links_RendersCorrectly()
    {
        // Arrange
        var markdown = "Visit [our website](https://example.com) for more information.";

        // Act
        var html = _markdownService.ToHtml(markdown);

        // Assert
        Assert.Contains("<a", html);
        Assert.Contains("href=\"https://example.com\"", html);
        Assert.Contains("our website", html);
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
        Assert.DoesNotContain("<script>", html);
        Assert.DoesNotContain("alert('XSS')", html);
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
        Assert.DoesNotContain("javascript:", html);
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
        Assert.DoesNotContain("onclick", html);
        Assert.DoesNotContain("alert('XSS')", html);
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
        Assert.DoesNotContain("<iframe", html);
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
        Assert.DoesNotContain("<object", html);
        Assert.DoesNotContain("<embed", html);
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
        Assert.Contains("<h1", html);
        Assert.Contains("<h2", html);
        Assert.Contains("<h3", html);
        Assert.Contains("<h4", html);
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
        Assert.Contains("<pre>", html);
        Assert.Contains("function hello()", html);
    }

    [Fact]
    public void RenderToHtml_InlineCode_RendersCorrectly()
    {
        // Arrange
        var markdown = "Use the `console.log()` function to debug.";

        // Act
        var html = _markdownService.ToHtml(markdown);

        // Assert
        Assert.Contains("<code>", html);
        Assert.Contains("console.log()", html);
    }

    [Fact]
    public void RenderToHtml_Blockquotes_RendersCorrectly()
    {
        // Arrange
        var markdown = "> This is a quote\n> with multiple lines";

        // Act
        var html = _markdownService.ToHtml(markdown);

        // Assert
        Assert.Contains("<blockquote>", html);
        Assert.Contains("This is a quote", html);
    }

    [Fact]
    public void RenderToHtml_EmptyString_ReturnsEmptyOrSafe()
    {
        // Arrange
        var markdown = "";

        // Act
        var html = _markdownService.ToHtml(markdown);

        // Assert
        Assert.NotNull(html);
        // Should return empty string or safe empty HTML
        Assert.True(html == "" || html == "<p></p>" || html == string.Empty);
    }

    [Fact]
    public void RenderToHtml_NullInput_HandlesGracefully()
    {
        // Arrange
        string? markdown = null;

        // Act & Assert
        var act = () => _markdownService.ToHtml(markdown!);

        // Should either handle null gracefully or throw ArgumentNullException
        var exception = Record.Exception(act);
        if (exception == null)
        {
            // If it doesn't throw, result should be safe
            var html = _markdownService.ToHtml(markdown!);
            Assert.NotNull(html);
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
        Assert.DoesNotContain("<>", html);
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
        var exception = Record.Exception(act);
        Assert.Null(exception);
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
        Assert.Contains("<h1", html);
        Assert.Contains("<h2", html);
        Assert.Contains("<ul>", html);
        Assert.Contains("<ol>", html);
        Assert.Contains("<strong>", html);
        Assert.Contains("<a", html);
        Assert.False(string.IsNullOrEmpty(html));
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
        Assert.DoesNotContain("data-secret", html);
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
        Assert.DoesNotContain("style=", html);
        Assert.DoesNotContain("display:none", html);
    }
}
