namespace Dotnetable.Shared.DTO.Public;

public class GoogleRecaptchaResponse
{
    public bool success { get; set; }
    public double score { get; set; }
    public string action { get; set; }
    public DateTime challenge_ts { get; set; }
    public string hostname { get; set; }
}
