using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.Tenpay.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Payments.Tenpay.Key")]
        public string Key { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Tenpay.Partner")]
        public string Partner { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Tenpay.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
    }
}