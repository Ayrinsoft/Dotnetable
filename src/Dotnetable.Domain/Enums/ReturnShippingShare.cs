namespace Dotnetable.Domain.Enums;

/// <summary>Splits a return-shipping invoice by <see cref="ReturnShippingPayer"/>.</summary>
public static class ReturnShippingShare
{
    public static (decimal SiteShare, decimal SellerShare, decimal CustomerShare) Split(
        ReturnShippingPayer payer, decimal cost, bool siteIsSeller)
    {
        cost = cost < 0 ? 0 : decimal.Round(cost, 4);
        var half = decimal.Round(cost / 2m, 4);
        return payer switch
        {
            ReturnShippingPayer.Customer => (0, 0, cost),
            ReturnShippingPayer.SplitFiftyFifty => siteIsSeller
                ? (half, 0, half)
                : (0, half, half),
            ReturnShippingPayer.Seller => siteIsSeller
                ? (cost, 0, 0)
                : (0, cost, 0),
            ReturnShippingPayer.DropOffAtCenter => (0, 0, 0),
            _ => (0, 0, 0),
        };
    }

    public static string Label(ReturnShippingPayer payer) => payer switch
    {
        ReturnShippingPayer.Customer => "customer",
        ReturnShippingPayer.SplitFiftyFifty => "50/50",
        ReturnShippingPayer.Seller => "seller",
        ReturnShippingPayer.DropOffAtCenter => "drop-off at center",
        _ => "unset",
    };
}
