using System.Collections.Generic;

namespace Dotnetable.Application.Authorization;

/// <summary>
/// Titles of the access levels (policies) seeded for every website on first setup / new site create.
/// They are looked up by these exact titles, so keep them stable.
/// </summary>
public static class DefaultPolicies
{
    /// <summary>Full-access policy for the super administrator of the master website (WebsiteID 1).</summary>
    public const string Administrators = "Administrators";

    /// <summary>
    /// Default policy assigned to website customers — covers sign-in, general access,
    /// commenting and purchasing. New self-registered members receive this policy.
    /// </summary>
    public const string Users = "Users";

    /// <summary>Warehouse operators: pick, receive, QC returns, post stock documents.</summary>
    public const string WarehouseStaff = "Warehouse staff";

    /// <summary>Order desk + payments verify (no refunds / accounting post).</summary>
    public const string SalesStaff = "Sales staff";

    /// <summary>Finance: ledger, refunds, wallets, bank accounts, settlements, accounting reports.</summary>
    public const string FinanceStaff = "Finance staff";

    /// <summary>HR + payroll (view/run/approve).</summary>
    public const string HrStaff = "HR staff";

    /// <summary>
    /// Staff policies seeded for every website (not Administrators / Users).
    /// Key = policy title; value = role keys to grant.
    /// </summary>
    public static IReadOnlyList<(string Title, IReadOnlyList<string> RoleKeys)> StaffTemplates { get; } =
    [
        (WarehouseStaff, new[]
        {
            RoleKeys.WarehouseView, RoleKeys.WarehouseReceive, RoleKeys.WarehouseIssue,
            RoleKeys.WarehouseApprove, RoleKeys.WarehousePost,
            RoleKeys.InventoryView, RoleKeys.OrdersView, RoleKeys.SuppliersView,
            RoleKeys.TasksView,
        }),
        (SalesStaff, new[]
        {
            RoleKeys.OrdersView, RoleKeys.OrdersEdit,
            RoleKeys.PaymentsView, RoleKeys.PaymentsVerify,
            RoleKeys.ClientsView, RoleKeys.ProductsView,
            RoleKeys.PriceListsView, RoleKeys.PriceListsInsert, RoleKeys.PriceListsEdit,
            RoleKeys.InventoryView, RoleKeys.ShippingView,
            RoleKeys.SupportView, RoleKeys.SupportEdit,
            RoleKeys.WalletsView,
            RoleKeys.TasksView,
        }),
        (FinanceStaff, new[]
        {
            RoleKeys.PaymentsView, RoleKeys.PaymentsVerify, RoleKeys.PaymentsRefund,
            RoleKeys.OrdersView, RoleKeys.WalletsView, RoleKeys.WalletsApprove, RoleKeys.WalletsAdjust,
            RoleKeys.BankAccountsView, RoleKeys.BankAccountsInsert, RoleKeys.BankAccountsEdit,
            RoleKeys.SettlementsView, RoleKeys.SettlementsEdit,
            RoleKeys.AccountingView, RoleKeys.AccountingEdit, RoleKeys.AccountingPost, RoleKeys.AccountingReport,
            RoleKeys.CurrencyView, RoleKeys.TaxView,
            RoleKeys.InventoryView, RoleKeys.WarehouseView,
            RoleKeys.TasksView,
        }),
        (HrStaff, new[]
        {
            RoleKeys.HrView, RoleKeys.HrEdit,
            RoleKeys.PayrollView, RoleKeys.PayrollRun, RoleKeys.PayrollApprove,
            RoleKeys.AccountingView, RoleKeys.AccountingReport,
            RoleKeys.TasksView,
        }),
    ];
}
