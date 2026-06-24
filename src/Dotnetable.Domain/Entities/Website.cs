using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Website
{
    public int WebsiteID { get; set; }

    public string TradeName { get; set; } = null!;

    public string WebsiteAddress { get; set; } = null!;

    public Guid AuthCode { get; set; }

    public bool Active { get; set; }

    public string Manager { get; set; } = null!;

    public string Mobile { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateOnly RegisterDate { get; set; }

    public bool AllowAllIP { get; set; }

    public string DefaultLanguageCode { get; set; } = null!;

    public byte WebsiteType { get; set; }

    /// <summary>
    /// show in title of pages
    /// </summary>
    public string BrandName { get; set; } = null!;

    public int? LogoFileID { get; set; }

    public int? FaveIconFileID { get; set; }

    public virtual FileRecord? FaveIconFile { get; set; }

    public virtual ICollection<FileAlbum> FileAlbums { get; set; } = new List<FileAlbum>();

    public virtual ICollection<FileTag> FileTags { get; set; } = new List<FileTag>();

    public virtual ICollection<Language> Languages { get; set; } = new List<Language>();

    public virtual FileRecord? LogoFile { get; set; }

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();

    public virtual ICollection<WebsiteIP> WebsiteIPs { get; set; } = new List<WebsiteIP>();

    public virtual ICollection<WebsiteScript> WebsiteScripts { get; set; } = new List<WebsiteScript>();

    public virtual ICollection<WebsiteSeoSetting> WebsiteSeoSettings { get; set; } = new List<WebsiteSeoSetting>();

    public virtual ICollection<WebsiteSocialLink> WebsiteSocialLinks { get; set; } = new List<WebsiteSocialLink>();

    public virtual ICollection<WebstieStorageSetting> WebstieStorageSettings { get; set; } = new List<WebstieStorageSetting>();

    public virtual ICollection<ContactUsMessage> ContactUsMessages { get; set; } = new List<ContactUsMessage>();

    public virtual ICollection<EmailSetting> EmailSettings { get; set; } = new List<EmailSetting>();
}
