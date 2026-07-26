using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class WebsiteClient
{
    public int WebsiteClientID { get; set; }

    public int WebsiteID { get; set; }

    public int? AvatarID { get; set; }

    public string? Email { get; set; }

    public string? Cellphone { get; set; }

    public string? CountryCode { get; set; }

    public string? Password { get; set; }

    public bool Active { get; set; }

    public DateOnly RegisterDate { get; set; }

    public bool? Gender { get; set; }

    public string? Givenname { get; set; }

    public string? Surname { get; set; }

    public Guid HashKey { get; set; }

    public byte ClientLevel { get; set; }

    public virtual FileRecord? Avatar { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<ClientBankAccount> ClientBankAccounts { get; set; } = new List<ClientBankAccount>();

    public virtual ClientWallet? ClientWallet { get; set; }

    public virtual ICollection<ClientWalletWithdrawal> ClientWalletWithdrawals { get; set; } = new List<ClientWalletWithdrawal>();

    public virtual ICollection<CouponRedemption> CouponRedemptions { get; set; } = new List<CouponRedemption>();

    public virtual ICollection<FileRecord> FileRecords { get; set; } = new List<FileRecord>();

    public virtual ICollection<FormResponse> FormResponses { get; set; } = new List<FormResponse>();

    public virtual ICollection<DigitalAccessLog> DigitalAccessLogs { get; set; } = new List<DigitalAccessLog>();

    public virtual ICollection<OrderDigitalAsset> OrderDigitalAssets { get; set; } = new List<OrderDigitalAsset>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<ProductAnswer> ProductAnswers { get; set; } = new List<ProductAnswer>();

    public virtual ICollection<ProductQuestion> ProductQuestions { get; set; } = new List<ProductQuestion>();

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

    public virtual ICollection<SupportSession> SupportSessions { get; set; } = new List<SupportSession>();

    public virtual Website Website { get; set; } = null!;

    public virtual ICollection<WebsiteClientAddress> WebsiteClientAddresses { get; set; } = new List<WebsiteClientAddress>();

    public virtual ICollection<WebsiteClientForgetPassword> WebsiteClientForgetPasswords { get; set; } = new List<WebsiteClientForgetPassword>();

    public virtual Wishlist? Wishlist { get; set; }
}
