using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class VendorTranslation
{
    public int VendorTranslationID { get; set; }

    public int VendorID { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual Vendor Vendor { get; set; } = null!;
}
