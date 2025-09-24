using System.ComponentModel.DataAnnotations;

namespace Dotnetable.Shared.DTO.Public;

public class GridviewRequest
{
    [Required(ErrorMessage ="Send {0}")]
    [Range(0, int.MaxValue, ErrorMessage = "Send correct number")]
    public int SkipCount { get; set; }

    [Required(ErrorMessage = "Send {0}")]
    [Range(1, int.MaxValue, ErrorMessage = "Send correct number")]
    public int TakeCount { get; set; }

    public string OrderbyParams { get; set; }
}
