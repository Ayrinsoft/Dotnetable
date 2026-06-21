using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class State
{
    public int StateID { get; set; }

    public int CountryID { get; set; }

    public string Tile { get; set; } = null!;

    public string LanguageCode { get; set; } = null!;

    public bool Active { get; set; }

    public virtual ICollection<City> Cities { get; set; } = new List<City>();

    public virtual Country Country { get; set; } = null!;

    public virtual ICollection<StateTranslation> StateTranslations { get; set; } = new List<StateTranslation>();
}
