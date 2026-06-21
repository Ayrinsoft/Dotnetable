using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class StateTranslation
{
    public int StateTranslationID { get; set; }

    public string Tile { get; set; } = null!;

    public string LanguageCode { get; set; } = null!;

    public int StateID { get; set; }

    public virtual State State { get; set; } = null!;
}
