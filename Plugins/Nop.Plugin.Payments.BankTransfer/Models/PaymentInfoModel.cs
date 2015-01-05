using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.BankTransfer.Models
{
    public class PaymentInfoModel : BaseNopModel
    {
        public string DescriptionText { get; set; }
    }
}