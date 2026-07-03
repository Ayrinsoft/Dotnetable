using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class MenuItemTranslation
{
    public int MenuItemTranslationID { get; set; }

    public int MenuItemID { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string Title { get; set; } = null!;

    public virtual MenuItem MenuItem { get; set; } = null!;
}
