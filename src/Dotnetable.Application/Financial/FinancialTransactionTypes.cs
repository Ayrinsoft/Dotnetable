namespace Dotnetable.Application.Financial;

/// <summary>
/// Well-known <c>FinancialLedgerEntry.TransactionType</c> codes.
/// Not exhaustive — callers may post any non-empty string for future kinds.
/// </summary>
public static class FinancialTransactionTypes
{
    public const string CustomerPayment = "CustomerPayment";
    public const string CustomerRefund = "CustomerRefund";
    public const string AdditionalCharge = "AdditionalCharge";
    public const string OrderShipping = "OrderShipping";
    public const string OrderTax = "OrderTax";
    public const string OrderDiscount = "OrderDiscount";
    public const string OrderMarkup = "OrderMarkup";
    public const string OrderLineRevenue = "OrderLineRevenue";
    public const string OrderLineCost = "OrderLineCost";
    public const string OrderLineProfit = "OrderLineProfit";
    public const string VendorSettlement = "VendorSettlement";
    public const string VendorCreditGrant = "VendorCreditGrant";
    public const string VendorCreditSale = "VendorCreditSale";
    public const string VendorCreditRefund = "VendorCreditRefund";
    public const string WalletCredit = "WalletCredit";
    public const string WalletDebit = "WalletDebit";
    public const string SettlementPaid = "SettlementPaid";
    public const string Adjustment = "Adjustment";
    public const string Manual = "Manual";
}

/// <summary>Values for <c>FinancialLedgerEntry.Flow</c>.</summary>
public static class FinancialFlow
{
    public const byte In = 1;
    public const byte Out = 2;
    /// <summary>Analytical component (shipping/markup/profit breakdown) — not a separate cash movement.</summary>
    public const byte Component = 3;
}
