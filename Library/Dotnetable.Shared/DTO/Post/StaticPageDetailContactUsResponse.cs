namespace Dotnetable.Shared.DTO.Post;

public class StaticPageDetailContactUsResponse
{
    public Dictionary<string, string> PhoneNumber { get; set; }
    public Dictionary<string, string> Address { get; set; }
    public string HTMLBody { get; set; }
    public Dictionary<string, string> EmailAddress { get; set; }
    public Dictionary<string, string> Faxnumber { get; set; }
    public List<WorkingHours> WorkHours { get; set; }
    public string MapLocationLatitude { get; set; }
    public string MapLocationLongitude { get; set; }

    public class WorkingHours
    {
        public string FromHour { get; set; }
        public string ToHour { get; set; }
        public string WeekDays { get; set; }
    }
}
