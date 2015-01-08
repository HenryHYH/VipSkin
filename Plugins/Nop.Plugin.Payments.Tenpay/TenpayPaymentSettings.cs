using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.Tenpay
{
    public class TenpayPaymentSettings : ISettings
    {
        public string Key { get; set; }
        public string Partner { get; set; }
        public decimal AdditionalFee { get; set; }
    }
}
