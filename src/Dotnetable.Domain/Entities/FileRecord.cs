using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class FileRecord
{
    public int FileRecordID { get; set; }

    public int WebsiteStorageSettingsID { get; set; }

    public short StorageProvider { get; set; }

    public string? StoragePath { get; set; }

    public string? CNDUrl { get; set; }

    public string OriginalFileName { get; set; } = null!;

    public string StoredFileName { get; set; } = null!;

    public string MimeType { get; set; } = null!;

    public int FileSizeKB { get; set; }

    public string? MetadataJSON { get; set; }

    public string? AltText { get; set; }

    public string? Title { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime UploadDate { get; set; }

    public string? ThumbnailStorage { get; set; }

    public string? ThumbnailCDN { get; set; }

    public byte FileCategory { get; set; }

    public string? CDNFileCode { get; set; }

    public int? FileFolderID { get; set; }

    public int WebsiteID { get; set; }

    public int? UploaderMemberID { get; set; }

    public int? WebsiteClientID { get; set; }

    public virtual ICollection<Bank> Banks { get; set; } = new List<Bank>();

    public virtual ICollection<Brand> Brands { get; set; } = new List<Brand>();

    public virtual FileFolder? FileFolder { get; set; }

    public virtual ICollection<FileRecordTag> FileRecordTags { get; set; } = new List<FileRecordTag>();

    public virtual ICollection<MediaSetItem> MediaSetItemFiles { get; set; } = new List<MediaSetItem>();

    public virtual ICollection<MediaSetItem> MediaSetItemVideoThumbnailFiles { get; set; } = new List<MediaSetItem>();

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<ShippingMethod> ShippingMethods { get; set; } = new List<ShippingMethod>();

    public virtual ICollection<SlideshowSlide> SlideshowSlideFiles { get; set; } = new List<SlideshowSlide>();

    public virtual ICollection<SlideshowSlide> SlideshowSlideMobileFiles { get; set; } = new List<SlideshowSlide>();

    public virtual Member? UploaderMember { get; set; }

    public virtual ICollection<Vendor> Vendors { get; set; } = new List<Vendor>();

    public virtual Website Website { get; set; } = null!;

    public virtual WebsiteClient? WebsiteClient { get; set; }

    public virtual ICollection<WebsiteClient> WebsiteClients { get; set; } = new List<WebsiteClient>();

    public virtual ICollection<Website> WebsiteFaveIconFiles { get; set; } = new List<Website>();

    public virtual ICollection<Website> WebsiteLogoFiles { get; set; } = new List<Website>();

    public virtual WebsiteStorageSetting WebsiteStorageSettings { get; set; } = null!;

    public virtual ICollection<WebsiteWatermarkSetting> WebsiteWatermarkSettings { get; set; } = new List<WebsiteWatermarkSetting>();
}
