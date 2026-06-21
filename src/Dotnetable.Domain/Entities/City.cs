using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class City
{
    public int CityID { get; set; }

    public int CountryID { get; set; }

    public int? StateID { get; set; }

    public string Title { get; set; } = null!;

    public string LanguageCode { get; set; } = null!;

    public double? Latitude { get; set; }

    public double Longitude { get; set; }

    public bool Active { get; set; }

    public virtual ICollection<CityTranslation> CityTranslations { get; set; } = new List<CityTranslation>();

    public virtual Country Country { get; set; } = null!;

    public virtual State? State { get; set; }
}
