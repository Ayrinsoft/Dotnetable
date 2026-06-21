using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ContactUsMessage
{
    public int ContactUsMessagesID { get; set; }

    public string SenderName { get; set; } = null!;

    public string EmailAddress { get; set; } = null!;

    public string CellphoneNumber { get; set; } = null!;

    public string MessageSubject { get; set; } = null!;

    public string MessageBody { get; set; } = null!;

    public bool Archive { get; set; }

    public DateTime LogTime { get; set; }

    public string SenderIPAddress { get; set; } = null!;
}
