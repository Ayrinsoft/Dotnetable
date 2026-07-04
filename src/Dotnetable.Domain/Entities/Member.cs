using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Member
{
    public int MemberID { get; set; }

    public bool Active { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string CellphoneNumber { get; set; } = null!;

    public string CountryCode { get; set; } = null!;

    public DateOnly RegisterDate { get; set; }

    public string Givenname { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public int? AvatarID { get; set; }

    public Guid HashKey { get; set; }

    public int PolicyID { get; set; }

    public bool? Gender { get; set; }

    public int WebsiteID { get; set; }

    public virtual FileRecord? Avatar { get; set; }

    public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();

    public virtual ICollection<ClientWalletTransaction> ClientWalletTransactions { get; set; } = new List<ClientWalletTransaction>();

    public virtual ICollection<ClientWalletWithdrawal> ClientWalletWithdrawals { get; set; } = new List<ClientWalletWithdrawal>();

    public virtual ICollection<EmailSubscribe> EmailSubscribes { get; set; } = new List<EmailSubscribe>();

    public virtual ICollection<FileRecord> FileRecords { get; set; } = new List<FileRecord>();

    public virtual ICollection<MemberForgetPassword> MemberForgetPasswords { get; set; } = new List<MemberForgetPassword>();

    public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Page> Pages { get; set; } = new List<Page>();

    public virtual ICollection<PaymentRefund> PaymentRefunds { get; set; } = new List<PaymentRefund>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Policy Policy { get; set; } = null!;

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<Settlement> SettlementApprovedByMembers { get; set; } = new List<Settlement>();

    public virtual ICollection<Settlement> SettlementCreatedByMembers { get; set; } = new List<Settlement>();

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

    public virtual Website Website { get; set; } = null!;
}
