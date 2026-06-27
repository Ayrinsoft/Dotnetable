using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Web.Controllers;

/// <summary>
/// Public blog. Posts are placeholder content for now; swap <see cref="SamplePosts"/>
/// for an API-backed source when the blog content service is ready.
/// </summary>
public class BlogController : Controller
{
    /// <summary>A blog post for display. Replace with an API DTO once content is live.</summary>
    public sealed record PostView(
        string Slug,
        string Title,
        string Category,
        string Excerpt,
        DateOnly Date,
        string Author,
        string Body);

    /// <summary>Demo posts used until the blog is wired to real content.</summary>
    public static readonly IReadOnlyList<PostView> SamplePosts =
    [
        new("getting-started", "Getting started with our platform", "Guides",
            "A quick tour of everything you need to launch your first project with confidence.",
            new DateOnly(2026, 6, 20), "The Team",
            "Welcome aboard! In this post we walk through the essentials so you can hit the ground running."),
        new("design-trends-2026", "Design trends to watch in 2026", "Design",
            "From bold typography to immersive motion, here are the trends shaping the web this year.",
            new DateOnly(2026, 6, 12), "The Team",
            "Design moves fast. We rounded up the directions we think matter most for the year ahead."),
        new("scaling-your-app", "Scaling your app without the headaches", "Engineering",
            "Practical tips for growing your product smoothly as traffic and demand increase.",
            new DateOnly(2026, 6, 3), "The Team",
            "Scaling is a journey, not a switch. Here is how to plan for growth from day one."),
        new("seo-fundamentals", "SEO fundamentals every site needs", "Marketing",
            "The foundational steps that help search engines — and customers — find you.",
            new DateOnly(2026, 5, 25), "The Team",
            "Search visibility starts with the basics. Get these right before anything else."),
    ];

    public IActionResult Index() => View(SamplePosts);

    public IActionResult Post(string slug)
    {
        var post = SamplePosts.FirstOrDefault(p =>
            string.Equals(p.Slug, slug, StringComparison.OrdinalIgnoreCase));
        return post is null ? NotFound() : View(post);
    }
}
