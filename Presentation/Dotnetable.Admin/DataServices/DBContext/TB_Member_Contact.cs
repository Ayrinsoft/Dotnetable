using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_Member_Contact
{
    public int MemberContactID { get; set; }

    public int MemberID { get; set; }

    public string Address { get; set; }

    public string LanguageCode { get; set; }

    public string PhoneNumber { get; set; }

    public string CellphoneNumber { get; set; }

    public string HomeOwnerName { get; set; }

    public string PointLatitude { get; set; }

    public string PointLongitude { get; set; }

    public bool Active { get; set; }

    public bool DefaultContact { get; set; }

    public string PostalCode { get; set; }

    public int CityID { get; set; }

    public virtual TB_City City { get; set; }

    public virtual TB_Member Member { get; set; }
}
