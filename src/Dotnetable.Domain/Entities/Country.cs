using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Country
{
    public int CountryID { get; set; }

    public string CountryCode { get; set; } = null!;

    public string LanguageCode { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string PhonePerfix { get; set; } = null!;

    public virtual ICollection<City> Cities { get; set; } = new List<City>();

    public virtual ICollection<CountryTranslation> CountryTranslations { get; set; } = new List<CountryTranslation>();

    public virtual ICollection<State> States { get; set; } = new List<State>();
}
