using System.Net;
using System.Text;
using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Web.Services;

/// <summary>
/// Renders a <see cref="SlideshowDto"/> into a self-contained Bootstrap carousel + lightbox HTML
/// fragment. Used both by the <c>_Slideshow</c> partial (zone-based placement, e.g. "home_top") and
/// by <see cref="ContentShortcodeProcessor"/> (the <c>[slideshow:ID]</c> shortcode embedded inside a
/// Post/Page body), so a slideshow always renders identically no matter where it is placed. The
/// carousel id and lightbox gallery group are both derived from <see cref="SlideshowDto.SlideshowID"/>
/// so two different slideshows never collide, even when both appear on the same page.
/// </summary>
public static class SlideshowHtmlRenderer
{
    public static string Render(SlideshowDto? slideshow)
    {
        if (slideshow is null || slideshow.Slides.Count == 0) return string.Empty;

        var carouselId = $"slideshow-{slideshow.SlideshowID}";
        var effectClass = slideshow.TransitionEffect == SlideTransitionEffect.Fade ? " carousel-fade" : "";
        var styleAttr = string.IsNullOrWhiteSpace(slideshow.AspectRatio)
            ? ""
            : $" style=\"--slideshow-ratio:{Encode(slideshow.AspectRatio!.Replace(":", "/"))}\"";
        var multi = slideshow.Slides.Count > 1;

        var html = new StringBuilder();
        html.Append($"<div id=\"{carouselId}\" class=\"carousel slide site-slideshow{effectClass}\"")
            .Append($" data-bs-ride=\"{(slideshow.AutoPlay ? "carousel" : "false")}\" data-bs-interval=\"{slideshow.IntervalMs}\"{styleAttr}>");

        if (slideshow.ShowDots && multi)
        {
            html.Append("<div class=\"carousel-indicators\">");
            for (var i = 0; i < slideshow.Slides.Count; i++)
            {
                html.Append($"<button type=\"button\" data-bs-target=\"#{carouselId}\" data-bs-slide-to=\"{i}\"");
                if (i == 0) html.Append(" class=\"active\" aria-current=\"true\"");
                html.Append($" aria-label=\"Slide {i + 1}\"></button>");
            }
            html.Append("</div>");
        }

        html.Append("<div class=\"carousel-inner\">");
        for (var i = 0; i < slideshow.Slides.Count; i++)
        {
            var slide = slideshow.Slides[i];
            html.Append($"<div class=\"carousel-item{(i == 0 ? " active" : "")}\">");

            var alt = Encode(slide.AltText ?? slide.Title ?? string.Empty);
            var loading = i == 0 ? "eager" : "lazy";
            var picture = string.IsNullOrEmpty(slide.MobileImageUrl)
                ? $"<img src=\"{Encode(slide.ImageUrl)}\" class=\"d-block w-100\" alt=\"{alt}\" loading=\"{loading}\" />"
                : $"<picture><source srcset=\"{Encode(slide.MobileImageUrl)}\" media=\"(max-width: 768px)\" /><img src=\"{Encode(slide.ImageUrl)}\" class=\"d-block w-100\" alt=\"{alt}\" loading=\"{loading}\" /></picture>";

            if (!string.IsNullOrWhiteSpace(slide.LinkUrl))
            {
                var target = slide.OpenInNewTab ? " target=\"_blank\" rel=\"noopener\"" : "";
                html.Append($"<a href=\"{Encode(slide.LinkUrl)}\"{target}>{picture}</a>");
            }
            else if (slideshow.EnableLightbox)
            {
                html.Append($"<a href=\"{Encode(slide.ImageUrl)}\" class=\"site-slideshow-lightbox\" data-lightbox-group=\"{carouselId}\" aria-label=\"{alt}\">{picture}</a>");
            }
            else
            {
                html.Append(picture);
            }

            var hasButton = !string.IsNullOrEmpty(slide.ButtonText) && !string.IsNullOrEmpty(slide.LinkUrl);
            if (!string.IsNullOrEmpty(slide.Title) || !string.IsNullOrEmpty(slide.Caption) || hasButton)
            {
                html.Append("<div class=\"carousel-caption d-none d-md-block\">");
                if (!string.IsNullOrEmpty(slide.Title)) html.Append($"<h5>{Encode(slide.Title)}</h5>");
                if (!string.IsNullOrEmpty(slide.Caption)) html.Append($"<p>{Encode(slide.Caption)}</p>");
                if (hasButton)
                {
                    var target = slide.OpenInNewTab ? " target=\"_blank\" rel=\"noopener\"" : "";
                    html.Append($"<a class=\"btn btn-primary btn-sm\" href=\"{Encode(slide.LinkUrl!)}\"{target}>{Encode(slide.ButtonText!)}</a>");
                }
                html.Append("</div>");
            }

            html.Append("</div>");
        }
        html.Append("</div>");

        if (slideshow.ShowArrows && multi)
        {
            html.Append($"<button class=\"carousel-control-prev\" type=\"button\" data-bs-target=\"#{carouselId}\" data-bs-slide=\"prev\">")
                .Append("<span class=\"carousel-control-prev-icon\" aria-hidden=\"true\"></span><span class=\"visually-hidden\">Previous</span></button>");
            html.Append($"<button class=\"carousel-control-next\" type=\"button\" data-bs-target=\"#{carouselId}\" data-bs-slide=\"next\">")
                .Append("<span class=\"carousel-control-next-icon\" aria-hidden=\"true\"></span><span class=\"visually-hidden\">Next</span></button>");
        }

        html.Append("</div>");
        return html.ToString();
    }

    private static string Encode(string value) => WebUtility.HtmlEncode(value);
}
