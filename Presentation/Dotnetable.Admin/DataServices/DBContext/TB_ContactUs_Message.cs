using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_ContactUs_Message
{
    public int ContactUsMessagesID { get; set; }

    public string SenderName { get; set; }

    public string EmailAddress { get; set; }

    public string CellphoneNumber { get; set; }

    public string MessageSubject { get; set; }

    public string MessageBody { get; set; }

    public bool Archive { get; set; }

    public DateTime LogTime { get; set; }

    public string SenderIPAddress { get; set; }
}
