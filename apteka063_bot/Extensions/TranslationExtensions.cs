using apteka063.Database;
//using apteka063.Resources;


namespace apteka063.Extensions
{
    internal static class TranslationConverter
    {
        internal static string ToLocaleString(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Filling => Resources.Translation.Filling,
                OrderStatus.NeedOrderConfirmation => Resources.Translation.NeedOrderConfirmation,
                OrderStatus.NeedUserPhone => Resources.Translation.NeedUserPhone,
                OrderStatus.NeedContactPhone => Resources.Translation.NeedPhone,
                OrderStatus.NeedContactName => Resources.Translation.NeedContactName,
                OrderStatus.NeedContactAddress => Resources.Translation.NeedAddress,
                OrderStatus.NeedOrderComment => Resources.Translation.NeedOrderComment,
                OrderStatus.Canceled => Resources.Translation.Canceled,
                OrderStatus.InProgress => Resources.Translation.InProgress,
                OrderStatus.Declined => Resources.Translation.Declined,
                OrderStatus.Closed => Resources.Translation.Closed,
                _ => "LocaleString N/A",
            };
        }
        internal static string ToLocaleString(OrderType status)
        {
            return status switch
            {
                OrderType.Pills => Resources.Translation.Pills,
                OrderType.Humaid => Resources.Translation.Humaid,
                OrderType.Transport => Resources.Translation.Transport,
                OrderType.N_A => "N/A",
                _ => "LocaleString N/A",
            };
        }
        internal static OrderType ToOrderType(string type)
        {
            if (type == Resources.Translation.Pills)
            {
                return OrderType.Pills;
            }
            else if (type == Resources.Translation.Humaid)
            {
                return OrderType.Humaid;
            }
            else if (type == Resources.Translation.Transport)
            {
                return OrderType.Transport;
            }
            else
            {
                return OrderType.N_A;
            }
        }
    }
}
